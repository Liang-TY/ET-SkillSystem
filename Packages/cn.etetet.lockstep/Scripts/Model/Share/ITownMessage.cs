namespace ET
{
    /// <summary>
    /// 城镇 Actor 消息接口（抄 IRoomMessage）：Gate 收到后填 PlayerId → 按 PlayerTownComponent.TownActorId
    /// 转发到 TownScene 纤程（NetComponentOnReadInvoker_Gate 的 case，03 文档 §1.2 路由表）。
    /// </summary>
    public interface ITownMessage: IMessage
    {
        long PlayerId { get; set; }
    }
}
