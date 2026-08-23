namespace ET.Server
{
    /// <summary>
    /// TownScene 常驻纤程初始化（抄 FiberInit_Match）：MMO 模式不模拟只转发，
    /// 常驻永不销毁（不调 FiberManager.Remove），玩家进出只动 TownComponent.Members。
    /// </summary>
    [Invoke(SceneType.Town)]
    public class FiberInit_Town: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();
            root.AddComponent<TownComponent>();

            await ETTask.CompletedTask;
        }
    }
}
