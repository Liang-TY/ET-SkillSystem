namespace ET
{
    /// <summary>眩晕：1 秒禁移动（ForbidMove 开/关成对，AddActions/RemoveActions）。</summary>
    [BuffId(BuffIds.Stun)]
    public class StunBuff : BuffDefinition
    {
        public override int TotalTimeMs => 1000;

        private static readonly int[] AddActionsArr = { ActionIds.ForbidMoveOn };
        public override int[] AddActions => AddActionsArr;

        private static readonly int[] RemoveActionsArr = { ActionIds.ForbidMoveOff };
        public override int[] RemoveActions => RemoveActionsArr;
    }
}
