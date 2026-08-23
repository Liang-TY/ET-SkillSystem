namespace ET.Client
{
    /// <summary>
    /// 战斗胜利监测（客户端=唯一模拟方，03 文档 §1.4）：地图有怪且全部死亡 → 上报 C2Room_BattleClear（一次性）。
    /// 服务器据此广播 Room2C_BattleEnd。怪物死亡=LSUnit Dispose（AI 组件随之消失，按此判活）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class LSBattleWatcherComponent: Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>地图配置了怪物（无怪地图不触发胜利）</summary>
        public bool HasMonsters;

        /// <summary>已上报（防重复）</summary>
        public bool Reported;
    }
}
