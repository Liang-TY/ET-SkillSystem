using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSFlightComponent))]
    [LSEntitySystemOf(typeof(LSFlightComponent))]
    [FriendOf(typeof(LSFlightComponent))]
    public static partial class LSFlightComponentSystem
    {
        // 物理常量（单位/秒体系）：重力 40 → lift 400px(初速 8) 空中约 0.4s、最高 0.8 单位；
        // 贴地摩擦 8/s → 水平击退滑行约 0.4s 衰减殆尽。手感不对在测试机改这三个数。
        [StaticField]
        private static readonly FP Gravity = (FP)40;

        [StaticField]
        private static readonly FP GroundFriction = (FP)8;

        [StaticField]
        private static readonly FP MinSlideSpeed = (FP)1 / 10;

        [EntitySystem]
        private static void Awake(this LSFlightComponent self)
        {
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
            v.y -= Gravity * dt;
            TSVector pos = unit.Position + v * dt;

            if (pos.y <= FP.Zero)
            {
                pos.y = FP.Zero;
                if (wasAirborne)
                {
                    // 击飞落地：动量清零趴住（DNF 击倒手感；起身时机由 HitstunTimer 管）
                    v = TSVector.zero;
                    self.Active = false;
                }
                else
                {
                    // 贴地滑行（纯水平击退）：只衰减水平速度
                    v.y = FP.Zero;
                    v.x -= v.x * GroundFriction * dt;
                    v.z -= v.z * GroundFriction * dt;
                    if (v.x * v.x + v.z * v.z < MinSlideSpeed * MinSlideSpeed)
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
