using TrueSync;

namespace ET
{
    /// <summary>
    /// 浴血之怒（U 键；DNF 鬼剑士 75 级二觉大招，技能 ID 229，PVF：sqr/character/swordman/bloodboom）。
    ///
    /// DNF 流程同构：
    /// - checkExecutableSkill：HP ≥ 上限 10% 才能施放（MinCastHpPct，TryCast 前拒绝不进 CD）；
    /// - onSetState：扣自身 5% 上限 HP（level 列0）→ 播 bloodboom.ani（ID 122）；
    /// - onKeyFrameFlag(1) = 帧 22（SET FLAG）：以自身为中心创建爆炸被动对象（→ BloodBoomArea）；
    /// - onEndCurrentAni：回待机。
    ///
    /// 施法特效（bloodboom.ani.als 的 casting/casting_back 两层）由视图层
    /// LSAnimOverlayViewComponent 按 AnimOverlayConfig 自动叠加，逻辑层零参与。
    /// 伤害/出血参数在 BloodBoomArea.HitReaction（DNF 由被动对象 24370 结算，同构）。
    /// </summary>
    [SkillId(SkillIds.BloodBoom)]
    public class BloodBoomSkill : SkillLogic
    {
        private const int BoomFrame = 22;               // bloodboom.ani 帧 22 = SET FLAG 1（爆炸触发帧）
        private static readonly FP HpCostPct = (FP)5;   // skl level 列0：施放消耗 5% 上限 HP
        private static readonly FP MinHpPct = (FP)10;   // skl static[0]：可发动的最低 HP 10%

        public override int CooldownMs => 5000;    // 原版 40s，demo 缩到 5s
        public override int TotalTimeMs => 980;    // bloodboom.ani 23 帧总时长（18×35 + 5×70）

        public override FP MinCastHpPct => MinHpPct;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.SwordmanBloodboom);
            ctx.ClearHitTargets();
            ctx.ConsumeCasterHp(ctx.GetCasterMaxHp() * HpCostPct / 100);   // 自身 HP 消耗（onSetState 同构）
            Log.Info($"[Skill] unit{ctx.GetCasterId()} 浴血之怒施放（消耗 5% 上限 HP，剩余 {ctx.GetCasterHp()}/{ctx.GetCasterMaxHp()}）");
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 帧 22（SET FLAG=1）引爆：SubState 保证只触发一次（DNF 被动对象 z=60 贴身创建 → 距离 0）
            if (ctx.GetSubState() == 0 && ctx.CurrentFrameIndex() >= BoomFrame)
            {
                ctx.SetSubState(1);
                ctx.CreateAreaInFront(AreaIds.BloodBoom, FP.Zero);
                Log.Info($"[Skill] unit{ctx.GetCasterId()} 浴血之怒引爆（帧 {ctx.CurrentFrameIndex()}）");
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();   // onEndCurrentAni → 回待机
        }
    }
}
