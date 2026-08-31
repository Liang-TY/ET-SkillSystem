using TrueSync;

namespace ET
{
    /// <summary>
    /// 怒气爆发·内圈血柱（BlastBloodSub.obj 直译：更宽血柱，隐藏第二判定体）。
    /// 参数同外圈——中心敌人被外圈+内圈同时覆盖 → 每 Tick 双份命中 = 中心 8 段
    /// （explain"中心双倍"数学等效；Area Tick 无去重已验证）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/024-BloodBlast.md §5
    /// </summary>
    [AreaId(AreaIds.BloodBlastCore)]
    public class BloodBlastCoreArea : AreaDefinition
    {
        public override int TotalTimeMs => 1800;
        public override int TickTimeMs => 450;

        // Sub 宽柱盒（x[-73,146] z[0,330]）折算
        public override TSVector HalfExtents => new((FP)12 / 10, (FP)10 / 10, (FP)165 / 100);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 90,
            HitstunMs = 500,
            KnockbackX = 0,
            LaunchY = 150,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] TickActionsArr = { ActionIds.MeleeHit };
        public override int[] TickActions => TickActionsArr;

        public override int ViewAnimId => AnimId.BlastBloodCore;
    }
}
