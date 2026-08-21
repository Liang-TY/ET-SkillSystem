using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 区域效果系统（ET.Skill：要调 AreaLoader/ActionLoader）。
    /// 生命周期：创建 → 进入检测（EnterActions）→ Tick（TickActions）→ 到时/离开（ExitActions）→ 回收。
    /// Route B：Just\* 标记由 LSAreaComponentSystem 开头清（同 LSSkillComponentSystem 模式）。
    /// </summary>
    [EntitySystemOf(typeof(LSArea))]
    [LSEntitySystemOf(typeof(LSArea))]
    [FriendOf(typeof(LSArea))]
    public static partial class LSAreaSystem
    {
        /// <summary>区域迭代复用缓冲（进出检测增删 InsideUnits，快照防并发）</summary>
        [StaticField]
        private static readonly List<long> unitIdScratch = new();

        [EntitySystem]
        private static void Awake(this LSArea self, int configId)
        {
            self.ConfigId = configId;
            self.InsideUnits ??= new();
        }

        /// <summary>创建区域（SkillContext.CreateArea 调）</summary>
        public static LSArea Create(this LSAreaComponent parent, LSUnit caster, TSVector position, int areaId)
        {
            AreaDefinition def = AreaLoader.Get(areaId);
            if (def == null)
            {
                Log.Error($"[Area] 未注册的 areaId={areaId}，跳过");
                return null;
            }

            LSArea area = parent.AddChild<LSArea, int>(areaId);
            area.CasterId = caster.Id;
            area.Position = position;
            area.RemainingMs = def.TotalTimeMs;
            area.TickTimer = 0;
            area.JustAdded = true;
            return area;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSArea self)
        {
            if (self.Removing)
            {
                self.Dispose();
                return;
            }

            AreaDefinition def = AreaLoader.Get(self.ConfigId);
            if (def == null)
            {
                self.Dispose();
                return;
            }

            LSWorld world = self.GetParent<LSAreaComponent>().Parent as LSWorld;
            LSUnitComponent unitComponent = world.GetComponent<LSUnitComponent>();
            int frameNo = world.Frame;

            // 1) 寿命
            if (def.TotalTimeMs > 0)
            {
                self.RemainingMs -= LSConstValue.UpdateInterval;
                if (self.RemainingMs <= 0)
                {
                    self.Expire(def, unitComponent, frameNo);
                    return;
                }
            }

            // 2) 进出检测：当前帧在区域内的单位 vs InsideUnits 缓存
            AABB areaBox = AABBUtil.FromCenter(self.Position, def.HalfExtents);
            unitIdScratch.Clear();
            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is not LSUnit unit || unit.Id == self.CasterId) continue;   // 不影响施法者
                LSHitboxComponent hitbox = unit.GetComponent<LSHitboxComponent>();
                if (hitbox == null || hitbox.CurrentHurtBoxes.Count == 0) continue;

                bool inside = false;
                foreach (AABB hurt in hitbox.CurrentHurtBoxes)
                {
                    if (AABBUtil.Intersects(areaBox, hurt)) { inside = true; break; }
                }
                if (inside) unitIdScratch.Add(unit.Id);
            }

            // 新进入 → EnterActions
            foreach (long unitId in unitIdScratch)
            {
                if (self.InsideUnits.Contains(unitId)) continue;
                self.InsideUnits.Add(unitId);
                RunActions(def.EnterActions, def.HitReaction, world, unitComponent, unitId, self.CasterId, frameNo);
            }

            // 离开 → ExitActions（在缓存但不在当前帧）
            List<long> exited = null;
            foreach (long unitId in self.InsideUnits)
            {
                if (unitIdScratch.Contains(unitId)) continue;
                exited ??= new List<long>();
                exited.Add(unitId);
            }
            if (exited != null)
            {
                foreach (long unitId in exited)
                {
                    self.InsideUnits.Remove(unitId);
                    RunActions(def.ExitActions, def.HitReaction, world, unitComponent, unitId, self.CasterId, frameNo);
                }
            }

            // 3) Tick
            if (def.TickTimeMs > 0)
            {
                self.TickTimer += LSConstValue.UpdateInterval;
                while (self.TickTimer >= def.TickTimeMs && !self.Removing)
                {
                    self.TickTimer -= def.TickTimeMs;
                    foreach (long unitId in self.InsideUnits)
                    {
                        RunActions(def.TickActions, def.HitReaction, world, unitComponent, unitId, self.CasterId, frameNo);
                    }
                }
            }
        }

        /// <summary>到时消失：对所有仍在区域内的单位跑 ExitActions → 置标记 → 下帧回收</summary>
        private static void Expire(this LSArea self, AreaDefinition def, LSUnitComponent unitComponent, int frameNo)
        {
            self.Removing = true;
            self.JustRemoved = true;
            LSWorld world = self.GetParent<LSAreaComponent>().Parent as LSWorld;
            foreach (long unitId in self.InsideUnits)
            {
                RunActions(def.ExitActions, def.HitReaction, world, unitComponent, unitId, self.CasterId, frameNo);
            }
            self.InsideUnits.Clear();
        }

        private static void RunActions(int[] actionIds, HitReaction hitReaction, LSWorld world, LSUnitComponent unitComponent, long targetId, long casterId, int frameNo)
        {
            if (actionIds == null) return;
            LSUnit target = unitComponent.GetChild<LSUnit>(targetId);
            if (target == null) return;
            LSUnit caster = unitComponent.GetChild<LSUnit>(casterId);
            foreach (int actionId in actionIds)
            {
                LSAction action = ActionLoader.Get(actionId);
                if (action == null) continue;
                action.Run(new LSActionContext(world, target, caster, frameNo, hitReaction));
            }
        }
    }
}
