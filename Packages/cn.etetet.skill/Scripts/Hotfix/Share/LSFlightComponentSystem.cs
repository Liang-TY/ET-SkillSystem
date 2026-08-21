using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSFlightComponent))]
    [LSEntitySystemOf(typeof(LSFlightComponent))]
    [FriendOf(typeof(LSFlightComponent))]
    [FriendOf(typeof(LSCombatComponent))]   // 落地播倒地动画 + 硬直托底（ET0002）
    public static partial class LSFlightComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSFlightComponent self)
        {
            // 默认物理参数（调参入口；按单位覆盖可做抗击飞差异，如重型 Boss）
            self.Gravity = (FP)40;
            self.GroundFriction = (FP)8;
            self.MinSlideSpeed = (FP)1 / 10;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSFlightComponent self)
        {
            if (!self.Active) return;
            LSUnit unit = self.GetParent<LSUnit>();
            FP dt = (FP)LSConstValue.UpdateInterval / 1000;

            // 本帧位移前是否在空中（区分落地/贴地滑行）：y>0 或初速向上（贴地小浮空值首帧 y 仍 0，
            // 不算空中会被误判成滑行——虽然小初速下一帧就落地，语义上仍是击飞）
            bool wasAirborne = unit.Position.y > FP.Zero || self.Velocity.y > FP.Zero;
            TSVector v = self.Velocity;
            v.y -= self.Gravity * dt;
            TSVector pos = unit.Position + v * dt;

            if (pos.y <= FP.Zero)
            {
                pos.y = FP.Zero;
                if (wasAirborne)
                {
                    // 击飞落地：动量清零趴住（DNF 击倒手感）——播倒地动画 + 硬直托底到动画播完
                    // （起身由 LSCombatComponentSystem 硬直结束逻辑切回 DefaultAnimId）
                    v = TSVector.zero;
                    self.Active = false;
                    LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();
                    if (combat != null && combat.DownAnimId != 0)
                    {
                        unit.GetComponent<LSAnimComponent>()?.Play(combat.DownAnimId);
                        AnimClipData downClip = AnimConfigRegistry.Get(combat.DownAnimId);
                        int downMs = downClip?.totalDuration ?? 0;
                        if (downMs > combat.HitstunTimer) combat.HitstunTimer = downMs;
                    }
                }
                else
                {
                    // 贴地滑行（纯水平击退）：只衰减水平速度
                    v.y = FP.Zero;
                    v.x -= v.x * self.GroundFriction * dt;
                    v.z -= v.z * self.GroundFriction * dt;
                    FP min = self.MinSlideSpeed;
                    if (v.x * v.x + v.z * v.z < min * min)
                    {
                        v = TSVector.zero;
                        self.Active = false;
                    }
                }
            }
            unit.Position = pos;
            self.Velocity = v;
        }
    }
}
