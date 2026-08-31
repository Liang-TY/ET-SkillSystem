using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·破军升龙击（ChargeCrashDash 350ms 冲撞 + ChargeCrashUpper 340ms 上挑，json 自带攻击盒）。
    /// DNF：肩冲撞击退敌人并顺势单手上挑浮空；**撞敌即停驻**（核心手感——每 tick 轮询
    /// GetEnemies+CheckHit 提前转段，分析文档确认现有门面可表达）。上挑反应走 Area（Ly400）。
    /// 下捶条件段（武器/精通）不做。
    /// 参考：Notes/技能实现/鬼剑士技能解析/068-ChargeCrash.md
    /// </summary>
    [SkillId(SkillIds.ChargeCrash)]
    public class ChargeCrashSkill : SkillLogic
    {
        // 冲撞（chargecrashdash.atk 直译：damage/push 200/hit lift up——无浮空，水平撞击）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 90,
            HitstunMs = 500,
            KnockbackX = 200,
            LaunchY = 0,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 2000;    // .skl 10000（用户定案：本批 CD 全 2s）
        public override int TotalTimeMs => 700;    // 冲撞 350 + 上挑 340（帧表实测 350 非 450）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        private const int DashMs = 350;            // 冲撞段（帧表实测：50×4+150）
        private const int DashDistanceX100 = 350;  // 冲刺 3.5 单位（static 观感等效）
        private const int UpperAreaAtMs = 60;      // 上挑段内 F2 起召区（帧表 F2@60）

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.SetPhase(0);
            ctx.PlayAnim(AnimId.SwordmanChargeCrashDash);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int sub = ctx.GetSubState();
            int elapsed = ctx.GetElapsedMs();

            // 冲撞段：匀速前冲 + 撞敌即停
            if (sub == 0)
            {
                // 撞敌停驻（帧驱动盒已激活——CheckHit 用当帧攻击盒 vs 受击盒）
                bool crashed = false;
                foreach (LSUnit enemy in ctx.GetEnemies())
                {
                    if (ctx.CheckHit(ctx.GetCaster(), enemy)) { crashed = true; break; }
                }
                if (crashed || elapsed >= DashMs)
                {
                    ctx.SetSubState(1);
                    ctx.SetPhase(elapsed);
                    ctx.PlayAnim(AnimId.SwordmanChargeCrashUpper);
                    ctx.ClearHitTargets();
                }
                else
                {
                    // 匀速前冲（撞墙由 MoveCasterForward 截断）
                    ctx.MoveCasterForward((FP)DashDistanceX100 / 100 * dtMs / DashMs);
                }
                return;
            }

            // 上挑段：F2 起召唤上挑区（Ly400 挑起）
            if (sub == 1 && elapsed - ctx.GetPhase() >= UpperAreaAtMs)
            {
                ctx.SetSubState(2);
                ctx.CreateAreaInFront(AreaIds.ChargeCrashUpper, (FP)8 / 10);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
