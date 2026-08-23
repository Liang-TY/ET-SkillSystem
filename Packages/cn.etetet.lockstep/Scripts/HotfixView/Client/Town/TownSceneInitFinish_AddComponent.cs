namespace ET.Client
{
    /// <summary>城镇初始化收尾（表现层）：本地玩家视图（Unit2D+分层渲染）+ 调试输入（WASD/N/F9）</summary>
    [Event(SceneType.LockStep)]
    public class TownSceneInitFinish_AddComponent: AEvent<Scene, TownSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            await room.AddComponent<TownPlayerViewComponent>().InitAsync();
            room.AddComponent<TownOperaComponent>();
        }
    }
}
