using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·银光落刃（jumpattack.ani 6 帧 300ms，F2 贯地刺击盒）。
    /// DNF：跳跃状态中按 Z 向下强力刺击（越高越强）+ 落地冲击波（ashenforksub PO，贴地击倒浮空）。
    /// RequireAirborne=真空中限定（本批跳跃系统落地后按原版门禁）。
    /// 下落判定：引擎停驻 F2 判定帧直到落地 → 我们手动盒从 F2 起持续到落地检测；
    /// 落地时点关盒 + 以落点为中心放冲击波 Area（刺击/冲击波独立结算，与原版双层一致）。
    /// 简化：高度分档伤害（static data 语义未考证）→ 固定值；巨剑精通多段/等级缩放延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/016-AshenFork.md
    /// </summary>
    [SkillId(SkillIds.AshenFork)]
    public class AshenForkSkill : SkillLogic
    {
        // ashenfork.atk 直译：down/push 270/lift 180（刺击段）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 70,
            HitstunMs = 500,
            KnockbackX = 270,
            LaunchY = 180,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 4000;          // .skl [dungeon][cool time]
        public override int TotalTimeMs => 700;          // 保险丝：跳跃空中 ~500ms + 落地缓冲
        public override bool RequireAirborne => true;    // DNF"跳跃状态中按指令"——地面施放拒绝

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 刺击盒起（F2 = 100+50 = 150ms）；盒持续到落地（引擎停驻判定帧同构）
        private const int BoxOnMs = 150;

        // F2 贯地盒直译（DNF px：x∈[13,88] y∈[-13,26] z∈[-11,167]）→ TSVector y/z 对调、/100
        private static readonly TSVector BoxOffset = new((FP)505 / 1000, (FP)78 / 100, (FP)65 / 1000);
        private static readonly TSVector BoxHalf = new((FP)375 / 1000, (FP)89 / 100, (FP)195 / 1000);

        // SubState 位：0=空中阶段，1=已落地（冲击波已放）
        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanJumpAttack);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 空中刺击判定：F2 起盒，持续到落地（下落中跟着人走）
            if (ctx.GetSubState() == 0)
            {
                if (ctx.GetElapsedMs() >= BoxOnMs)
                    ctx.SetAttackHitbox(BoxOffset, BoxHalf);

                // 落地时点：关盒 + 落点为中心放冲击波（一次性）
                if (!ctx.IsCasterAirborne())
                {
                    ctx.SetSubState(1);
                    ctx.DisableAttackHitbox();
                    ctx.CreateAreaInFront(AreaIds.AshenFork, FP.Zero);
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
