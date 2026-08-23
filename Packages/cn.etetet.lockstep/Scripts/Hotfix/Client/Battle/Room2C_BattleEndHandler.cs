namespace ET.Client
{
    /// <summary>战斗结束（服务器广播）：等 3 秒收场 → 回城镇（恢复记住的位置，03 文档 §1.4）</summary>
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_BattleEndHandler : MessageHandler<Scene, Room2C_BattleEnd>
    {
        protected override async ETTask Run(Scene root, Room2C_BattleEnd message)
        {
            Log.Info("[Battle] 收到 BattleEnd，3 秒后回城镇");
            await root.GetComponent<TimerComponent>().WaitAsync(3000);
            await TownHelper.EnterTown(root, TownMemory.LastTownPosition);
        }
    }
}
