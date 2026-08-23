namespace ET.Server
{
    /// <summary>
    /// 玩家离开战斗房间（回城镇时 Gate 通知，03 文档 §1.4）：移除房间成员；
    /// 全员走完 → 销毁 RoomRoot 纤程（Remove 必须在本纤程线程内执行——handler 天然满足）。
    /// </summary>
    [MessageHandler(SceneType.RoomRoot)]
    public class G2Room_LeavePlayerHandler : MessageHandler<Scene, G2Room_LeavePlayer>
    {
        protected override async ETTask Run(Scene root, G2Room_LeavePlayer message)
        {
            Room room = root.GetComponent<Room>();
            if (room == null) return;

            RoomServerComponent server = room.GetComponent<RoomServerComponent>();
            server?.GetChild<RoomPlayer>(message.PlayerId)?.Dispose();
            Log.Info($"[Room] 玩家{message.PlayerId}离开房间，剩余 {server?.Children.Count ?? 0}");

            if (server == null || server.Children.Count == 0)
            {
                Log.Info("[Room] 全员走完，销毁 RoomRoot 纤程");
                await FiberManager.Instance.Remove(root.Fiber.Id);
                return;   // 纤程已毁，到此为止
            }
            await ETTask.CompletedTask;
        }
    }
}
