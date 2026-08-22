using System;
using System.Collections.Generic;
using System.IO;
using TrueSync;

namespace ET
{
    [FriendOf(typeof(Room))]
    [FriendOf(typeof(LSCombatComponent))]   // 怪物工厂写 HurtAnimId（ET0002）
    [FriendOf(typeof(LSCollisionComponent))]   // 碰撞矩阵初始化写字段（ET0002）
    public static partial class RoomSystem
    {
        public static Room Room(this Entity entity)
        {
            return entity.IScene as Room;
        }

        /// <summary>
        /// 初始化房间世界。mapId=0 或未注册 = 空地（无怪物无碰撞，回放/旧流程走这条）。
        /// 怪物/碰撞全部配置驱动（MapDefinition 第七类内容，03 文档 §3.3）——两端跑同一份 Init，Id 序天然一致。
        /// </summary>
        public static void Init(this Room self, List<LockStepUnitInfo> unitInfos, long startTime, int mapId = 0, int frame = -1)
        {
            self.StartTime = startTime;
            self.AuthorityFrame = frame;
            self.PredictionFrame = frame;
            self.Replay.UnitInfos = unitInfos;
            self.FrameBuffer = new FrameBuffer(frame);
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);
            LSWorld lsWorld = self.LSWorld;
            lsWorld.Frame = frame + 1;
            lsWorld.AddComponent<LSUnitComponent>();
            lsWorld.AddComponent<LSBulletComponent>();   // 投射物容器
            lsWorld.AddComponent<LSAreaComponent>();     // 区域效果容器（火圈等）
            MapDefinition mapDef = MapLoader.Get(mapId);
            InitCollision(lsWorld, mapDef);              // 碰撞矩阵（先建——组件 Id 序 = 快照一致性）
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                LockStepUnitInfo unitInfo = unitInfos[i];
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
                Log.Info($"[Room] 玩家单位创建：PlayerId={unitInfo.PlayerId}");
            }

            // 按地图配置创建怪物（替代原硬编码测试桩；怪物不进 PlayerIds、不加 LSInputComponent）
            if (mapDef?.MonsterAiIds != null && false)   // TODO: 碰撞调试，暂时禁用怪物
            {
                for (int i = 0; i < mapDef.MonsterAiIds.Length; i++)
                {
                    CreateMonster(lsWorld, mapDef.MonsterAiIds[i], mapDef.MonsterSpawns[i], mapDef.MonsterForwards[i]);
                }
            }
        }

        /// <summary>
        /// 碰撞矩阵初始化：读 MapTileLayoutCache（视图层已解析 tile_layout.json）→ LSCollisionComponent（进快照）。
        /// 缓存未命中（地图没配 TileLayoutPath / json 缺失）= 空地无碰撞。
        /// </summary>
        private static void InitCollision(LSWorld lsWorld, MapDefinition mapDef)
        {
            if (mapDef?.TileLayoutPath == null) return;
            TileLayoutData layout = MapTileLayoutCache.Get(mapDef.TileLayoutPath);
            if (layout == null)
            {
                Log.Warning($"[Room] 瓦片布局未加载：{mapDef.TileLayoutPath}——空地无碰撞");
                return;
            }

            LSCollisionComponent collision = lsWorld.AddComponent<LSCollisionComponent>();
            collision.GridWidth = layout.gridWidth;
            collision.GridHeight = layout.gridHeight;
            // CellSize 对齐贴图实际世界尺寸（不是 DNF 逻辑 80px——贴图分辨率 ≠ 网格逻辑像素）
            collision.CellSize = layout.visualWidth / collision.GridWidth;
            Log.Info($"[Collision] visualWidth={layout.visualWidth} visualHeight={layout.visualHeight} → CellSize={collision.CellSize}");
            collision.PassGrid = new byte[layout.gridWidth * layout.gridHeight];
            for (int i = 0; i < collision.PassGrid.Length; i++)
            {
                // DNF [pass type] 原值直译：'2'=可走(1)，其余('0' 等)=阻挡(0)；串短于矩阵时尾部全阻挡
                collision.PassGrid[i] = i < layout.passTypes.Length && layout.passTypes[i] == '2' ? (byte)1 : (byte)0;
            }

            // 坐标对齐：瓦片贴图以中心对齐世界原点（Sprite pivot=0.5, localPosition=0）
            // → 网格 col=0（左边缘）的世界 x = -gridWidth×CellSize/2
            collision.OriginX = -(FP)collision.GridWidth * collision.CellSize / 2;

            // z 对齐：找可行走带的行范围中线 → 让 z=0 落在行走带中间（单位战斗的 z 活动区间）
            int minRow = -1, maxRow = -1;
            for (int row = 0; row < collision.GridHeight && minRow < 0; row++)
                for (int col = 0; col < collision.GridWidth; col++)
                    if (collision.PassGrid[row * collision.GridWidth + col] == 1) { minRow = row; break; }
            for (int row = collision.GridHeight - 1; row >= 0 && maxRow < 0; row--)
                for (int col = 0; col < collision.GridWidth; col++)
                    if (collision.PassGrid[row * collision.GridWidth + col] == 1) { maxRow = row; break; }
            int centerRow = minRow < 0 ? collision.GridHeight / 2 : (minRow + maxRow) / 2;
            // row = (z - OriginZ) / CellSize → z=0 时 row=centerRow → OriginZ = -centerRow × CellSize
            collision.OriginZ = -(FP)centerRow * collision.CellSize;

            Log.Info($"[Room] 碰撞矩阵就绪：{collision.GridWidth}x{collision.GridHeight} 格，cell={collision.CellSize}，" +
                     $"origin=({collision.OriginX},{collision.OriginZ})，可行走行 {minRow}~{maxRow} 中线={centerRow}");
            // 诊断：碰撞框的世界范围 vs 单位出生点（排查对齐）
            FP leftX = collision.OriginX;
            FP rightX = collision.OriginX + (FP)collision.GridWidth * collision.CellSize;
            FP frontZ = collision.OriginZ;   // row=gridHeight-1（最前/最近摄像机）
            FP backZ = collision.OriginZ + (FP)collision.GridHeight * collision.CellSize;  // row=0（最后）
            Log.Info($"[Room] 碰撞世界范围：x[{leftX:F2}~{rightX:F2}] z[{frontZ:F2}~{backZ:F2}]，" +
                     $"玩家出生(0,0,0)→格子(col={(int)((0-leftX)/collision.CellSize)},row={(int)((0-frontZ)/collision.CellSize)})");
        }

        /// <summary>按地图配置创建怪物（原测试桩抽取参数化；组件挂载序 = LSUpdate 执行序，勿动）</summary>
        private static void CreateMonster(LSWorld lsWorld, int monsterAiId, TSVector position, TSVector forward)
        {
            LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
            LSUnit monster = lsUnitComponent.AddChild<LSUnit>();
            monster.Position = position;
            monster.Forward = forward;   // 出生朝向（配置直译）——攻击盒采样/发弹方向都吃朝向
            monster.AddComponent<LSAnimComponent>().Play(AnimId.Idle);

            // 数值（HP）
            var monsterNum = monster.AddComponent<LSNumericComponent>();
            monsterNum.Set(NumericType.HpBase, 500);
            monsterNum.Set(NumericType.MaxHpBase, 500);

            // 战斗状态（默认动画 Stay=Idle、受击 Damage、击倒落地 Down）。先于 Hitbox 挂
            monster.AddComponent<LSCombatComponent, int>(AnimId.Idle);
            monster.GetComponent<LSCombatComponent>().HurtAnimId = AnimId.Hurt;
            monster.GetComponent<LSCombatComponent>().DownAnimId = AnimId.MonsterDown;

            monster.AddComponent<LSFlightComponent>();
            monster.AddComponent<LSBuffComponent>();

            // 技能（怪物走玩家同款 Cast 框架：CD/硬直门禁/帧驱动攻击盒/HitReaction 全复用）
            monster.AddComponent<LSSkillComponent>();
            monster.AddComponent<LSCastComponent>();

            // 命中盒组件（受击盒采样 + 攻击判定帧驱动）
            monster.AddComponent<LSHitboxComponent>();

            // AI（行为机 + 第六类内容配置驱动，见 02 文档 §10.4）
            monster.AddComponent<LSMonsterAIComponent, int>(monsterAiId);
            Log.Info($"[Monster] unit{monster.Id} @ {monster.Position} AI={monsterAiId}（地图配置驱动）");
        }

        public static void Update(this Room self, OneFrameInputs oneFrameInputs)
        {
            LSWorld lsWorld = self.LSWorld;
            // 设置输入到每个LSUnit身上
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameInputs.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                if (lsUnit == null)
                {
                    Log.Warning($"[Room] 输入 key={kv.Key} 找不到单位，丢弃该输入");
                    continue;
                }
                LSInputComponent lsInputComponent = lsUnit.GetComponent<LSInputComponent>();
                if (lsInputComponent == null)
                {
                    // 输入 key 撞上了非玩家单位（如怪物局部 Id）——绝不把输入喂给无输入组件的单位
                    Log.Warning($"[Room] 输入 key={kv.Key} 命中 unit{lsUnit.Id}（无 LSInputComponent，非玩家单位？），丢弃");
                    continue;
                }
                if (lsUnit.Id != kv.Key)
                {
                    Log.Warning($"[Room] 输入 key={kv.Key} 解析到 unit{lsUnit.Id}（Id 不匹配！）");
                }
                lsInputComponent.LSInput = kv.Value;
                if (kv.Value.Button != 0)
                    Log.Info($"[Room] 帧{lsWorld.Frame} 输入 key={kv.Key} → unit{lsUnit.Id} button={kv.Value.Button}");
            }
            
            if (!self.IsReplay)
            {
                // 保存当前帧场景数据
                self.SaveLSWorld();
                self.Record(self.LSWorld.Frame);
            }

            lsWorld.Update();
        }
        
        public static LSWorld GetLSWorld(this Room self, int sceneType, int frame)
        {
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            LSWorld lsWorld = MemoryPackHelper.Deserialize(typeof (LSWorld), memoryBuffer) as LSWorld;
            lsWorld.SceneType = sceneType;
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            return lsWorld;
        }

        private static void SaveLSWorld(this Room self)
        {
            int frame = self.LSWorld.Frame;
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);
            
            MemoryPackHelper.Serialize(self.LSWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);

            long hash = memoryBuffer.GetBuffer().Hash(0, (int) memoryBuffer.Length);
            
            self.FrameBuffer.SetHash(frame, hash);
        }

        // 记录需要存档的数据
        public static void Record(this Room self, int frame)
        {
            if (frame > self.AuthorityFrame)
            {
                return;
            }
            OneFrameInputs oneFrameInputs = self.FrameBuffer.FrameInputs(frame);
            OneFrameInputs saveInput = OneFrameInputs.Create();
            oneFrameInputs.CopyTo(saveInput);
            self.Replay.FrameInputs.Add(saveInput);
            if (frame % LSConstValue.SaveLSWorldFrameCount == 0)
            {
                MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
                byte[] bytes = memoryBuffer.ToArray();
                self.Replay.Snapshots.Add(bytes);
            }
        }
    }
}