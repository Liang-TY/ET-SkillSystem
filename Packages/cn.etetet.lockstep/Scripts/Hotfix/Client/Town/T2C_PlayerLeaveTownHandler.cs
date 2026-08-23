namespace ET.Client
{
    /// <summary>有玩家离开城镇（TownScene 广播）：移除远端角色视图</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PlayerLeaveTownHandler : MessageHandler<Scene, T2C_PlayerLeaveTown>
    {
        protected override async ETTask Run(Scene root, T2C_PlayerLeaveTown message)
        {
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.RemoveRemote(message.PlayerId);
            await ETTask.CompletedTask;
        }
    }
}
