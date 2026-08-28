using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·鬼斩（hardattack.ani 18 帧 950ms，判定帧 f3-f17 向前挥砍）。
    /// .atk：魔法/暗属性/击倒(down)/push 300/lift 300。
    /// 攻击盒：OnUpdate 手动设（F6 起盒 F12 关盒——.ani 无 attackBox，引擎施加武器判定）。
    /// 刀光：hardattack1/2.ani overlay 叠加（暗属性刀光是本技能辨识度主体）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/005-HardAttack.md
    /// </summary>
    [SkillId(SkillIds.HardAttack)]
    public class HardAttackSkill : SkillLogic
    {
        // hardattack.atk 直译（魔法/暗属性/击倒/push 300/lift 300）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 80,
            HitstunMs = 800,        // [down] 击倒 → 800ms 硬直 + 击飞落地 Down 链
            KnockbackX = 300,       // push 300
            LaunchY = 300,          // lift 300
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 6000;    // .skl [dungeon][cool time] 6000ms
        public override int TotalTimeMs => 950;    // hardattack.ani 总时长

        // 攻击盒时间窗口（帧 → ms：f6 = 50×3+25×2+50 = 300ms，f12 = 550ms）
        private const int HitboxOnMs = 300;        // F6 起盒（挥砍动作开始）
        private const int HitboxOffMs = 550;       // F12 关盒（挥砍动作结束）
        private static readonly TSVector HitboxOffset = new((FP)9 / 10, FP.Zero, (FP)4 / 5);
        private static readonly TSVector HitboxHalfExtents = new((FP)4 / 5, (FP)3 / 10, (FP)3 / 5);

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.HardAttack);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int elapsedMs)
        {
            // 攻击盒窗口：挥砍开始起盒，结束关盒（只调一次由系统幂等处理）
            if (elapsedMs >= HitboxOnMs && elapsedMs < HitboxOffMs)
                ctx.SetAttackHitbox(HitboxOffset, HitboxHalfExtents);
            else if (elapsedMs >= HitboxOffMs)
                ctx.DisableAttackHitbox();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
