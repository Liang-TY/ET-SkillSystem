using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterUnitCreate_CreateUnitView: AEvent<EventType.AfterUnitCreate>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterUnitCreate args)
        {
            //try
            //{
            //    ResourcesComponent.Instance.LoadBundle("unit.unit3d");
            //}
            //catch (System.Exception)
            //{
            //    Log.Console($"无法加载unit.unit3d");
            //    throw;
            //}

            ResourcesComponent.Instance.LoadBundle(args.Unit.Config.PrefabName + ".unity3d");
            try
            {
                GameObject unitGame0bject = (GameObject)ResourcesComponent.Instance.GetAsset("Unit.unity3d", "Unit");
                GameObject go = UnityEngine.Object.Instantiate(unitGame0bject, GlobalComponent.Instance.Unit, true);
                GameObject gameGameObject =
                (GameObject)ResourcesComponent.Instance.GetAsset(args.Unit.Config.PrefabName + ".unity3d", args.Unit.Config.PrefabName);
                UnityEngine.Object.Instantiate(gameGameObject, go.transform, true);
                args.Unit.AddComponent<GameObjectComponent>().GameObject = go;
                if (args.Unit.Type != UnitType.Bullet)
                {
                    args.Unit.AddComponent<AnimatorComponent>();
                }

                args.Unit.Position = args.Unit.Position;
            }
            catch (System.Exception)
            {
                Log.Console($"无法加载Unit.unity3d");
                throw;
            }


            await ETTask.CompletedTask;
        }
    }
}