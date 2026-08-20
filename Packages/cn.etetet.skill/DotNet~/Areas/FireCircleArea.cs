using TrueSync;

namespace ET
{
    /// <summary>火圈：在施法者前方生成持续燃烧区域，单位在内每 1 秒扣 10 血（FireDamageTick 复用）。</summary>
    [AreaId(AreaIds.FireCircle)]
    public class FireCircleArea : AreaDefinition
    {
        public override int TotalTimeMs => 5000;    // 持续 5 秒
        public override int TickTimeMs => 1000;     // 每 1 秒烧一次

        public override TSVector HalfExtents => new((FP)15 / 10, (FP)5 / 10, (FP)15 / 10);   // 3×1×3 单位

        private static readonly int[] TickActionsArr = { ActionIds.FireDamageTick };
        public override int[] TickActions => TickActionsArr;

        public override int ViewAnimId => AnimId.FireCircle;      // 循环火焰
        public override int ViewEndAnimId => AnimId.FireCircleEnd; // 熄灭收尾
    }
}
