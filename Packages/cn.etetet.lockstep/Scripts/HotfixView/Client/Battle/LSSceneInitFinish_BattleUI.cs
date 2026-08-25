namespace ET.Client
{
    /// <summary>
    /// DemoUI：战斗场景就绪 → 关 Loading、开 BattleInfo（怪条隐藏）、挂 HUD 轮询组件。
    /// 回放模式不开 HUD（与 LSBattleWatcher 同策略）。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class LSSceneInitFinish_BattleUI: AEvent<Scene, LSSceneInitFinish>
    {
        protected override async ETTask Run(Scene scene, LSSceneInitFinish args)
        {
            await scene.YIUIMgr().ClosePanelAsync<LoadingPanelComponent>();

            BattleInfoPanelComponent panel = await scene.YIUIRoot().OpenPanelAsync<BattleInfoPanelComponent>();
            panel?.HideMonster();

            Room room = scene.GetComponent<Room>();
            if (room != null && !room.IsReplay)
                room.AddComponent<BattleHudUnitComponent, BattleInfoPanelComponent>(panel);
        }
    }
}
