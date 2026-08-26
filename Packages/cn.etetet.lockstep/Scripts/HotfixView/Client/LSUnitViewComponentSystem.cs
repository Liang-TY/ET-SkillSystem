using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSAnimResComponent))]
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

            // 2D 动画资源（作用域驱动：NPK 挂载 + 依赖收集 + 按场景加载）
            LSAnimResComponent animRes = room.GetComponent<LSAnimResComponent>();
            if (animRes == null)
            {
                animRes = room.AddComponent<LSAnimResComponent>();
            }

            // NPK 挂载（首次才挂载）
            NpkLoaderComponent npkLoader = room.GetComponent<NpkLoaderComponent>();
            if (npkLoader == null)
            {
                npkLoader = room.AddComponent<NpkLoaderComponent>();
                await npkLoader.LoadAllNpks();
            }

            // 加法混合材质
            if (animRes.AdditiveMaterial == null)
            {
                Shader additiveShader = Shader.Find("ET/SpriteAdditive");
                if (additiveShader != null)
                {
                    animRes.AdditiveMaterial = new Material(additiveShader);
                }
            }

            // 作用域加载：副本场景的全部动画 IMG
            ResourceScopeComponent scope = room.GetComponent<ResourceScopeComponent>();
            if (scope == null)
            {
                scope = room.AddComponent<ResourceScopeComponent>();
            }

            // 先卸载城镇作用域（如果存在——从城镇直接进战斗的情况）
            scope.UnloadScope("anim", "town");

            await scope.LoadScope("dungeon", room.MapId.ToString(), animRes);

            self.UnitPrefab = await room.GetComponent<ResourcesLoaderComponent>()
                .LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");

            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in lsUnitComponent.Children)
            {
                CreateUnitView(self, (LSUnit)kv.Value);
            }
        }

        /// <summary>
        /// 单位视图差分（子弹视图同款）：怪物死亡 Dispose 后逻辑里没了 → 销毁视图 GO+实体；
        /// 逻辑新增单位（未来召唤物）→ 补建视图。
        /// </summary>
        [EntitySystem]
        private static void Update(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent unitComponent = room.LSWorld?.GetComponent<LSUnitComponent>();
            if (unitComponent == null) return;

            // 差分移除：视图有、逻辑无（死亡/移除的单位）
            List<long> removed = null;
            foreach (var kv in self.Children)
            {
                if (unitComponent.GetChild<LSUnit>(kv.Key) != null) continue;
                removed ??= new List<long>();
                removed.Add(kv.Key);
            }
            if (removed != null)
            {
                foreach (long id in removed)
                {
                    LSUnitView view = self.GetChild<LSUnitView>(id);
                    if (view == null) continue;
                    if (view.GameObject != null) UnityEngine.Object.Destroy(view.GameObject);
                    view.Dispose();
                }
            }

            // 差分新增：逻辑有、视图无（当前只有初始怪物，为召唤物预留）
            if (self.UnitPrefab == null) return;
            foreach (var kv in unitComponent.Children)
            {
                if (self.Children.ContainsKey(kv.Key)) continue;
                CreateUnitView(self, (LSUnit)kv.Value);
            }
        }

        /// <summary>建单个单位视图（InitAsync 与差分新增共用）：prefab 实例化 + 分层渲染配置 + 动画视图组件</summary>
        private static void CreateUnitView(this LSUnitViewComponent self, LSUnit lsUnit)
        {
            Scene root = self.Root();
            GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();

            GameObject unitGo = UnityEngine.Object.Instantiate(self.UnitPrefab, globalComponent.Unit, true);
            // 出生摆位用屏幕映射（z→Y），避免首帧从错误位置 Lerp 飞入
            unitGo.transform.position = new Vector3(
                (float)lsUnit.Position.x, (float)(lsUnit.Position.z + lsUnit.Position.y), 0f);

            LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);

            // 分层渲染：从 prefab 的 21 个子 GO 中按 sortingOrder 匹配 renderer
            // prefab 结构：子 GO "0"~"20"，各带 SpriteRenderer，sortingOrder = GO 名
            // 怪物 = 只配 1 层（skin→bantuamazones），玩家 = 配 3 层（DNF 换装）
            if (lsUnit.GetComponent<LSInputComponent>() != null)
            {
                // 玩家（鬼剑士 + 太刀）
                lsUnitView.RenderConfig = new UnitRenderConfig();
                lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "sm_body0000.img", SortingOrder = 10 });
                lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200b.img", SortingOrder = 16 });
                lsUnitView.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200c.img", SortingOrder = 17 });
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
            lsUnitView.AddComponent<LSAnimOverlayViewComponent>();   // .als 特效叠加（有配置才建层）
        }
    }
}
