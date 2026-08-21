using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// .als 特效叠加视图系统：AnimId 变化 → 差分重建叠加组；startFrame 到达 → 激活对应层；
    /// 各层独立渲染时间自推帧（同弹视图模式，播完停末帧）；父动画切走 → 整组销毁。
    /// </summary>
    [EntitySystemOf(typeof(LSAnimOverlayViewComponent))]
    [LSEntitySystemOf(typeof(LSAnimOverlayViewComponent))]
    [FriendOf(typeof(LSAnimOverlayViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSAnimComponent))]
    [FriendOf(typeof(LSAnimResComponent))]
    public static partial class LSAnimOverlayViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimOverlayViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSAnimOverlayViewComponent self)
        {
            ClearOverlays(self);
        }

        [LSEntitySystem]
        private static void LSRollback(this LSAnimOverlayViewComponent self)
        {
            // 回滚后父动画/帧号可能重置：清缓存强制下一帧重建（重建会重置各层帧计时）
            self.LastParentAnimId = -1;
            ClearOverlays(self);
        }

        [EntitySystem]
        private static void Update(this LSAnimOverlayViewComponent self)
        {
            LSUnitView view = self.GetParent<LSUnitView>();
            LSUnit unit = view.Unit;   // EntityRef<LSUnit> 隐式转 LSUnit（struct 不能直接 ?.）
            LSAnimComponent anim = unit?.GetComponent<LSAnimComponent>();
            if (anim == null)
            {
                if (self.Overlays.Count > 0)
                {
                    ClearOverlays(self);
                    self.LastParentAnimId = -1;
                }
                return;
            }

            // 1) 父动画变化 → 重建叠加组（绝大多数动画无 overlay 配置，这里只是一次字典查询）
            if (anim.AnimId != self.LastParentAnimId)
            {
                ClearOverlays(self);
                self.LastParentAnimId = anim.AnimId;
                CreateOverlays(self, view, anim.AnimId);
            }

            if (self.Overlays.Count == 0) return;

            // 2) 推进：startFrame 门控（读逻辑层父动画帧号）+ 渲染时间自推帧
            LSAnimResComponent res = self.Room()?.GetComponent<LSAnimResComponent>();
            foreach (OverlayViewInfo info in self.Overlays)
            {
                bool visible = info.Config.startFrame < 0 || anim.FrameIndex >= info.Config.startFrame;
                if (info.Go.activeSelf != visible) info.Go.SetActive(visible);
                if (!visible) continue;
                AdvanceFrame(info, res, Time.deltaTime);
            }
        }

        private static void CreateOverlays(LSAnimOverlayViewComponent self, LSUnitView view, int parentAnimId)
        {
            AnimOverlayConfig cfg = AnimConfigRegistry.GetOverlay(parentAnimId);
            if (cfg?.overlays == null) return;

            foreach (AnimOverlayEntry entry in cfg.overlays)
            {
                if (entry.effectAnimId == AnimId.None) continue;   // 别名未映射（如空占位动画）
                GameObject go = new GameObject($"Overlay_{entry.effectAni}");
                go.transform.SetParent(view.GameObject.transform, false);   // 挂单位根下：跟位置 + 朝向镜像
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                // DNF 层号直译为 sortingOrder：负 = 身后（身体层动态值 ≈ -(z*100)，通常 ~0）；
                // 10001+ = 前景标记段（身前，高于一切动态深度值）。新增 GO 不受 prefab 21 层限制。
                renderer.sortingOrder = entry.z;

                self.Overlays.Add(new OverlayViewInfo
                {
                    Go = go,
                    Renderer = renderer,
                    OriginalMaterial = renderer.sharedMaterial,
                    Config = entry,
                    FrameIndex = 0,
                    Timer = 0,
                });
                go.SetActive(false);   // 等 startFrame 到达
            }
            if (self.Overlays.Count > 0)
                Log.Info($"[LSAnimOverlay] anim{parentAnimId} 建 {self.Overlays.Count} 层叠加特效");
        }

        private static void ClearOverlays(LSAnimOverlayViewComponent self)
        {
            foreach (OverlayViewInfo info in self.Overlays)
            {
                if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            }
            self.Overlays.Clear();
        }

        /// <summary>渲染时间自推帧（同弹视图：播完停末帧；整组销毁由父动画切换驱动）</summary>
        private static void AdvanceFrame(OverlayViewInfo info, LSAnimResComponent res, float dt)
        {
            AnimClipData clip = AnimConfigRegistry.Get(info.Config.effectAnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;

            info.Timer += dt;
            while (info.FrameIndex < clip.frames.Length - 1)
            {
                float delay = clip.frames[info.FrameIndex].delay / 1000f;
                if (delay <= 0) delay = 0.05f;
                if (info.Timer < delay) break;
                info.Timer -= delay;
                info.FrameIndex++;
            }

            AnimFrameData frame = clip.frames[info.FrameIndex];
            info.Renderer.sprite = res?.GetSprite(frame.image.path, frame.image.index);   // 空路径帧 = null（隐形占位）

            // §2.1 绝对摆位（父 GO = 单位根，中间层链 = 0）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            info.Renderer.transform.localPosition = new Vector3(
                (frame.imagePos.x + center.x) / 100f,
                -(frame.imagePos.y + center.y) / 100f,
                0f);

            // LINEARDODGE 加法混合（特效帧数据驱动）
            if (frame.graphicEffect == 1 && res != null && res.AdditiveMaterial != null)
                info.Renderer.sharedMaterial = res.AdditiveMaterial;
            else if (info.OriginalMaterial != null)
                info.Renderer.sharedMaterial = info.OriginalMaterial;
        }
    }
}
