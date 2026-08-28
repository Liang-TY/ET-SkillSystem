using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·三段斩（tripleslash1~3.ani 各 5 帧 580ms，单 cast 段状态机）。
    /// DNF（jg_swordman tripleslash.nut 实证）：滑动前冲 3 连斩，段间连按续段，中断/结束才起 CD；
    /// 每段前冲 200px/200ms 匀速；段1/2 普通硬直，段3（终段）击倒。
    /// 段3 命中反应是技能级单值放不下的第二套参数 → 一次性 Area（BloodBoom 范式），本体盒让位防双结算。
    /// 攻击盒：.ani 无 attackBox（引擎施加武器判定）→ 手动盒。
    /// 弧光/扬尘：slash1~5 + move1~2 手组装 overlay 挂层。
    /// 简化：段间反向斩（无方向输入门面）/5 连强化（TP 143）/撞墙绕行（MoveCasterForward 自带撞墙停）延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/008-TripleSlash.md
    /// </summary>
    [SkillId(SkillIds.TripleSlash)]
    public class TripleSlashSkill : SkillLogic
    {
        private const int SegmentCount = 3;      // 5 连强化时改 5（段4/5 动画已注册）

        // 每段时长（json 直译 580ms：F0-F3 ×70 + F4 收招 ×300）
        private const int SegmentMs = 580;

        // 续段窗口：DNF onProcCon 动画帧≥3 起（≈210ms）
        private const int CancelMs = 210;

        // 每段前冲：DNF 200px / 200ms 匀速（参照脚本硬编码）
        private const int DashMs = 200;
        private const int DashUnitsX100 = 200;   // 2 单位 ×100 定点

        // 攻击盒窗口（F0 flag=判定帧，引擎施加 → 手动盒覆盖挥砍半程）
        private const int BoxOffMs = 280;

        // 盒：前偏 0.8、半尺寸 (0.7 横向, 0.35 高度, 0.6 纵深)——DNF 草案值 y/z 对调后
        private static readonly TSVector BoxOffset = new((FP)8 / 10, (FP)3 / 10, FP.Zero);
        private static readonly TSVector BoxHalf = new((FP)7 / 10, (FP)35 / 100, (FP)6 / 10);

        // 段1/2 命中（tripleslash1/2.atk 直译：damage/push 200/lift 120）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 60,
            HitstunMs = 500,
            KnockbackX = 200,
            LaunchY = 120,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int CooldownMs => 6000;      // .skl [dungeon][cool time]
        public override bool ManualCooldown => true; // DNF：连斩中断/结束才起 CD（startSkillCoolTime）
        public override int TotalTimeMs => 2000;     // 保险丝：全连 3×580=1740 内必 EndCast

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.SetPhase(0);
            ctx.PlayAnim(AnimId.SwordmanTripleSlash1);
            ctx.ClearHitTargets();
            ctx.SetAttackHitbox(BoxOffset, BoxHalf);
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int seg = ctx.GetSubState();
            int t = ctx.GetElapsedMs() - ctx.GetPhase();

            // 段内前冲（每帧增量，纯函数回滚安全；撞墙由 MoveCasterForward 截断）。
            // 先整数算出本 tick 位移 ×100（dtMs=50 整除 DashMs=200，无舍入），再 FP 除 100 转单位
            if (t < DashMs)
                ctx.MoveCasterForward((FP)(DashUnitsX100 * dtMs / DashMs) / 100);

            // 攻击盒窗口（终段走 Area，不放本体盒）
            if (seg < SegmentCount - 1)
            {
                if (t < BoxOffMs) ctx.SetAttackHitbox(BoxOffset, BoxHalf);
                else ctx.DisableAttackHitbox();
            }

            // 续段：DNF onProcCon——段内取消点起，缓冲有技能键 → 下一段
            if (t >= CancelMs && seg < SegmentCount - 1 && ctx.PeekBufferedButton() == 13)
            {
                ctx.ConsumeBuffer();
                ctx.SetSubState(seg + 1);
                ctx.SetPhase(ctx.GetElapsedMs());
                ctx.PlayAnim(AnimId.SwordmanTripleSlash1 + seg + 1);
                ctx.ClearHitTargets();
                if (seg + 1 < SegmentCount - 1)
                {
                    ctx.SetAttackHitbox(BoxOffset, BoxHalf);
                }
                else
                {
                    // 终段：本体盒让位 + 前方一次性击倒区域（独立 HitReaction，防双结算）
                    ctx.DisableAttackHitbox();
                    ctx.CreateAreaInFront(AreaIds.TripleSlashFinish, (FP)9 / 10);
                }
            }

            // 收招：本段播完无续段输入（或终段播完）
            if (t >= SegmentMs) ctx.EndCast();
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
