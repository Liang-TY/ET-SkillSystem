using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneChangeStart_AddComponent: AEvent<Scene, LSSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneChangeStart args)
        {
            Room room = clientScene.GetComponent<Room>();
            ResourcesLoaderComponent resourcesLoaderComponent = room.AddComponent<ResourcesLoaderComponent>();
            room.AddComponent<UIComponent>();
            
            // 创建loading界面
            
            
            // 创建房间UI
            await UIHelper.Create(args.Room, UIType.UILSRoom, UILayer.Low);
            
            // 加载场景资源
            await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.demores/Scenes/{"Game"}.unity", LoadSceneMode.Single);

            // 注册动画 clip + 加载技能内容 DLL + 弹视图 + 战斗表现钩子 + 地图瓦片（必须在 room.Init 建 unit 之前）
            await LSAnimClipRegistrar.RegisterAll(clientScene);
            await SkillContentLoader.Load(clientScene);
            // 地图瓦片按 Room.MapId 懒加载（SkillContentLoader 之后——MapLoader 那时才注册完）；
            // 碰撞矩阵缓存须在 room.Init 前就绪——PublishAsync 的 await 时序保证
            await room.AddComponent<LSMapViewComponent>().InitAsync();
            await room.AddComponent<LSBulletViewComponent>().InitAsync();
            await room.AddComponent<LSAreaViewComponent>().InitAsync();
            room.AddComponent<LSCastViewComponent>();
        }
    }
}