using TrueSync;

namespace ET
{
    /// <summary>
    /// 班图女战士·冰息（.mob attack index 3；icebreath.ani 8 帧 660ms，本体无判定）。
    /// 帧 3（.mob [throw frame]）发射冰雾弹（→ IceBreathBullet：250 速/1200ms 寿命/水属性/10% 冰冻）。
    /// 出生偏移 (60, 0, 70) = 身前 0.6 单位（DNF 偏移 z=70 是贴地高度，弹视图贴地）。
    /// 原 .mob 权重 0——按授权做远程触发（距离 &gt; 1.2 单位，阶段2 AI 落实）。
    /// </summary>
    [SkillId(SkillIds.MonsterIceBreath)]
    public class MonsterIceBreath : SkillLogic
    {
        private const int ThrowFrame = 3;   // .mob [throw frame]

        public override int CooldownMs => 2000;    // 远程技能给足间隔（demo 值）
        public override int TotalTimeMs => 660;    // 动画总时长（本体无攻击盒）

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.MonsterIceBreath);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 帧 3 发射（一次性，SubState 守卫；DNF [throw frame] 同构）
            if (ctx.GetSubState() == 0 && ctx.CurrentFrameIndex() >= ThrowFrame)
            {
                ctx.SetSubState(1);
                ctx.CreateBullet(BulletIds.IceBreath);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
