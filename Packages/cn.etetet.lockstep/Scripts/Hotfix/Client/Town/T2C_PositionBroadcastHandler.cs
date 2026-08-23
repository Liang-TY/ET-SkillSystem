using TrueSync;

namespace ET.Client
{
    /// <summary>远端玩家位置包（TownScene 转发）：更新插值目标</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PositionBroadcastHandler : MessageHandler<Scene, T2C_PositionBroadcast>
    {
        protected override async ETTask Run(Scene root, T2C_PositionBroadcast message)
        {
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.UpdateRemote(message.PlayerId, message.Position, message.Forward, message.IsMoving);
            await ETTask.CompletedTask;
        }
    }
}
