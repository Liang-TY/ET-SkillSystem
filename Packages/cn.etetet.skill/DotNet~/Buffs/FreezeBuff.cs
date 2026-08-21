namespace ET
{
    /// <summary>
    /// 冰冻：3.5 秒定身（ForbidMove 开/关，Stun 同构）。
    /// DNF 出处：冰息弹 .atk [freeze] 10 20 3500（10% 概率/等级 20/3.5 秒）——概率在 HitReaction.ProcChance，
    /// 时长取预设（.atk 参数化记档 02 文档 §9）。冰蓝色调染色表现延后，当前仅定身。
    /// </summary>
    [BuffId(BuffIds.Freeze)]
    public class FreezeBuff : BuffDefinition
    {
        public override int TotalTimeMs => 3500;

        private static readonly int[] AddActionsArr = { ActionIds.ForbidMoveOn };
        public override int[] AddActions => AddActionsArr;

        private static readonly int[] RemoveActionsArr = { ActionIds.ForbidMoveOff };
        public override int[] RemoveActions => RemoveActionsArr;
    }
}
