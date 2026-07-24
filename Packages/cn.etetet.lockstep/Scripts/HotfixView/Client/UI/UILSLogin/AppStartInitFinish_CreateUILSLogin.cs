namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			// 使用 YIUI Panel
			Log.Info("[ScrollTest] Entry: opening TestScrollPanel");
			await root.YIUIRoot().OpenPanelAsync<LtyTestPanelComponent>();
			Log.Info("[ScrollTest] Entry: panel opened");
		}
	}
}
