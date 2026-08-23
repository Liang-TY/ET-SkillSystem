namespace ET.Server
{
    /// <summary>
    /// 客户端请求进城镇（登录后 / 战斗结束回城）：挂 PlayerTownComponent（Gate 路由切到城镇）
    /// → 转发 TownScene 加入成员 → 回已有成员列表。GetOrAdd 语义——AddComponent 同类型重复挂会抛。
    /// </summary>
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_EnterTownHandler : MessageSessionHandler<C2G_EnterTown, T2C_EnterTownConfirm>
    {
        protected override async ETTask Run(Session session, C2G_EnterTown request, T2C_EnterTownConfirm response)
        {
            Player player = session.GetComponent<SessionPlayerComponent>().Player;

            // 战斗→城镇的路由切换（03 文档 §1.4）：通知 RoomRoot 移除成员（全员走完纤程自毁）→ 摘 PlayerRoomComponent
            PlayerRoomComponent roomComponent = player.GetComponent<PlayerRoomComponent>();
            if (roomComponent != null)
            {
                G2Room_LeavePlayer g2RoomLeave = G2Room_LeavePlayer.Create();
                g2RoomLeave.PlayerId = player.Id;
                session.Root().GetComponent<MessageSender>().Send(roomComponent.RoomActorId, g2RoomLeave);   // Send 是 void（投递即返回）
                player.RemoveComponent<PlayerRoomComponent>();
            }

            PlayerTownComponent townComponent = player.GetComponent<PlayerTownComponent>();
            if (townComponent == null)
            {
                townComponent = player.AddComponent<PlayerTownComponent>();
            }
            townComponent.TownActorId = TownSceneHolder.TownActorId;

            G2Town_EnterPlayer g2TownEnter = G2Town_EnterPlayer.Create();
            g2TownEnter.PlayerId = player.Id;
            Town2G_EnterPlayer townResponse = await session.Root().GetComponent<MessageSender>()
                .Call(TownSceneHolder.TownActorId, g2TownEnter) as Town2G_EnterPlayer;
            if (townResponse != null)
            {
                response.Members.AddRange(townResponse.Members);
            }
        }
    }
}
