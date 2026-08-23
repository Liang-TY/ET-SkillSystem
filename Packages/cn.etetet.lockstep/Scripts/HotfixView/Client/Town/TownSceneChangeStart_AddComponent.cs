using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// 城镇场景切换（表现层）：加载 Game.unity（共用场景）→ 动画 clip 注册+图集 → 城镇瓦片地面+碰撞+叠图（阶段B）。
    /// LSAnimResComponent 须早于 TownPlayerViewComponent.InitAsync（InitFinish 阶段）。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class TownSceneChangeStart_AddComponent: AEvent<Scene, TownSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneChangeStart args)
        {
            Room room = args.Room;
            ResourcesLoaderComponent resourcesLoaderComponent = room.AddComponent<ResourcesLoaderComponent>();
            await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.demores/Scenes/Game.unity", LoadSceneMode.Single);

            // 角色 Idle/Walk 帧动画资源（同战斗链顺序：先注册 clip 再载图集）
            await LSAnimClipRegistrar.RegisterAll(clientScene);
            await room.AddComponent<LSAnimResComponent>().InitAsync();

            // 瓦片地面 + 客户端权威碰撞 + 绿红调试叠图
            await room.AddComponent<TownMapViewComponent>().InitAsync();
        }
    }
}
