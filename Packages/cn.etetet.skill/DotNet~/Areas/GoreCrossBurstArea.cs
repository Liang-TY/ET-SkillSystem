using TrueSync;

namespace ET
{
    /// <summary>
    /// 十字斩·血之十字相位2 三联爆发（gorecross.obj etc motion 直译：GoreCross3.ani 320ms +
    /// GoreCross4 渐隐收尾）。GoreCrossAdd.atk：down/push 250/lift 300 击倒。
    /// 参考：Notes/技能实现/鬼剑士技能解析/064-GoreCross.md §2.3
    /// </summary>
    [AreaId(AreaIds.GoreCrossBurst)]
    public class GoreCrossBurstArea : AreaDefinition
    {
        public override int TotalTimeMs => 320;
        public override int TickTimeMs => 0;

        public override TSVector HalfExtents => new((FP)14 / 10, (FP)5 / 10, (FP)17 / 10);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 150,
            HitstunMs = 800,
            KnockbackX = 250,
            LaunchY = 300,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.GoreCross3Cross;
        public override int ViewEndAnimId => AnimId.GoreCross3CrossFade;
    }
}
