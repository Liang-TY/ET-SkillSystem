using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    public static partial class LSUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this LSUnitViewComponent self)
        {

        }

        public static async ETTask InitAsync(this LSUnitViewComponent self)
        {
            Room room = self.Room();

            // Initialize 2D animation resources
            LSAnimResComponent animRes = room.AddComponent<LSAnimResComponent>();
            await animRes.InitAsync();

            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            Scene root = self.Root();
            GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();

            // Half B: 遍历所有 unit（玩家 + 怪物），不只 PlayerIds
            GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
            foreach (var kv in lsUnitComponent.Children)
            {
                LSUnit lsUnit = (LSUnit)kv.Value;

                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.SpriteRenderer = unitGo.GetComponentInChildren<SpriteRenderer>();   // 修 bug #2
                lsUnitView.AddComponent<LSSpriteAnimViewComponent>();   // Half B: 自写换帧组件（Mecanim LSAnimatorComponent 已删）
            }
        }
    }
}