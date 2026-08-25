namespace ET.Client
{
    /// <summary>
    /// DemoUI：战斗结束回城 → 开 Loading（"正在返回城镇…"）；
    /// 到镇后由 TownSceneInitFinish_OpenTownHUD 统一收口关闭。
    /// </summary>
    [Event(SceneType.LockStep)]
    [FriendOf(typeof(LoadingPanelComponent))]
    public class ReturnTown_ShowLoading: AEvent<Scene, ReturnTown>
    {
        protected override async ETTask Run(Scene scene, ReturnTown args)
        {
            LoadingPanelComponent loading = await scene.YIUIRoot().OpenPanelAsync<LoadingPanelComponent>();
            if (loading != null)
                loading.u_ComTextSub.text = "正在返回城镇…";
        }
    }
}
