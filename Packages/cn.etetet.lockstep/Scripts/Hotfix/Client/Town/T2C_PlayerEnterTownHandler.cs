using TrueSync;

namespace ET.Client
{
    /// <summary>有玩家进城镇（TownScene 广播）：创建远端角色视图</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PlayerEnterTownHandler : MessageHandler<Scene, T2C_PlayerEnterTown>
    {
        protected override async ETTask Run(Scene root, T2C_PlayerEnterTown message)
        {
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.CreateRemote(message.PlayerId, message.Position);
            await ETTask.CompletedTask;
        }
    }
}
