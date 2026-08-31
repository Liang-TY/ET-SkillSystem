using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·拔刀斩（momentaryslash.ani 12 帧 1055ms，F0=500ms 蓄势原帧直用）。
    /// DNF：拔刀蓄势后向周围大范围快速斩出强力一击（波 PO Start.ani F0 盒：半尺寸 1.1×0.7×0.7）。
    /// 基础版（classic 形态）：蓄力五段/太刀旋转砸落/精通追击全跳过。
    /// 大波视觉 = 短命纯视觉区（MomentarySlashWave Area，New_BigWave）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/009-MomentarySlash.md
    /// </summary>
    [SkillId(SkillIds.MomentarySlash)]
    public class MomentarySlashSkill : SkillLogic
    {
        // momentaryslashwave.atk 直译：absolute 3000/down/push 0/lift 0（长硬直近似击倒）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 120,
            HitstunMs = 800,
            KnockbackX = 0,
            LaunchY = 0,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 2000;    // .skl 15000（用户定案：本批 CD 全 2s）
        public override int TotalTimeMs => 1055;   // 动画原时长（F0 蓄势 500ms 本来就是帧时长）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 出刀窗口：F4 = 500+25×3+50（F1-F3 三连快帧）→ 150ms 判定窗
        private const int SlashOnMs = 575;
        private const int SlashOffMs = 725;

        // 波 PO 盒折算：中心 x≈+0.4、半尺寸 (1.1 横向, 0.7 高度, 0.7 纵深)
        private static readonly TSVector BoxOffset = new((FP)5 / 10, (FP)35 / 100, FP.Zero);
        private static readonly TSVector BoxHalf = new((FP)11 / 10, (FP)7 / 10, (FP)7 / 10);

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanMomentarySlash);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int t = ctx.GetElapsedMs();

            // 出刀瞬间：起盒 + 大波视觉
            if (ctx.GetSubState() == 0 && t >= SlashOnMs)
            {
                ctx.SetSubState(1);
                ctx.SetAttackHitbox(BoxOffset, BoxHalf);
                ctx.CreateAreaInFront(AreaIds.MomentarySlashWave, (FP)5 / 10);
            }
            // 判定窗关闭
            else if (ctx.GetSubState() == 1 && t >= SlashOffMs)
            {
                ctx.SetSubState(2);
                ctx.DisableAttackHitbox();
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
