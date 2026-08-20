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

            GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
            foreach (var kv in lsUnitComponent.Children)
            {
                LSUnit lsUnit = (LSUnit)kv.Value;

                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);

                // 分层渲染：从 prefab 的 21 个子 GO 中按 sortingOrder 匹配 renderer
                // prefab 结构：子 GO "0"~"20"，各带 SpriteRenderer，sortingOrder = GO 名
                // 怪物 = 只配 1 层（skin→bantuamazones），玩家 = 配 3 层（DNF 换装）
                if (lsUnit.GetComponent<LSInputComponent>() != null)
                {
                    // 玩家（鬼剑士 + 太刀）
                    lsUnitView.RenderConfig = new UnitRenderConfig();
                    lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "sm_body0000.img", SortingOrder = 10 });
                    lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana_blade.img", SortingOrder = 16 });
                    lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana_handle.img", SortingOrder = 17 });
                }
                else
                {
                    // 怪物（bantu 单层）
                    lsUnitView.RenderConfig = new UnitRenderConfig();
                    lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "bantuamazones.img", SortingOrder = 10 });
                }

                // 按 sortingOrder 匹配 prefab 里的 SpriteRenderer
                SpriteRenderer[] allRenderers = unitGo.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (RenderLayer layer in lsUnitView.RenderConfig.Layers)
                {
                    foreach (SpriteRenderer r in allRenderers)
                    {
                        if (r.sortingOrder != layer.SortingOrder) continue;
                        layer.Renderer = r;
                        layer.OriginalMaterial = r.sharedMaterial;
                        break;
                    }
                    if (layer.Renderer == null)
                        Log.Warning($"[UnitView] 未找到 sortingOrder={layer.SortingOrder} 的 renderer（{layer.AtlasName}）");
                }

                // 兼容旧代码：单层引用指向第 0 层的 renderer
                lsUnitView.SpriteRenderer = lsUnitView.RenderConfig.Layers.Count > 0
                    ? lsUnitView.RenderConfig.Layers[0].Renderer
                    : unitGo.GetComponentInChildren<SpriteRenderer>();

                lsUnitView.AddComponent<LSSpriteAnimViewComponent>();
            }
        }
    }
}
