using TrueSync;

namespace ET
{
    /// <summary>
    /// 银光落刃·落地冲击波（ashenforksub.obj 单相位直译：bottom 贴地层 / pass all 全穿透）。
    /// 以落点为中心贴地对称扩散，主打原地击倒浮空（down/lift 300/无 push——与击退型对照）。
    /// 视觉：EarthQuakeRing 主层 + 尘土背层（ashenforksub.ani 帧驱动自推）。
    /// 参考：Notes/技能实现/鬼剑士技���解析/016-AshenFork.md §2.2
    /// </summary>
    [AreaId(AreaIds.AshenFork)]
    public class AshenForkArea : AreaDefinition
    {
        public override int TotalTimeMs => 330;   // PO 动画 6 帧 330ms
        public override int TickTimeMs => 0;      // 一次性（Enter 即结）

        // PO 盒 F0-F3 并集折算（±110×40×17 px → 单位）：贴地扁盒
        public override TSVector HalfExtents => new((FP)11 / 10, (FP)2 / 10, (FP)35 / 100);

        // ashenforksub.atk 直译：down/lift 300/无 push（纯浮空不推走）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 90,
            HitstunMs = 800,
            KnockbackX = 0,
            LaunchY = 300,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.AshenForkSubRing;
        public override int ViewBackAnimId => AnimId.AshenForkSubDust;
    }
}
