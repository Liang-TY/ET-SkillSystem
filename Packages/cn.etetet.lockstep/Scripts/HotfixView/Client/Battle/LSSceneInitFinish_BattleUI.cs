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
            // 移除旧 ET UI（如果匹配/进战斗流程还会弹出 UILSLobby/UILSRoom）
            await UIHelper.Remove(scene, UIType.UILSLobby);
            await UIHelper.Remove(scene, UIType.UILSRoom);

            await scene.YIUIMgr().ClosePanelAsync<LoadingPanelComponent>();

            BattleInfoPanelComponent panel = await scene.YIUIRoot().OpenPanelAsync<BattleInfoPanelComponent>();
            panel?.HideMonster();

            Room room = scene.GetComponent<Room>();
            if (room != null && !room.IsReplay)
                room.AddComponent<BattleHudUnitComponent, BattleInfoPanelComponent>(panel);
        }
    }
}
