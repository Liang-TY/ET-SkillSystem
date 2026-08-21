using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// .als 特效叠加的共享渲染助手（单位视图 LSAnimOverlayViewComponent 与区域视图
    /// LSAreaViewComponent 共用）：查父动画的 AnimOverlayConfig → 动态建子 GO+SpriteRenderer
    /// → 各自渲染时间自推帧（startFrame 门控、播完停末帧、LINEARDODGE/RGBA 帧级效果）→ 销毁。
    /// 子 GO 挂 parent 变换下：自动跟随位置（单位根还有朝向镜像）。
    /// </summary>
    public static class LSAnimOverlayUtil
    {
        /// <summary>
        /// 为父动画建叠加层组（无配置返回空列表）。
        /// baseSortingOrder：sortingOrder = base + z（DNF 层号直译：负 = 主层身后，正 = 身前）。
        /// 单位传 0（身体层动态值 ≈ 0）；区域传主层 sortingOrder + 1（如 5+1=6，子层绕主层排序）。
        /// </summary>
        public static List<OverlayViewInfo> CreateOverlays(Transform parent, int parentAnimId, int baseSortingOrder)
        {
            List<OverlayViewInfo> overlays = new();
            AnimOverlayConfig cfg = AnimConfigRegistry.GetOverlay(parentAnimId);
            if (cfg?.overlays == null) return overlays;

            foreach (AnimOverlayEntry entry in cfg.overlays)
            {
                if (entry.effectAnimId == AnimId.None) continue;   // 别名未映射（如空占位动画）
                GameObject go = new GameObject($"Overlay_{entry.effectAni}");
                go.transform.SetParent(parent, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = baseSortingOrder + entry.z;

                overlays.Add(new OverlayViewInfo
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
            if (overlays.Count > 0)
                Log.Info($"[LSAnimOverlay] anim{parentAnimId} 建 {overlays.Count} 层叠加特效");
            return overlays;
        }

        /// <summary>销毁整组叠加层</summary>
        public static void DestroyOverlays(List<OverlayViewInfo> overlays)
        {
            if (overlays == null) return;
            foreach (OverlayViewInfo info in overlays)
            {
                if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            }
            overlays.Clear();
        }

        /// <summary>
        /// 推进整组：startFrame 门控（parentFrameIndex = 父动画当前帧）+ 渲染时间自推帧
        /// （同弹视图模式，播完停末帧；整组销毁由父视图生命周期驱动）。
        /// </summary>
        public static void AdvanceOverlays(List<OverlayViewInfo> overlays, int parentFrameIndex,
            LSAnimResComponent res, float dt)
        {
            if (overlays == null) return;
            foreach (OverlayViewInfo info in overlays)
            {
                bool visible = info.Config.startFrame < 0 || parentFrameIndex >= info.Config.startFrame;
                if (info.Go.activeSelf != visible) info.Go.SetActive(visible);
                if (!visible) continue;
                AdvanceFrame(info, res, dt);
            }
        }

        /// <summary>
        /// 推进"脱离父动画"的叠加层（DNF [none effect add] 语义：特效不随父动画结束而截断，
        /// 原地播完为止）：无门控推进，播完的当场销毁并移出列表。倒序遍历安全删除。
        /// </summary>
        public static void AdvanceDetaching(List<OverlayViewInfo> overlays, LSAnimResComponent res, float dt)
        {
            if (overlays == null || overlays.Count == 0) return;
            for (int i = overlays.Count - 1; i >= 0; i--)
            {
                OverlayViewInfo info = overlays[i];
                AdvanceFrame(info, res, dt);
                AnimClipData clip = AnimConfigRegistry.Get(info.Config.effectAnimId);
                if (clip?.frames == null || clip.frames.Length == 0) continue;
                float lastDelay = clip.frames[clip.frames.Length - 1].delay / 1000f;
                if (lastDelay <= 0) lastDelay = 0.05f;
                if (info.FrameIndex >= clip.frames.Length - 1 && info.Timer >= lastDelay)
                {
                    if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
                    overlays.RemoveAt(i);
                }
            }
        }

        /// <summary>渲染时间自推帧（同弹视图：播完停末帧）</summary>
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

            // §2.1 绝对摆位（父 GO = 挂点根，中间层链 = 0）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            info.Renderer.transform.localPosition = new Vector3(
                (frame.imagePos.x + center.x) / 100f,
                -(frame.imagePos.y + center.y) / 100f,
                0f);

            // 帧级效果：LINEARDODGE 加法混合 + RGBA 染色/透明度
            ApplyFrameEffects(info.Renderer, info.OriginalMaterial, frame, res);
        }

        /// <summary>帧级渲染效果（三个视图共用）：加法混合材质切换 + RGBA 染色（0 = 无染色回白）</summary>
        public static void ApplyFrameEffects(SpriteRenderer renderer, Material originalMaterial,
            AnimFrameData frame, LSAnimResComponent res)
        {
            if (frame.graphicEffect == 1 && res != null && res.AdditiveMaterial != null)
                renderer.sharedMaterial = res.AdditiveMaterial;
            else if (originalMaterial != null)
                renderer.sharedMaterial = originalMaterial;

            renderer.color = frame.rgba != 0
                ? new Color32((byte)((frame.rgba >> 16) & 0xFF), (byte)((frame.rgba >> 8) & 0xFF),
                    (byte)(frame.rgba & 0xFF), (byte)((frame.rgba >> 24) & 0xFF))
                : Color.white;
        }
    }
}
