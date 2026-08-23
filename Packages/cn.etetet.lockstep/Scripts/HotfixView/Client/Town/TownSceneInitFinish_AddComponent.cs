namespace ET.Client
{
    /// <summary>城镇初始化收尾（表现层���：挂调试输入（N 匹配/F9 大厅）；阶段 B 追加角色视图/相机</summary>
    [Event(SceneType.LockStep)]
    public class TownSceneInitFinish_AddComponent: AEvent<Scene, TownSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            room.AddComponent<TownOperaComponent>();
            await ETTask.CompletedTask;
        }
    }
}
