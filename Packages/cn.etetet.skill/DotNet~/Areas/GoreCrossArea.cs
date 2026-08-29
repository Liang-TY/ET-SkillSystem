using TrueSync;

namespace ET
{
    /// <summary>
    /// 十字斩·血之十字相位1（gorecross.obj basic motion 直译：GoreCross1.ani 十字闪光升起
    /// 4 帧 320ms + GoreCross2 叠加层）。damage/push30/lift30 + 出血（血气旺盛常开 100%）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/064-GoreCross.md §2.3
    /// </summary>
    [AreaId(AreaIds.GoreCrossCross)]
    public class GoreCrossArea : AreaDefinition
    {
        public override int TotalTimeMs => 320;
        public override int TickTimeMs => 0;

        // PO 攻击盒 F1（x[-83,142] y[-25,50] z[-96,193]）折算
        public override TSVector HalfExtents => new((FP)11 / 10, (FP)4 / 10, (FP)14 / 10);

        // gorecross.atk 直译：damage/push 30/lift 30/cut+blood
        private static readonly HitReaction Reaction = new()
        {
            Damage = 120,
            HitstunMs = 500,
            KnockbackX = 30,
            LaunchY = 30,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit, ActionIds.AddBleedBuff };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.GoreCrossFlash;
        public override int ViewBackAnimId => AnimId.GoreCrossCross;
    }
}
