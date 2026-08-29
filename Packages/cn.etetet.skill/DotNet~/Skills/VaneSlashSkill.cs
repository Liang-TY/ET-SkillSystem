namespace ET
{
    /// <summary>
    /// 鬼剑士·裂波斩（vaneslashtry 上斩 350ms + vaneslash 旋转 1490ms）。
    /// DNF：上斩抓取前方敌人 → 挑起 → 裂波波轮多段 3 次 → 下斩终结砸飞。
    /// 抓取系统缺失 → 上斩=普通命中+LaunchY 挑起（Ly 180）；波轮=Tick 多段 Area（350ms×3，
    /// 与 PO 无盒暖机段对齐）固定身前 0.8；终结=一次性 Area（down/Kb500/Ly200）。
    /// 简化：蓄条 400ms 砍掉瞬发、无敌帧不做、波动刻印联动跳过。
    /// 参考：Notes/技能实现/鬼剑士技能解析/058-VaneSlash.md
    /// </summary>
    [SkillId(SkillIds.VaneSlash)]
    public class VaneSlashSkill : SkillLogic
    {
        // 上斩（vaneslashtry.atk：none/0/0——抓取代劳浮空 → 我方 Ly 180 挑起替代）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 70,
            HitstunMs = 600,
            KnockbackX = 0,
            LaunchY = 180,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 8000;    // .skl [dungeon][cool time]
        public override int TotalTimeMs => 2000;   // try 350 + spin 1490 + 余量（终结 Area 独立结算）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        private const int SpinAtMs = 350;          // try 播完 → 旋转 + 召唤波轮
        private const int FinalAtMs = 1600;        // 波轮 1250ms 结束 → 下斩终结

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanVaneSlashTry);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int t = ctx.GetElapsedMs();

            // 上斩 → 旋转：try 盒（F2/F3 帧驱动）切走后手动清残留 + 召唤波轮
            if (ctx.GetSubState() == 0 && t >= SpinAtMs)
            {
                ctx.SetSubState(1);
                ctx.PlayAnim(AnimId.SwordmanVaneSlash);
                ctx.DisableAttackHitbox();
                ctx.CreateAreaInFront(AreaIds.VaneSlashWheel, (FP)8 / 10);
            }

            // 波轮结束 → 下斩终结
            if (ctx.GetSubState() == 1 && t >= FinalAtMs)
            {
                ctx.SetSubState(2);
                ctx.CreateAreaInFront(AreaIds.VaneSlashFinal, (FP)8 / 10);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
