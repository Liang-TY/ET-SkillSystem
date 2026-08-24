namespace ET.Client
{
    /// <summary>
    /// DemoUI：登录完成 → 开选角面板（替代旧的"直进城镇"；选角确认后由 RoleSelectPanel 进城镇）。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class LoginFinish_OpenRoleSelect: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene scene, LoginFinish args)
        {
            await scene.YIUIRoot().OpenPanelAsync<RoleSelectPanelComponent>();
        }
    }
}
