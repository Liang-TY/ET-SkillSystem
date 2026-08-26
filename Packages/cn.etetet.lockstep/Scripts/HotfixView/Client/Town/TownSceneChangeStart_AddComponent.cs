using UnityEngine;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// 城镇场景切换：加载场景 → 注册动画 clip → NPK挂载 → 作用域加载（角色+城镇瓦片）。
    /// </summary>
    [Event(SceneType.LockStep)]
    [FriendOf(typeof(LSAnimResComponent))]
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

            // 作用域加载：角色常驻 + 城镇动画
            LSAnimResComponent animRes = room.GetComponent<LSAnimResComponent>();
            if (animRes == null)
                animRes = room.AddComponent<LSAnimResComponent>();

            // 加法混合材质
            if (animRes.AdditiveMaterial == null)
            {
                Shader additiveShader = Shader.Find("ET/SpriteAdditive");
                if (additiveShader != null)
                    animRes.AdditiveMaterial = new Material(additiveShader);
            }

            ResourceScopeComponent scope = room.GetComponent<ResourceScopeComponent>();
            if (scope == null)
                scope = room.AddComponent<ResourceScopeComponent>();

            // 先卸载副本作用域（从副本回城镇的情况）
            scope.UnloadScope("anim", "dungeon");

            // 城镇加载角色常驻 + 全量动画（暂不分城镇/副本）
            await scope.LoadScope("town", "default", animRes);

            // 瓦片地面 + 客户端权威碰撞
            await room.AddComponent<TownMapViewComponent>().InitAsync();
        }
    }
}
