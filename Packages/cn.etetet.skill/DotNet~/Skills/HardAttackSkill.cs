namespace ET
{
    /// <summary>
    /// 鬼剑士·鬼斩（hardattack.ani 18 帧 950ms，判定帧 f3-f17 向前挥砍）。
    /// .atk：魔法/暗属性/击倒(down)/push 300/lift 300。
    /// 攻击盒由动画帧驱动（damageBox f3-f17），HitReaction 参数读本类 override。
    /// 参考：Notes/技能实现/鬼剑士技能解析/005-HardAttack.md
    /// </summary>
    [SkillId(SkillIds.HardAttack)]
    public class HardAttackSkill : SkillLogic
    {
        // hardattack.atk 直译（魔法/暗属性/击倒/push 300/lift 300；demo 伤害值换算：玩家 HP 1000）
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

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.HardAttack);
            ctx.ClearHitTargets();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
