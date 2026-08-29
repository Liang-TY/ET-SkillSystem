using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·崩山击（hopsmashready 400ms 蓄力 + hopsmash 900ms 前跃下砸）。
    /// DNF：前跃多段下砸 2-3 次（F3-F5 每帧盒），末击击倒；已学血气旺盛则落地冲击波+出血。
    /// 多段：F3/F4/F5 帧驱动盒自动激活 + 过帧界 ClearHitTargets（resetHitObjectList 同构），
    /// 多段/末击不同反应走 PhaseHitReaction(SubState)（替代"末击走小 Area"手法）。
    /// 霸体 F2-F6（540ms，SUPERARMOR）—— SetCasterSuperArmor 一次性设。
    /// 简化：蓄力按放变距固定中档（前跃 1.5 单位）、血气旺盛常开、等级缩放延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/065-HopSmash.md
    /// </summary>
    [SkillId(SkillIds.HopSmash)]
    public class HopSmashSkill : SkillLogic
    {
        // 多段（hopsmash.atk 直译：damage/push 30/lift 30）
        private static readonly HitReaction MultiHit = new()
        {
            Damage = 40,
            HitstunMs = 300,
            KnockbackX = 30,
            LaunchY = 30,
        };

        // 末击（hopsmashfinal.atk 直译：down/push 200/lift 200）
        private static readonly HitReaction Finisher = new()
        {
            Damage = 80,
            HitstunMs = 700,
            KnockbackX = 200,
            LaunchY = 200,
        };

        public override HitReaction HitReaction => MultiHit;

        // phase = SubState：2/3=多段第1/2击（F3/F4 帧），4=末击（F5 帧）
        public override HitReaction PhaseHitReaction(int phase) => phase >= 4 ? Finisher : MultiHit;

        public override int CooldownMs => 4000;    // .skl [dungeon][cool time]
        public override int TotalTimeMs => 1300;   // ready 400 + smash 900

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 时序（smash 动画相对时长换算 + ready 400ms 偏移）
        private const int SmashAtMs = 400;         // ready 播完切下砸
        private const int HopEndMs = 820;          // 前跃段结束（F0-F2 = 420ms）
        private const int SuperArmorAtMs = 760;    // 霸体起点（F2），时长 540ms 到 1300
        private const int Hit1AtMs = 820;          // F3 首击（帧驱动盒自动激活）
        private const int Hit2AtMs = 1000;         // F4 过帧界清命中表
        private const int Hit3AtMs = 1060;         // F5 过帧界清命中表（末击）
        private const int WaveAtMs = 1120;         // 落地冲击波
        private const int HopDistanceX100 = 150;   // 前跃 1.5 单位（DNF 200px 档折中）

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanHopSmashReady);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            int t = ctx.GetElapsedMs();

            // 蓄力 → 前跃下砸
            if (ctx.GetSubState() == 0 && t >= SmashAtMs)
            {
                ctx.SetSubState(1);
                ctx.PlayAnim(AnimId.SwordmanHopSmash);
            }

            // 前跃（水平位移近似低跃，动画承担视觉；撞墙由 MoveCasterForward 截断）
            if (t >= SmashAtMs && t < HopEndMs)
                ctx.MoveCasterForward((FP)HopDistanceX100 / 100 * dtMs / (HopEndMs - SmashAtMs));

            // 霸体（F2-F6 一次性设 540ms）
            if (ctx.GetSubState() < 2 && t >= SuperArmorAtMs)
            {
                ctx.SetSubState(2);
                ctx.SetCasterSuperArmor(TotalTimeMs - SuperArmorAtMs);
            }

            // 多段过帧界：清命中表（帧驱动盒自动继续激活）
            if (ctx.GetSubState() == 2 && t >= Hit1AtMs) ctx.SetSubState(3);
            if (ctx.GetSubState() == 3 && t >= Hit2AtMs)
            {
                ctx.SetSubState(4);
                ctx.ClearHitTargets();
            }
            if (ctx.GetSubState() == 4 && t >= Hit3AtMs)
            {
                ctx.SetSubState(5);
                ctx.ClearHitTargets();
            }

            // 落地冲击波（血气旺盛常开）
            if (ctx.GetSubState() == 5 && t >= WaveAtMs)
            {
                ctx.SetSubState(6);
                ctx.CreateAreaInFront(AreaIds.HopSmashWave, (FP)5 / 10);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
