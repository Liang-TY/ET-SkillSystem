using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视图层动画：读逻辑层 LSAnimComponent 的 AnimId/FrameIndex，diff 到变化就换 sprite。
    /// 分层渲染：遍历 RenderConfig.Layers，同帧号从各自图集取 sprite（DNF 换装同构）。
    /// </summary>
    [EntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [LSEntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSAnimComponent))]
    [FriendOf(typeof(LSCombatComponent))]
    [FriendOf(typeof(LSAnimResComponent))]
    public static partial class LSSpriteAnimViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSSpriteAnimViewComponent self)
        {
            self.SpriteRenderer = self.GetParent<LSUnitView>().SpriteRenderer;
            self.OriginalMaterial = self.SpriteRenderer != null ? self.SpriteRenderer.sharedMaterial : null;
        }

        [EntitySystem]
        private static void Update(this LSSpriteAnimViewComponent self)
        {
            LSUnitView view = self.GetParent<LSUnitView>();
            LSUnit unit = view.Unit;
            LSAnimComponent anim = unit?.GetComponent<LSAnimComponent>();
            if (anim == null) return;
            if (view.RenderConfig == null) return;

            // ---- 受击闪白（效果执行；触发由 LSCastViewComponentSystem 的 HP diff 检测）----
            // 闪白优先；否则应用当前帧的 RGBA 染色（0 = 无染色回白）
            AnimFrameData curFrame = anim.GetCurrentFrame();
            if (self.FlashTimer > 0)
            {
                self.FlashTimer -= Time.deltaTime;
                foreach (RenderLayer layer in view.RenderConfig.Layers)
                    if (layer.Renderer != null) layer.Renderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            }
            else
            {
                Color tint = curFrame.rgba != 0
                    ? new Color32((byte)((curFrame.rgba >> 16) & 0xFF), (byte)((curFrame.rgba >> 8) & 0xFF),
                        (byte)(curFrame.rgba & 0xFF), (byte)((curFrame.rgba >> 24) & 0xFF))
                    : Color.white;
                foreach (RenderLayer layer in view.RenderConfig.Layers)
                    if (layer.Renderer != null) layer.Renderer.color = tint;
            }

#if UNITY_EDITOR
            LSUnitViewDebug dbg = view.GameObject.GetComponentInChildren<LSUnitViewDebug>();
            if (dbg != null)
            {
                dbg.AnimId = anim.AnimId;
                dbg.FrameIndex = anim.FrameIndex;
                dbg.FrameTick = (float)anim.FrameTick;
                dbg.Speed = (float)anim.Speed;
                dbg.IsLoop = anim.IsLoop;
                dbg.IsFinished = anim.IsFinished;
                dbg.FaceRight = view.FaceRight;
                RenderLayer firstLayer = view.RenderConfig.Layers.Count > 0 ? view.RenderConfig.Layers[0] : null;
                dbg.SpriteName = firstLayer?.Renderer != null && firstLayer.Renderer.sprite ? firstLayer.Renderer.sprite.name : "null";
            }
#endif

            // 朝向翻转在 LSUnitViewSystem（父组件先跑，翻根 GO 或单层 renderer）——此处不重复处理
            // 只有帧真的变了才碰渲染器
            if (anim.AnimId == self.LastAnimId && anim.FrameIndex == self.LastFrameIndex) return;

            AnimFrameData frame = anim.GetCurrentFrame();
            if (frame.image.path == null || frame.image.path.Length == 0) return;
            LSAnimResComponent res = self.Room()?.GetComponent<LSAnimResComponent>();

            // ---- 分层渲染：遍历 RenderConfig.Layers，同帧号从各自图集取 sprite ----
            foreach (RenderLayer layer in view.RenderConfig.Layers)
            {
                if (layer.Renderer == null) continue;
                Sprite sprite = res?.GetSprite(layer.AtlasName, frame.image.index);
                if (sprite == null) continue;
                layer.Renderer.sprite = sprite;

                // §2.1 绝对摆位（chain 用 localPosition——不受根 GO 翻转影响，世界坐标差值会随 scale 符号变）
                Vector2 center = res?.GetFrameCenter(layer.AtlasName, frame.image.index) ?? Vector2.zero;
                Transform parentT = layer.Renderer.transform.parent;
                Vector3 chain = parentT != null && parentT != view.GameObject.transform
                    ? parentT.localPosition : Vector3.zero;
                // 像素对齐（2026-08-27）：帧偏移 (imagePos+center) 里 center 奇宽带 .5px，随换帧翻转 → snap 整像素（成因②）
                float offX = Mathf.Round(frame.imagePos.x + center.x) / 100f;
                float offY = Mathf.Round(frame.imagePos.y + center.y) / 100f;
                layer.Renderer.transform.localPosition = new Vector3(offX - chain.x, -offY - chain.y, 0f);

                // LINEARDODGE 加法混合
                if (frame.graphicEffect == 1 && res != null && res.AdditiveMaterial != null)
                    layer.Renderer.sharedMaterial = res.AdditiveMaterial;
                else if (layer.OriginalMaterial != null)
                    layer.Renderer.sharedMaterial = layer.OriginalMaterial;
            }

            self.LastAnimId = anim.AnimId;
            self.LastFrameIndex = anim.FrameIndex;
        }

        [LSEntitySystem]
        private static void LSRollback(this LSSpriteAnimViewComponent self)
        {
            self.LastAnimId = -1;
            self.LastFrameIndex = -1;
        }
    }
}
