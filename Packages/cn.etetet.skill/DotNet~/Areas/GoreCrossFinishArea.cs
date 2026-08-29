using TrueSync;

namespace ET
{
    /// <summary>
    /// 十字斩·强力追击（gorecrossmercilessness.atk 直译：down/push 120/lift 300/blood 80）。
    /// 召唤瞬间按技能键触发（剑魂/狂战士系分支——demo 无分支常开）；无专属动画，
    /// 视觉复用三联爆发（引擎版追击动画未考证）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/064-GoreCross.md §5
    /// </summary>
    [AreaId(AreaIds.GoreCrossFinish)]
    public class GoreCrossFinishArea : AreaDefinition
    {
        public override int TotalTimeMs => 280;
        public override int TickTimeMs => 0;

        public override TSVector HalfExtents => new((FP)14 / 10, (FP)5 / 10, (FP)17 / 10);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 250,
            HitstunMs = 800,
            KnockbackX = 120,
            LaunchY = 300,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit, ActionIds.AddBleedBuff };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.GoreCross3Cross;
    }
}
