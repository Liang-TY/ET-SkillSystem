namespace ET
{
    /// <summary>
    /// 班图女战士·下段踢（.mob attack index 0；lowkick.ani 7 帧 540ms，判定帧 f3-f5 扫腿扇形）。
    /// .atk：物理/普通硬直/push 100/lift 200。攻击盒由动画帧驱动（IsAttackDrivenAnim 白名单）。
    /// demo：AI 随机池权重 100、触发距离 1.15（阶段2 AI 落实）。
    /// </summary>
    [SkillId(SkillIds.MonsterLowKick)]
    public class MonsterLowKick : SkillLogic
    {
        // lowkick.atk 直译（demo 换算：玩家 HP 1000）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 50,
            HitstunMs = 400,        // [damage] 普通硬直
            KnockbackX = 100,
            LaunchY = 200,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 800;      // .mob [attack delay]
        public override int TotalTimeMs => 540;     // 动画总时长（判定帧 f3-f5 帧盒自动驱动）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.MonsterLowKick);
            ctx.ClearHitTargets();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
