namespace ET
{
    /// <summary>燃烧：持续 3 秒，每 1 秒 Tick 扣 10（FireDamageTickAction）。叠层=刷新时长。</summary>
    [BuffId(BuffIds.Burn)]
    public class BurnBuff : BuffDefinition
    {
        public override int TotalTimeMs => 3000;
        public override int TickTimeMs => 1000;

        private static readonly int[] TickActionsArr = { ActionIds.FireDamageTick };
        public override int[] TickActions => TickActionsArr;
    }
}
