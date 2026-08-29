namespace ET
{
    /// <summary>
    /// 鬼剑士·十字斩（gorecross.ani 29 帧 1330ms，无攻击盒 → 手动盒，flag 帧触发）。
    /// DNF：F7 横斩（push100/lift100）→ F15 纵斩击飞（lift200）+ 召唤血之十字 PO（升起+出血→
    /// 三联爆发击倒）；召唤瞬间按技能键=强力追击（mercilessness：down/push120/lift300）。
    /// 两刀不同反应走 PhaseHitReaction(SubState)；血之十字两相位 = 两个 Area 顺序创建；
    /// 追击 = F15 起输入窗口（Phase 字段当一次性标志，与爆发近似互斥——窗口 1070~1390ms）。
    /// 简化：血气旺盛常开（出血 100%）、追击无专属动画（复用三联爆发视觉）、等级缩放延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/064-GoreCross.md
    /// </summary>
    [SkillId(SkillIds.GoreCross)]
    public class GoreCrossSkill : SkillLogic
    {
        // 第一刀（gorecross1.atk 直译：damage/push 100/lift 100）
        private static readonly HitReaction Slash1 = new()
        {
            Damage = 80,
            HitstunMs = 500,
            KnockbackX = 100,
            LaunchY = 100,
        };

        // 第二刀（gorecross2.atk 直译：damage/push 100/lift 200 击飞）
        private static readonly HitReaction Slash2 = new()
        {
            Damage = 90,
            HitstunMs = 600,
            KnockbackX = 100,
            LaunchY = 200,
        };

        public override HitReaction HitReaction => Slash1;

        // phase = SubState：1=第一刀窗口，2=第二刀窗口（追击/爆发走 Area 不经此）
        public override HitReaction PhaseHitReaction(int phase) => phase == 1 ? Slash1 : Slash2;

        public override int CooldownMs => 3000;    // .skl [dungeon][cool time]
        public override int TotalTimeMs => 1400;   // 动画 1330 + 爆发时点 1390 兜底

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 攻击盒（引擎施加无数据 → 草案值）：前偏 0.9，半尺寸 (0.7 横向, 0.3 高度, 0.6 纵深)
        private static readonly TrueSync.TSVector BoxOffset = new((TrueSync.FP)9 / 10, (TrueSync.FP)3 / 10, TrueSync.FP.Zero);
        private static readonly TrueSync.TSVector BoxHalf = new((TrueSync.FP)7 / 10, (TrueSync.FP)3 / 10, (TrueSync.FP)6 / 10);

        // SubState：0=起手 1=第一刀窗 2=第二刀窗 3=第二刀后 4=爆发已发（终态）
        // Phase：0=默认，1=追击已发（一次性标志）
        private const int Slash1AtMs = 350;        // F7 flag1
        private const int Slash1OffMs = 500;
        private const int Slash2AtMs = 1070;       // F15 flag2
        private const int Slash2OffMs = 1200;
        private const int BurstAtMs = 1390;
        private const int FinishButton = 18;       // 追击输入 = 本技能键

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.SetPhase(0);
            ctx.PlayAnim(AnimId.SwordmanGoreCross);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int sub = ctx.GetSubState();
            int t = ctx.GetElapsedMs();

            // 第一刀窗口
            if (sub == 0 && t >= Slash1AtMs)
            {
                ctx.SetSubState(1);
                ctx.SetAttackHitbox(BoxOffset, BoxHalf);
            }
            else if (sub == 1 && t >= Slash1OffMs)
            {
                ctx.SetSubState(2);
                ctx.DisableAttackHitbox();
            }
            // 第二刀 + 血之十字（相位1）
            else if (sub == 2 && t >= Slash2AtMs)
            {
                ctx.SetSubState(3);
                ctx.ClearHitTargets();
                ctx.SetAttackHitbox(BoxOffset, BoxHalf);
                ctx.CreateAreaInFront(AreaIds.GoreCrossCross, (FP)9 / 10);
            }

            // 第二刀关盒
            if (sub == 3 && t >= Slash2OffMs && ctx.GetSubState() < 4) ctx.DisableAttackHitbox();

            // 强力追击（F15 起输入窗口，一次性）
            if (sub == 3 && ctx.GetPhase() == 0 && t >= Slash2AtMs && ctx.PeekBufferedButton() == FinishButton)
            {
                ctx.ConsumeBuffer();
                ctx.SetPhase(1);
                ctx.CreateAreaInFront(AreaIds.GoreCrossFinish, (FP)9 / 10);
            }

            // 三联爆发（相位2，时间驱动——已越出动画帧）
            if (sub == 3 && t >= BurstAtMs)
            {
                ctx.SetSubState(4);
                ctx.CreateAreaInFront(AreaIds.GoreCrossBurst, (FP)9 / 10);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
