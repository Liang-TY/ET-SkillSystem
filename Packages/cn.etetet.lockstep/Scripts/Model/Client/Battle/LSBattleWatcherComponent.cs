namespace ET.Client
{
    /// <summary>
    /// 战斗胜利监测（客户端=唯一模拟方，03 文档 §1.4）：地图有怪且全部死亡 → **先让战斗继续跑 3 秒**
    /// （收场动画/走动——帧泵不停）→ 到点上报 C2Room_BattleClear → 服务器广播 BattleEnd（此时才停帧）
    /// → 客户端立刻切回城镇。怪物死亡=LSUnit Dispose（AI 组件随之消失，按此判活）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class LSBattleWatcherComponent: Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>地图配置了怪物（无怪地图不触发胜利）</summary>
        public bool HasMonsters;

        /// <summary>已上报（防重复）</summary>
        public bool Reported;

        /// <summary>到点时刻（ClientNow+3000；0=未起表——全灭当帧起表）</summary>
        public long ReportAtTime;
    }
}
