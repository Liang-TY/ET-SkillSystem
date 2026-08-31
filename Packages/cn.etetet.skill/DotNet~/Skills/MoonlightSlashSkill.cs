namespace ET
{
    /// <summary>
    /// 鬼剑士·月光斩（moonlightslash1/2/full 三段自动连段，json 自带攻击盒 → 帧驱动自动激活）。
    /// DNF：暗属性月形斩 → 单手上挑 → 满月（已学满月斩解锁——demo 常开）；自动连段无需按键确认
    /// （剑影版 onEndCurrentAni 链式推进同构）。
    /// 三段不同反应走 PhaseHitReaction(SubState)；月牙/满月特效 = 手组装 overlay。
    /// 简化：方向控制不做（无按键状态门面）、暗属性不带元素、每段单次命中。
    /// 参考：Notes/技能实现/鬼剑士技能解析/077-MoonlightSlash.md
    /// </summary>
    [SkillId(SkillIds.MoonlightSlash)]
    public class MoonlightSlashSkill : SkillLogic
    {
        // 三段反应（moonlightslash1/2/full.atk 直译：段1 lift300 down；段2 down/lift300；段3 down/push100/lift300）
        private static readonly HitReaction Seg1 = new() { Damage = 70, HitstunMs = 500, KnockbackX = 0, LaunchY = 300 };
        private static readonly HitReaction Seg2 = new() { Damage = 80, HitstunMs = 600, KnockbackX = 0, LaunchY = 300 };
        private static readonly HitReaction Seg3 = new() { Damage = 100, HitstunMs = 800, KnockbackX = 100, LaunchY = 300 };

        public override HitReaction HitReaction => Seg1;

        // phase = SubState = 段号 0-2
        public override HitReaction PhaseHitReaction(int phase) => phase switch
        {
            0 => Seg1,
            1 => Seg2,
            _ => Seg3,
        };

        public override int CooldownMs => 2000;    // .skl 4000（用户定案：本批 CD 全 2s）

        private static readonly int[] SegmentMs = { 620, 620, 550 };
        private const int SegmentCount = 3;

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int TotalTimeMs => 1850;   // 保险丝：三段 1790 + 余量

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.SetPhase(0);
            ctx.PlayAnim(AnimId.SwordmanMoonlightSlash1);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int seg = ctx.GetSubState();
            int t = ctx.GetElapsedMs() - ctx.GetPhase();

            // 自动连段：段动画播完切下一段（DNF 同构——无输入确认）
            if (t >= SegmentMs[seg])
            {
                if (seg < SegmentCount - 1)
                {
                    ctx.SetSubState(seg + 1);
                    ctx.SetPhase(ctx.GetElapsedMs());
                    ctx.PlayAnim(AnimId.SwordmanMoonlightSlash1 + seg + 1);
                    ctx.ClearHitTargets();
                }
                else
                {
                    ctx.EndCast();
                }
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
