using System;
using System.Collections.Generic;
using System.IO;
using TrueSync;

namespace ET
{
    [FriendOf(typeof(Room))]
    [FriendOf(typeof(LSCombatComponent))]   // 怪物工厂写 HurtAnimId（ET0002）
    public static partial class RoomSystem
    {
        public static Room Room(this Entity entity)
        {
            return entity.IScene as Room;
        }
        
        public static void Init(this Room self, List<LockStepUnitInfo> unitInfos, long startTime, int frame = -1)
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
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                LockStepUnitInfo unitInfo = unitInfos[i];
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
                Log.Info($"[Room] 玩家单位创建：PlayerId={unitInfo.PlayerId}");
            }

            // Half B 测试桩：班图女战士（阶段1 技能轮播驱动；阶段2 换 AI）
            // 不进 PlayerIds、不加 LSInputComponent（不是玩家：不吃输入、相机不跟）
            LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
            LSUnit monster = lsUnitComponent.AddChild<LSUnit>();
            monster.Position = new TSVector(3, 0, 0);
            monster.Forward = new TSVector(-1, 0, 0);   // 面向玩家出生点（0,0,0）——攻击盒采样/发弹方向都吃朝向
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

            // 阶段1 临时轮播驱动（每 3s 依次放一个技能；阶段2 AI 替换）
            monster.AddComponent<LSMonsterDebugDriverComponent>();
            Log.Info($"[Monster] 测试桩怪物 unit{monster.Id} @ {monster.Position}（轮播驱动）");
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