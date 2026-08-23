namespace ET.Server
{
    /// <summary>
    /// 客户端上报怪物全灭（客户端唯一模拟方检测，03 文档 §1.4）：广播 Room2C_BattleEnd +
    /// 停止帧输入收集（LSServerUpdater 看 BattleEnded）；**不销毁纤程**——等玩家走完由
    /// G2Room_LeavePlayer 逐个摘、全空自毁（频繁进出不反复建拆纤程）。
    /// </summary>
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_BattleClearHandler : MessageHandler<Scene, C2Room_BattleClear>
    {
        protected override async ETTask Run(Scene root, C2Room_BattleClear message)
        {
            Room room = root.GetComponent<Room>();
            if (room == null || room.BattleEnded) return;

            room.BattleEnded = true;
            RoomMessageHelper.BroadCast(room, Room2C_BattleEnd.Create());
            Log.Info($"[Room] 玩家{message.PlayerId}上报战斗胜利，广播 BattleEnd（纤程保留等玩家走完）");
            await ETTask.CompletedTask;
        }
    }
}
