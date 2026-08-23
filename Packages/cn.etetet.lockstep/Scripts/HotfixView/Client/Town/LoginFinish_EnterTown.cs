using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 登录完成直接进城镇（替代大厅 UI——UILSLobby 代码保留、不再自动打开；回放入口移到城镇 F9，03 文档 §1.1）。
    /// 出生点 (0,0,0)＝街道中段（阶段 B 按瓦片布局可视后校准）。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class LoginFinish_EnterTown: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene scene, LoginFinish args)
        {
            await TownHelper.EnterTown(scene, new TSVector(0, 0, 0));
        }
    }
}
