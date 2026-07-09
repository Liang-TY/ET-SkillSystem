namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			// 使用 YIUI 面板替换旧的 ET UI 登录界面
			await root.YIUIRoot().OpenPanelAsync<LoginPanelComponent>();
		}
	}
}
