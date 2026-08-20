namespace ET
{
    /// <summary>
    /// 鬼剑士普攻第一段（swordman_attack1.json，10 帧）。
    /// 注意：鬼剑士 .ani 没有 attackBox（DNF 玩家攻击盒由 nut 脚本动态创建），
    /// 所以本技能用 SetAttackHitbox 手动设攻击盒（阶段 3.5 的帧驱动路径不适用于玩家角色）。
    /// </summary>
    [SkillId(SkillIds.NormalAttack)]
    public class NormalAttack : SkillLogic
    {
        // swordman_attack1 共 10 帧，取消窗口从帧 6 起（收招半程）
        private const int CancelFrame = 6;

        // 攻击盒参数（DNF 太刀普攻范围：面前 ~1 单位，横向 ~0.8，高 ~1）
        private static readonly TrueSync.TSVector HitboxOffset = new((TrueSync.FP)8 / 10, (TrueSync.FP)5 / 10, 0);
        private static readonly TrueSync.TSVector HitboxHalf = new((TrueSync.FP)4 / 10, (TrueSync.FP)5 / 10, (TrueSync.FP)3 / 10);

        public override int CooldownMs => 0;
        public override int TotalTimeMs => 400;   // 10 帧 × 40ms

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit, ActionIds.AddBurnBuff };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.SwordmanAttack1);
            ctx.ClearHitTargets();
            ctx.SetAttackHitbox(HitboxOffset, HitboxHalf);   // 手动设攻击盒（.ani 无 attackBox）
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            if (ctx.CurrentFrameIndex() >= CancelFrame && ctx.PeekBufferedButton() == 1)
            {
                ctx.RestartCurrentSkill();
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
