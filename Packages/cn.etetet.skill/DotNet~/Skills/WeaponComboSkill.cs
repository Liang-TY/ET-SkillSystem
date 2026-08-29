namespace ET
{
    /// <summary>
    /// 鬼剑士·里·鬼剑术（weaponcomboblade1-4.ani 太刀 4 段，json 自带攻击盒 → 帧驱动自动激活）。
    /// DNF：普攻派生连段（剑魂特有），段间自动连击 + 按普攻键提前推进，每段 resetHitObjectList。
    /// 保守版：独立按键起手（A 键），派生钩子后补；段间推进 = 单 cast 段状态机（普攻同款）+
    /// ClearHitTargets（flag 100 同构）；每段不同反应走 PhaseHitReaction(SubState)。
    /// 简化：单武器太刀 4 段（其他武器族纯内容件后补）、末段后互锁静默窗不做、等级缩放延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/067-WeaponCombo.md
    /// </summary>
    [SkillId(SkillIds.WeaponCombo)]
    public class WeaponComboSkill : SkillLogic
    {
        // 各段反应（weaponcomboblade1-4.atk 直译：damage bonus/push/lift；末段 down 击飞）
        private static readonly HitReaction Seg1 = new() { Damage = 60, HitstunMs = 400, KnockbackX = 30, LaunchY = 80 };
        private static readonly HitReaction Seg2 = new() { Damage = 70, HitstunMs = 400, KnockbackX = 30, LaunchY = 95 };
        private static readonly HitReaction Seg3 = new() { Damage = 80, HitstunMs = 400, KnockbackX = 30, LaunchY = 105 };
        private static readonly HitReaction Seg4 = new() { Damage = 110, HitstunMs = 700, KnockbackX = 80, LaunchY = 400 };

        public override HitReaction HitReaction => Seg1;

        // phase = SubState = 段号 0-3
        public override HitReaction PhaseHitReaction(int phase) => phase switch
        {
            0 => Seg1,
            1 => Seg2,
            2 => Seg3,
            _ => Seg4,
        };

        public override int CooldownMs => 0;       // .skl [dungeon][cool time] 0（施放不起 CD）

        private static readonly int[] SegmentMs = { 700, 640, 700, 640 };   // 太刀 4 段（json 直译）
        private const int SegmentCount = 4;
        private const int CancelMs = 300;          // 段取消点（flag 100 ≈ F3 半程，四段统一 300）
        private const int NormalAttackButton = 1;  // 提前推进 = 普攻键（DNF 里鬼按攻击键续段同构）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int TotalTimeMs => 2800;   // 保险丝：全连 2680 + 余量

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.SetPhase(0);
            ctx.PlayAnim(AnimId.SwordmanWeaponComboBlade1);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int seg = ctx.GetSubState();
            int t = ctx.GetElapsedMs() - ctx.GetPhase();   // 段内时间

            // 提前推进：取消窗口起按普攻键（DNF onProcCon 连打同构）
            // 自动推进：段动画播完（里鬼为自动连段）
            bool advance = (t >= CancelMs && ctx.PeekBufferedButton() == NormalAttackButton) || t >= SegmentMs[seg];
            if (advance && seg < SegmentCount - 1)
            {
                if (ctx.PeekBufferedButton() == NormalAttackButton) ctx.ConsumeBuffer();
                ctx.SetSubState(seg + 1);
                ctx.SetPhase(ctx.GetElapsedMs());
                ctx.PlayAnim(AnimId.SwordmanWeaponComboBlade1 + seg + 1);
                ctx.ClearHitTargets();   // = DNF resetHitObjectList（flag 100 同构）
                return;
            }

            // 末段播完收招
            if (seg == SegmentCount - 1 && t >= SegmentMs[seg])
            {
                ctx.EndCast();
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
