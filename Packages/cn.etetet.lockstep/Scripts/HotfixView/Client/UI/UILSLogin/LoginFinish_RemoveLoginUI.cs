namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class LoginFinish_RemoveUILSLogin: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			// 使用 YIUI 关闭面板替换旧的 ET UI 移除
			await scene.YIUIMgr().ClosePanelAsync<LoginPanelComponent>();
		}
	}
}
