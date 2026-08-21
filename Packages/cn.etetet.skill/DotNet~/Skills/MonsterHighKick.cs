namespace ET
{
    /// <summary>
    /// 班图女战士·高踢（.mob attack index 2；highkick.ani 6 帧 370ms，判定帧 f2-f3 高挑盒）。
    /// .atk：击倒/push 200/lift 200/**20% 出血 5s/240**（ProcStatus → BleedBuff 预设）。
    /// 原 .mob 权重 0（AI 随机池不选）——按授权进近身随机池。
    /// </summary>
    [SkillId(SkillIds.MonsterHighKick)]
    public class MonsterHighKick : SkillLogic
    {
        // highkick.atk 直译：击倒 + 20% 出血（DNF 5000ms/240 → BleedBuff 预设 3s/15×3，参数化记档）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 60,
            HitstunMs = 800,
            KnockbackX = 200,
            LaunchY = 200,
            ProcBuffId = BuffIds.Bleed,
            ProcChance = 20,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 800;
        public override int TotalTimeMs => 370;

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.MonsterHighKick);
            ctx.ClearHitTargets();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
