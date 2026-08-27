using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇本地玩家视图系统（阶段B）：Unit2D prefab + 鬼剑士 3 层渲染（同战斗配置）+
    /// 视图层自推帧（子弹视图同款——城镇无逻辑实体）；摆位/翻转与战斗单位同公式（03 文档 §4.4：屏幕=(x, z+y, 0)）。
    /// </summary>
    [EntitySystemOf(typeof(TownPlayerViewComponent))]
    [FriendOf(typeof(TownPlayerViewComponent))]
    [FriendOf(typeof(TownPlayerComponent))]
    [FriendOf(typeof(TownCollisionComponent))]   // 读 OriginX/OriginZ 钳相机边界（ET0002）
    public static partial class TownPlayerViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TownPlayerViewComponent self)
        {
            self.Camera = Camera.main;
        }

        [EntitySystem]
        private static void Destroy(this TownPlayerViewComponent self)
        {
            if (self.Root != null)
            {
                UnityEngine.Object.Destroy(self.Root);
                self.Root = null;
            }
        }

        public static async ETTask InitAsync(this TownPlayerViewComponent self)
        {
            Room room = self.GetParent<Room>();
            LSAnimResComponent res = room.GetComponent<LSAnimResComponent>();
            if (res == null)
            {
                Log.Warning("[TownPlayerView] LSAnimResComponent 不存在（需在 ChangeStart 阶段先 InitAsync）");
                return;
            }

            GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>()
                .LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
            self.UnitPrefab = prefab;   // 缓存给远端玩家视图复用
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.name = "TownPlayer";

            // 玩家 3 层渲染配置（同战斗 LSUnitViewComponentSystem：鬼剑士+太刀）
            self.RenderConfig = new UnitRenderConfig();
            self.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "sm_body0000.img", SortingOrder = 10 });
            self.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200b.img", SortingOrder = 16 });
            self.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200c.img", SortingOrder = 17 });

            SpriteRenderer[] allRenderers = go.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (RenderLayer layer in self.RenderConfig.Layers)
            {
                foreach (SpriteRenderer r in allRenderers)
                {
                    if (r.sortingOrder != layer.SortingOrder) continue;
                    layer.Renderer = r;
                    layer.OriginalMaterial = r.sharedMaterial;
                    break;
                }
                if (layer.Renderer == null)
                    Log.Warning($"[TownPlayerView] 未找到 sortingOrder={layer.SortingOrder} 的 renderer（{layer.AtlasName}）");
            }

            self.Root = go;
            self.AnimId = AnimId.SwordmanIdle;
            SyncTransform(self);
        }

        [EntitySystem]
        private static void Update(this TownPlayerViewComponent self)
        {
            if (self.Root == null || self.RenderConfig == null) return;
            SyncTransform(self);
            AdvanceFrame(self, Time.deltaTime);
        }

        [EntitySystem]
        private static void LateUpdate(this TownPlayerViewComponent self)
        {
            if (self.Root == null || self.Camera == null) return;

            // 相机跟随（2026-08-28）：城镇长图，相机跟本地玩家；+0.5 让角色居中（脚底上方 0.5 单位）。
            Vector3 pos = self.Root.transform.position;
            Vector3 target = new Vector3(pos.x, pos.y + 0.5f, 0f);

            // 边界钳制：读碰撞组件 OriginX/OriginZ（视觉半宽/半高），clamp 到地图内再 snap 到像素格
            Room room = self.GetParent<Room>();
            TownCollisionComponent collision = room?.GetComponent<TownCollisionComponent>();
            if (collision != null)
            {
                self.Camera.transform.position = CameraClampHelper.ClampToMap(
                    target, (float)collision.OriginX, (float)collision.OriginZ, self.Camera);
            }
            else
            {
                self.Camera.transform.position = new Vector3(
                    Mathf.Round(target.x * 100f) / 100f,
                    Mathf.Round(target.y * 100f) / 100f,
                    self.Camera.transform.position.z);
            }
        }

        /// <summary>位置/朝向同步：屏幕映射 (x, z+y, 0)（同战斗单位）；翻转翻根 GO（所有层一起镜像）</summary>
        private static void SyncTransform(this TownPlayerViewComponent self)
        {
            TownPlayerComponent player = self.GetParent<Room>().GetComponent<TownPlayerComponent>();
            if (player == null) return;

            TSVector pos = player.Position;
            self.Root.transform.position = new Vector3((float)pos.x, (float)(pos.z + pos.y), 0f);

            // 像素对齐（2026-08-27）：运动时坐标是小数 → Point 采样逐帧跳变（沸腾/糊）。
            // snap 到 1/100 单位（100ppu = 1 屏幕像素）；只动视图层，逻辑层（帧同步）绝不动。
            Vector3 p = self.Root.transform.position;
            self.Root.transform.position = new Vector3(Mathf.Round(p.x * 100f) / 100f, Mathf.Round(p.y * 100f) / 100f, p.z);

            bool faceRight = player.Forward.x >= FP.Zero;
            if (faceRight != self.FaceRight)
            {
                self.FaceRight = faceRight;
                self.Root.transform.localScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f);
            }
        }

        /// <summary>
        /// 渲染时间自推帧（循环）：按 clip delay 换帧；换帧时逐层取 sprite + imagePos 绝对摆位
        /// （同战斗 LSSpriteAnimViewComponent 的换帧块，去掉战斗特效染色——城镇只要 Idle/Walk）。
        /// </summary>
        private static void AdvanceFrame(this TownPlayerViewComponent self, float dt)
        {
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;

            // 动画切换（Idle↔Walk）帧数不同——归零防越界（崩溃点：旧 FrameIndex 超新 clip 长度）
            if (self.AnimId != self.LastAnimId)
            {
                self.FrameIndex = 0;
                self.Timer = 0;
            }

            self.Timer += dt;
            bool frameChanged = false;
            while (true)
            {
                float delay = clip.frames[self.FrameIndex].delay / 1000f;
                if (delay <= 0f) delay = 0.05f;   // 防零延迟死循环（子弹视图同款兜底）
                if (self.Timer < delay) break;
                self.Timer -= delay;
                self.FrameIndex = (self.FrameIndex + 1) % clip.frames.Length;   // 城镇动画循环
                frameChanged = true;
            }
            // 动画切换（Idle↔Walk 由 TownOpera 写 AnimId）也要刷一帧
            if (!frameChanged && self.AnimId == self.LastAnimId && self.FrameIndex == self.LastFrameIndex) return;

            AnimFrameData frame = clip.frames[self.FrameIndex];
            if (frame.image.path == null || frame.image.path.Length == 0) return;

            LSAnimResComponent res = self.GetParent<Room>().GetComponent<LSAnimResComponent>();
            foreach (RenderLayer layer in self.RenderConfig.Layers)
            {
                if (layer.Renderer == null) continue;
                Sprite sprite = res?.GetSprite(layer.AtlasName, frame.image.index);
                if (sprite == null) continue;
                layer.Renderer.sprite = sprite;

                // §2.1 绝对摆位（同战斗：local = 内容中心 + imagePos 修正，链式偏移扣除）
                Vector2 center = res?.GetFrameCenter(layer.AtlasName, frame.image.index) ?? Vector2.zero;
                Transform parentT = layer.Renderer.transform.parent;
                Vector3 chain = parentT != null && parentT != self.Root.transform
                    ? parentT.localPosition : Vector3.zero;
                // 像素对齐（2026-08-27）：帧偏移 (imagePos+center) 里 center 奇宽带 .5px，随换帧翻转 → snap 整像素（成因②）
                float offX = Mathf.Round(frame.imagePos.x + center.x) / 100f;
                float offY = Mathf.Round(frame.imagePos.y + center.y) / 100f;
                layer.Renderer.transform.localPosition = new Vector3(offX - chain.x, -offY - chain.y, 0f);
            }

            self.LastAnimId = self.AnimId;
            self.LastFrameIndex = self.FrameIndex;
        }
    }
}
