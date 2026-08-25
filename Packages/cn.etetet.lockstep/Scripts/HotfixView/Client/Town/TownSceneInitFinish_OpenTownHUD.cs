namespace ET.Client
{
    /// <summary>
    /// DemoUI：城镇就绪（登录进入/战斗回城统一收口）→ 关战斗 UI，开城镇 HUD（主界面+技能栏）。
    /// BattleHudUnitComponent 随战斗 Room 销毁，无需在此清理。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class TownSceneInitFinish_OpenTownHUD: AEvent<Scene, TownSceneInitFinish>
    {
        protected override async ETTask Run(Scene scene, TownSceneInitFinish args)
        {
            var mgr = scene.YIUIMgr();
            await mgr.ClosePanelAsync<BattleInfoPanelComponent>();
            await mgr.ClosePanelAsync<BattleTipPanelComponent>();
            await mgr.ClosePanelAsync<LoadingPanelComponent>();

            await scene.YIUIRoot().OpenPanelAsync<MainHUDPanelComponent>();
            await scene.YIUIRoot().OpenPanelAsync<SkillHUDPanelComponent>();
        }
    }
}
