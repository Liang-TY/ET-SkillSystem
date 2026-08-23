namespace ET.Client
{
    /// <summary>城镇调试输入（视图组件，随 Room 连根拔）：N=匹配进战斗、F9=开大厅 UI（回放调试入口）</summary>
    [ComponentOf(typeof (Room))]
    public class TownOperaComponent: Entity, IAwake, IUpdate, IDestroy
    {
    }
}
