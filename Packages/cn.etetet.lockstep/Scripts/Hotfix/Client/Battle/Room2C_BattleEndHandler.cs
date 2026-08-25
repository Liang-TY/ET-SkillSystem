namespace ET.Client
{
    /// <summary>
    /// 战斗结束（服务器广播）：立刻切回城镇（恢复记住的位置，03 文档 §1.4）。
    /// 收场 3 秒在客户端上报前已经等过（LSBattleWatcherComponent.ClearDelayMs）——
    /// 到这一步时帧已停、立刻走人。
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_BattleEndHandler : MessageHandler<Scene, Room2C_BattleEnd>
    {
        protected override async ETTask Run(Scene root, Room2C_BattleEnd message)
        {
            Log.Info("[Battle] 收到 BattleEnd，回城镇");
            EventSystem.Instance.Publish(root, new ReturnTown());
            await TownHelper.EnterTown(root, TownMemory.LastTownPosition);
        }
    }
}
