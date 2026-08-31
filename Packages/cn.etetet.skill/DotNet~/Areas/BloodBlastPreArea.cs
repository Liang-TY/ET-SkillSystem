using TrueSync;

namespace ET
{
    /// <summary>
    /// 怒气爆发·前段（BlastBloodPreSub.obj 直译：地面爆发圈渐大 F1-F5，先手浮空）。
    /// BlastBloodPreSub.atk：down / push 0 / lift 400。
    /// 参考：Notes/技能实现/鬼剑士技能解析/024-BloodBlast.md §2.3
    /// </summary>
    [AreaId(AreaIds.BloodBlastPre)]
    public class BloodBlastPreArea : AreaDefinition
    {
        public override int TotalTimeMs => 400;
        public override int TickTimeMs => 0;

        // presub F5 盒（x[-127,254] y[-24,48] z[-64,116]）折算
        public override TSVector HalfExtents => new((FP)19 / 10, (FP)4 / 10, (FP)9 / 10);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 60,
            HitstunMs = 500,
            KnockbackX = 0,
            LaunchY = 400,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.BlastBloodPre;
        public override int ViewBackAnimId => AnimId.BlastBloodPreFront;
    }
}
