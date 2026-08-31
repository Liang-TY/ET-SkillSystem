using TrueSync;

namespace ET
{
    /// <summary>
    /// 破军升龙击·上挑（chargecrashupper.atk 直译：down / lift 400 浮空挑起）。
    /// 上挑段 F2 时刻身前召唤（独立反应——冲撞段走技能 HitReaction）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/068-ChargeCrash.md §5
    /// </summary>
    [AreaId(AreaIds.ChargeCrashUpper)]
    public class ChargeCrashUpperArea : AreaDefinition
    {
        public override int TotalTimeMs => 200;
        public override int TickTimeMs => 0;

        // Upper F3 盒折算（x[-30,160] y[-15,30] z[39,140]）
        public override TSVector HalfExtents => new((FP)83 / 100, (FP)4 / 10, (FP)55 / 100);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 120,
            HitstunMs = 800,
            KnockbackX = 0,
            LaunchY = 400,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.ChargeCrashUpSlash;
    }
}
