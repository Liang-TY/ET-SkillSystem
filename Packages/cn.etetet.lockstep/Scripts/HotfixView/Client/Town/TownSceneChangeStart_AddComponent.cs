using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// 城镇场景切换：加载场景 → 注册动画 clip → NPK挂载 → 作用域加载（角色+城镇瓦片）。
    /// </summary>
    [Event(SceneType.LockStep)]
    public class TownSceneChangeStart_AddComponent: AEvent<Scene, TownSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneChangeStart args)
        {
            Room room = args.Room;
            ResourcesLoaderComponent resourcesLoaderComponent = room.AddComponent<ResourcesLoaderComponent>();
            await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.demores/Scenes/Game.unity", LoadSceneMode.Single);

            // 动画 clip 注册
            await LSAnimClipRegistrar.RegisterAll(clientScene);

            // NPK 挂载（启动时一次）
            NpkLoaderComponent npkLoader = room.GetComponent<NpkLoaderComponent>();
            if (npkLoader == null)
            {
                npkLoader = room.AddComponent<NpkLoaderComponent>();
                await npkLoader.LoadAllNpks();
            }

            // 作用域加载：角色常驻 + 城镇瓦片（不含怪物/副本特效）
            LSAnimResComponent animRes = room.GetComponent<LSAnimResComponent>();
            if (animRes == null)
                animRes = room.AddComponent<LSAnimResComponent>();
            ResourceScopeComponent scope = room.GetComponent<ResourceScopeComponent>();
            if (scope == null)
                scope = room.AddComponent<ResourceScopeComponent>();

            // 先收集城镇瓦片（需要从 tile layout 读）
            // TODO: 这里先加载角色+全量动画，城镇瓦片由 TownMapViewComponent 自己处理
            // 后续 TownMapViewComponent 也改成走作用域
            var townImgs = ResourceDependencyCollector.CollectForDungeon(); // 暂用全量（含怪物，城镇会多加载但不会崩）
            await scope.LoadScope("anim", "town", townImgs, animRes);

            // 瓦片地面 + 客户端权威碰撞
            await room.AddComponent<TownMapViewComponent>().InitAsync();
        }
    }
}
