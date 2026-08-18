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

            // 注册动画 clip + 加载技能内容 DLL + 弹视图（必须在 room.Init 建 unit 之前；此事件 await PublishAsync 完成后才 Init）
            await LSAnimClipRegistrar.RegisterAll(clientScene);
            await SkillContentLoader.Load(clientScene);
            await room.AddComponent<LSBulletViewComponent>().InitAsync();
        }
    }
}