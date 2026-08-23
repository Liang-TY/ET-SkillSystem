namespace ET.Server
{
    /// <summary>
    /// 城镇位置包：更新成员表 → 转发给其他成员（不校验不模拟——纯中继站，03 文档 §1.1）。
    /// 客户端同步开关默认关，单人 demo 无包；多人打开即用。
    /// </summary>
    [MessageHandler(SceneType.Town)]
    [FriendOf(typeof(TownComponent))]
    public class C2T_PositionUpdateHandler: MessageHandler<Scene, C2T_PositionUpdate>
    {
        protected override async ETTask Run(Scene root, C2T_PositionUpdate message)
        {
            TownComponent town = root.GetComponent<TownComponent>();

            if (town.Members.TryGetValue(message.PlayerId, out TownPlayerInfo info))
            {
                info.Position = message.Position;
                info.Forward = message.Forward;
            }

            T2C_PositionBroadcast broadcast = T2C_PositionBroadcast.Create();
            broadcast.PlayerId = message.PlayerId;
            broadcast.Position = message.Position;
            broadcast.Forward = message.Forward;
            broadcast.IsMoving = message.IsMoving;

            MessageLocationSenderComponent sender = root.GetComponent<MessageLocationSenderComponent>();
            foreach (var kv in town.Members)
            {
                if (kv.Key == message.PlayerId) continue;
                sender.Get(LocationType.GateSession).Send(kv.Key, broadcast);
            }

            await ETTask.CompletedTask;
        }
    }
}
