namespace ET
{
    /// <summary>
    /// 鬼剑士普攻三段连击（attack1/2/3.ani，单 cast 段状态机——三段斩同款）。
    /// DNF：普攻 X 连打 1→2→3 循环；每段取消窗口起接受下一段输入，播完无输入收招。
    /// 状态全在 LSCast（SubState=段号，Phase=段起点累计 ms），cast 销毁即归零，回滚安全。
    /// 攻击盒：段1/2 手动盒（.ani 无 attackBox，DNF 玩家攻击盒原版由 nut 动态创建）；
    /// 段3 attack3.json 自带 F2/F3 盒 → 帧驱动自动激活，手动盒在换段时关掉让位。
    /// 参考：Notes/技能实现/鬼剑士技能解析/（attack1-3 json 实测帧表）
    /// </summary>
    [SkillId(SkillIds.NormalAttack)]
    public class NormalAttack : SkillLogic
    {
        private const int SegmentCount = 3;

        // 段时长（json 直译：attack1 600 / attack2 650 / attack3 550）
        private static readonly int[] SegmentMs = { 600, 650, 550 };

        // 段内取消点（DNF 半程惯例，attack1 F6=300ms；三段统一 300——手感连贯）
        private const int CancelMs = 300;

        // 段1/2 手动盒窗口（挥砍半程）
        private const int BoxOnMs = 50;
        private const int BoxOffMs = 400;

        // 段1/2 攻击盒（DNF 太刀普攻：面前 ~1 单位）——注意 DNF(x横向,y纵深,z高度)→TSVector(x,y=高度,z=纵深)
        private static readonly TrueSync.TSVector Box12Offset = new((TrueSync.FP)8 / 10, (TrueSync.FP)5 / 10, TrueSync.FP.Zero);
        private static readonly TrueSync.TSVector Box12Half = new((TrueSync.FP)4 / 10, (TrueSync.FP)5 / 10, (TrueSync.FP)3 / 10);

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit, ActionIds.AddBurnBuff };
        public override int[] HitActions => HitActionsArr;

        // 空中攻击链（DNF 空中连斩同构：首击 jumpattack 普攻数值，第 2 击起链斩交替技能数值）
        private static readonly HitReaction AirReaction = new()
        {
            Damage = 60,
            HitstunMs = 500,
            KnockbackX = 270,
            LaunchY = 180,
        };

        // phase = SubState：地面段 0-2 → 默认反应；空中段 10-12 → 链斩反应（jumpattack.atk 直译）
        public override HitReaction PhaseHitReaction(int phase) => phase >= 10 ? AirReaction : HitReaction;

        public override int CooldownMs => 0;
        public override int TotalTimeMs => 2000;   // 保险丝：地面全连 1800 / 空中链 ~1300 内必 EndCast

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetPhase(0);
            ctx.ClearHitTargets();
            if (ctx.IsCasterAirborne())
            {
                // 空中链：首击 = jumpattack（与银光落刃共用动作，F2 盒帧驱动自动激活）
                ctx.SetSubState(10);
                ctx.PlayAnim(AnimId.SwordmanJumpAttack);
                return;
            }
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanAttack1);
            ctx.SetAttackHitbox(Box12Offset, Box12Half);
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            if (ctx.GetSubState() >= 10)
            {
                UpdateAir(ctx);
                return;
            }

            int seg = ctx.GetSubState();
            int t = ctx.GetElapsedMs() - ctx.GetPhase();   // 段内时间

            // 段内攻击盒（仅段1/2；段3 帧驱动自动接管）
            if (seg < SegmentCount - 1)
            {
                if (t >= BoxOnMs && t < BoxOffMs)
                    ctx.SetAttackHitbox(Box12Offset, Box12Half);
                else if (t >= BoxOffMs)
                    ctx.DisableAttackHitbox();
            }

            // 续段：取消窗口起，缓冲有普攻键 → 下一段（DNF 连打循环 1→2→3→1）
            if (t >= CancelMs && ctx.PeekBufferedButton() == 1)
            {
                int next = (seg + 1) % SegmentCount;
                ctx.ConsumeBuffer();
                ctx.SetSubState(next);
                ctx.SetPhase(ctx.GetElapsedMs());
                ctx.PlayAnim(AnimId.SwordmanAttack1 + next);
                ctx.ClearHitTargets();
                if (next < SegmentCount - 1)
                    ctx.SetAttackHitbox(Box12Offset, Box12Half);
                else
                    ctx.DisableAttackHitbox();   // 段3 帧驱动盒接管（attack3.json F2/F3）
                return;
            }

            // 收招：本段播完无续段输入
            if (t >= SegmentMs[seg])
            {
                ctx.EndCast();
            }
        }

        // ---- 空中攻击链（SubState 10=首击 jumpattack，11/12=链斩交替）----
        private const int AirFirstMs = 300;        // jumpattack 6 帧 300ms
        private const int AirSlashMs = 370;        // jumpattackmultislash 5 帧 370ms
        private const int AirBoxOnMs = 50;
        private const int AirBoxOffMs = 300;

        private static void UpdateAir(SkillContext ctx)
        {
            // 落地：链终止（DNF 落地回地面状态）
            if (!ctx.IsCasterAirborne())
            {
                ctx.EndCast();
                return;
            }

            int seg = ctx.GetSubState();
            int t = ctx.GetElapsedMs() - ctx.GetPhase();

            // 链斩段（json 无攻击盒 → 手动盒；首击 jumpattack 帧驱动自动）
            if (seg > 10)
            {
                if (t >= AirBoxOnMs && t < AirBoxOffMs)
                    ctx.SetAttackHitbox(Box12Offset, Box12Half);
                else if (t >= AirBoxOffMs)
                    ctx.DisableAttackHitbox();
            }

            // 首击播完 / 链斩段推进：按 X 交替 1↔2（DNF 空中攻击键连打同构）
            int segMs = seg == 10 ? AirFirstMs : AirSlashMs;
            if (t >= segMs && ctx.PeekBufferedButton() == 1)
            {
                ctx.ConsumeBuffer();
                int next = seg == 10 ? 11 : (seg == 11 ? 12 : 11);
                ctx.SetSubState(next);
                ctx.SetPhase(ctx.GetElapsedMs());
                ctx.PlayAnim(AnimId.JumpAttackMultiSlash1 + (next - 11));
                ctx.ClearHitTargets();
                return;
            }

            // 无输入：首击播完收招 / 链斩段播完收招（滞空重按 X 再续）
            if (t >= segMs)
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
