using TrueSync;

namespace ET
{
    /// <summary>
    /// 投射物系统（ET.Skill：要调 BulletLoader/ActionLoader 防循环依赖）。
    /// 弹的组件在运行时创建（Id 大于全部初始组件）→ 弹的 LSUpdate 晚于单位——
    /// 单位本帧受击盒已采样完毕 ✓。
    /// </summary>
    [EntitySystemOf(typeof(LSBullet))]
    [LSEntitySystemOf(typeof(LSBullet))]
    [FriendOf(typeof(LSBullet))]
    public static partial class LSBulletSystem
    {
        [EntitySystem]
        private static void Awake(this LSBullet self, int configId)
        {
            self.ConfigId = configId;
            self.HitTargets ??= new();
        }

        /// <summary>
        /// 创建投射物（SkillContext.CreateBullet 调；回滚后从快照恢复不重跑）。
        /// 出生点 = 施法者身前 0.8 单位，方向 = 施法者朝向，y=半高（贴地飞行的波）。
        /// </summary>
        public static LSBullet Create(this LSBulletComponent parent, LSUnit caster, int bulletId)
        {
            BulletDefinition def = BulletLoader.Get(bulletId);
            if (def == null)
            {
                Log.Error($"[Bullet] 未注册的 bulletId={bulletId}，跳过");
                return null;
            }

            TSVector forward = caster.Forward;
            LSBullet bullet = parent.AddChild<LSBullet, int>(bulletId);
            bullet.CasterId = caster.Id;
            bullet.Direction = new TSVector(forward.x >= FP.Zero ? FP.One : -FP.One, FP.Zero, FP.Zero);
            // 出生 = 施法者位置 + 朝向 × 身前距离 + 高度/纵深偏移（def.SpawnOffset 直译 DNF 投掷参数）
            bullet.Position = caster.Position
                              + new TSVector(bullet.Direction.x * def.SpawnOffset.x, FP.Zero, FP.Zero)
                              + new TSVector(FP.Zero, def.SpawnOffset.y, def.SpawnOffset.z);
            bullet.RemainingMs = def.TotalTimeMs;
            Log.Info($"[Bullet] unit{caster.Id} 发射 {def.GetType().Name} @ {bullet.Position}");
            return bullet;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSBullet self)
        {
            BulletDefinition def = BulletLoader.Get(self.ConfigId);
            if (def == null)
            {
                self.Dispose();
                return;
            }

            LSWorld world = self.GetParent<LSBulletComponent>().Parent as LSWorld;

            // 1) 寿命
            self.RemainingMs -= LSConstValue.UpdateInterval;
            if (self.RemainingMs <= 0)
            {
                self.Dispose();
                return;
            }

            // 2) 飞行（先判碰撞再移动：出生帧身前的盒就能命中贴脸目标）
            AABB box = AABBUtil.FromCenter(self.Position, def.HalfExtents);
            LSUnitComponent unitComponent = world.GetComponent<LSUnitComponent>();
            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is not LSUnit unit || unit.Id == self.CasterId) continue;   // 不打自己
                LSHitboxComponent hitbox = unit.GetComponent<LSHitboxComponent>();
                if (hitbox == null || hitbox.CurrentHurtBoxes.Count == 0) continue;
                if (self.HitTargets.Contains(unit.Id)) continue;   // 已结算过（穿透去重）

                bool hit = false;
                foreach (AABB hurt in hitbox.CurrentHurtBoxes)
                {
                    if (!AABBUtil.Intersects(box, hurt)) continue;
                    hit = true;
                    break;
                }
                if (!hit) continue;

                self.HitTargets.Add(unit.Id);
                RunHitActions(def, world, unit, self.CasterId);

                if (def.DestroyOnHit)
                {
                    self.Dispose();
                    return;
                }
            }

            // 3) 位移（50ms 固定步长：Speed 单位/秒 × dt）
            self.Position += self.Direction * def.Speed * LSConstValue.UpdateInterval / 1000;
        }

        /// <summary>命中效果（owner=受击单位，source=施法者）</summary>
        private static void RunHitActions(BulletDefinition def, LSWorld world, LSUnit target, long casterId)
        {
            if (def.HitActions == null) return;
            LSUnit caster = world.GetComponent<LSUnitComponent>().GetChild<LSUnit>(casterId);
            int frameNo = world.Frame;
            foreach (int actionId in def.HitActions)
            {
                LSAction action = ActionLoader.Get(actionId);
                if (action == null)
                {
                    Log.Error($"[Bullet] bulletId 配置引用了未注册的 actionId={actionId}，跳过");
                    continue;
                }
                action.Run(new LSActionContext(world, target, caster, frameNo, def.HitReaction));
            }
        }
    }
}
