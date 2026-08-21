namespace ET
{
    /// <summary>
    /// 出血：持续 3 秒每 1 秒 Tick 扣 15（BleedDamageTick）。叠层 = 刷新时长（与燃烧同构）。
    /// DNF 出处（血气旺盛技能 63 开启时的 level 列2-5）：出血机率/持续时间/攻击力/对出血敌人增伤率
    /// ——demo 固定值，等级化时随 skl 数据进表。
    /// </summary>
    [BuffId(BuffIds.Bleed)]
    public class BleedBuff : BuffDefinition
    {
        public override int TotalTimeMs => 3000;
        public override int TickTimeMs => 1000;

        private static readonly int[] TickActionsArr = { ActionIds.BleedDamageTick };
        public override int[] TickActions => TickActionsArr;
    }
}
