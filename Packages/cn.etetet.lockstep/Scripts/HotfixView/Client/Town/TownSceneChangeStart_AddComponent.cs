using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>城镇场景切换（表现层）：加载 Game.unity（城镇/战斗共用，03 文档 §6）；阶段 B 追加城镇瓦片视图</summary>
    [Event(SceneType.LockStep)]
    public class TownSceneChangeStart_AddComponent: AEvent<Scene, TownSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneChangeStart args)
        {
            Room room = args.Room;
            ResourcesLoaderComponent resourcesLoaderComponent = room.AddComponent<ResourcesLoaderComponent>();
            await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.demores/Scenes/Game.unity", LoadSceneMode.Single);
        }
    }
}
