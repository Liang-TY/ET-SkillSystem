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
            if (mapDef?.MonsterAiIds != null)
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
            collision.CellSize = (FP)layout.cellSizePx / 100;   // px → 单位（1 单位=100px）
            collision.PassGrid = new byte[layout.gridWidth * layout.gridHeight];
            for (int i = 0; i < collision.PassGrid.Length; i++)
            {
                // DNF [pass type] 原值直译：'2'=可走(1)，其余('0' 等)=阻挡(0)；串短于矩阵时尾部全阻挡
                collision.PassGrid[i] = i < layout.passTypes.Length && layout.passTypes[i] == '2' ? (byte)1 : (byte)0;
            }
            Log.Info($"[Room] 碰撞矩阵就绪：{collision.GridWidth}x{collision.GridHeight} 格，cell={collision.CellSize}");
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