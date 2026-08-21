namespace ET
{
    /// <summary>
    /// 班图女战士·膝踢（.mob attack index 1；kneekick.ani 5 帧 360ms，判定帧 f1-f3——
    /// 即 8月17日 f9dd3bf8e 接入的那套帧，现在作为怪物技能正式挂上 Cast 框架）。
    /// .atk：物理/击倒/push 100/lift 200。demo：贴身（0.3 单位内）触发。
    /// </summary>
    [SkillId(SkillIds.MonsterKneeKick)]
    public class MonsterKneeKick : SkillLogic
    {
        // kneekick.atk 直译（[down] 击倒 → 硬直 800ms + 击飞落地 Down 链）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 50,
            HitstunMs = 800,
            KnockbackX = 100,
            LaunchY = 200,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 800;
        public override int TotalTimeMs => 360;     // 动画总时长（判定帧 f1-f3 帧盒自动驱动）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.Attack1);   // kneekick（AnimId.Attack1 历史沿用）
            ctx.ClearHitTargets();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
