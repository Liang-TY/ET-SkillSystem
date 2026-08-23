namespace ET.Server
{
    /// <summary>玩家离开城镇（匹配进战斗时 Gate 通知）：移除成员 → 广播 LeaveTown 给剩余成员</summary>
    [MessageHandler(SceneType.Town)]
    [FriendOf(typeof(TownComponent))]
    public class G2Town_LeavePlayerHandler: MessageHandler<Scene, G2Town_LeavePlayer>
    {
        protected override async ETTask Run(Scene root, G2Town_LeavePlayer message)
        {
            TownComponent town = root.GetComponent<TownComponent>();
            if (!town.Members.Remove(message.PlayerId)) return;

            T2C_PlayerLeaveTown leave = T2C_PlayerLeaveTown.Create();
            leave.PlayerId = message.PlayerId;
            MessageLocationSenderComponent sender = root.GetComponent<MessageLocationSenderComponent>();
            foreach (var kv in town.Members)
            {
                sender.Get(LocationType.GateSession).Send(kv.Key, leave);
            }

            Log.Info($"[Town] 玩家{message.PlayerId}离开城镇，剩余 {town.Members.Count}");
            await ETTask.CompletedTask;
        }
    }
}
