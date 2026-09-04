using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// P2 Step 3 真实预览控制器：PreviewRenderUtility 隐藏场景 + 玩家分层渲染
    /// （身体 + 武器×2，对应运行时 UnitRenderConfig 三层）+ overlay 层，
    /// 按 SkillPreviewState（TimeMs/FacingLeft）确定性采样渲染到 RenderTexture。
    /// 不创建 LSCast/LSWorld，不执行碰撞/伤害（02 §1）；坐标/摆位与运行时同公式
    /// （LSSpriteAnimViewComponentSystem §2.1：每层独立 center + 共享 frame.imagePos）。
    /// </summary>
    internal sealed class SkillPreviewController
    {
        private const int TextureWidth = 960;
        private const int TextureHeight = 540;
        private const float PixelsPerUnit = 100f;   // 与 Sprite.Create 一致

        /// <summary>玩家分层渲染配置（与运行时 LSUnitViewComponentSystem 玩家三层一致，单一来源待 P4 抽数）。</summary>
        private static readonly (string atlas, int sortingOrder)[] PlayerLayers =
        {
            ("sprite/character/swordman/equipment/avatar/skin/sm_body0000.img", 10),
            ("sprite/character/swordman/equipment/weapon/katana/katana9200b.img", 16),
            ("sprite/character/swordman/equipment/weapon/katana/katana9200c.img", 17),
        };

        private PreviewRenderUtility utility;
        private Transform root;
        private readonly List<SpriteRenderer> unitRenderers = new();   // 身体+武器（按 sortingOrder 排）
        private SpriteRenderer overlayRenderer;
        private readonly List<SpriteRenderer> areaRenderers = new();   // SpawnEvent 预览实例（本轮建池）

        /// <summary>上一帧渲染诊断（供窗口调试条）。</summary>
        public string LastDebugInfo { get; private set; }

        public RenderTexture Texture { get; private set; }

        public void EnsureCreated()
        {
            if (utility != null) return;

            utility = new PreviewRenderUtility();
            Texture = new RenderTexture(TextureWidth, TextureHeight, 24);
            utility.camera.targetTexture = Texture;
            utility.camera.transform.position = new Vector3(0, 0, -10f);
            utility.camera.orthographic = true;
            utility.camera.orthographicSize = TextureHeight / (2f * PixelsPerUnit);
            utility.camera.clearFlags = CameraClearFlags.SolidColor;
            utility.camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);

            GameObject rootGo = new("SkillPreviewRoot");
            utility.AddSingleGO(rootGo);
            root = rootGo.transform;

            // 单位层（身体+武器）：sortingOrder 与运行时一致
            foreach ((string atlas, int sortingOrder) in PlayerLayers)
            {
                GameObject go = new($"Unit_{sortingOrder}");
                go.transform.SetParent(root, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = sortingOrder;
                unitRenderers.Add(renderer);
            }

            // overlay（特效挂层）
            GameObject overlayGo = new("Overlay");
            overlayGo.transform.SetParent(root, false);
            overlayRenderer = overlayGo.AddComponent<SpriteRenderer>();
            overlayRenderer.sortingOrder = 20;

            // Area 预览实例池（血爆等 createArea 特效视图；主层 + 背层，对应 LSAreaView）。
            // 一个 GO 只允许一个 Renderer → 每个实例拆两个子 GO 各挂 SpriteRenderer。
            for (int i = 0; i < 4; i++)
            {
                GameObject areaGo = new($"Area_{i}");
                areaGo.transform.SetParent(root, false);
                GameObject mainGo = new("AreaMain");
                mainGo.transform.SetParent(areaGo.transform, false);
                SpriteRenderer main = mainGo.AddComponent<SpriteRenderer>();
                main.sortingOrder = 5;    // 运行时区域主层=5
                GameObject backGo = new("AreaBack");
                backGo.transform.SetParent(areaGo.transform, false);
                SpriteRenderer back = backGo.AddComponent<SpriteRenderer>();
                back.sortingOrder = 4;    // 背层=4
                areaRenderers.Add(main);
                areaRenderers.Add(back);
            }
        }

        /// <summary>
        /// 渲染一帧：clip/overlay/area 视图由调用方取好传入。
        /// areaViews 为空 = 本帧无活动区域特效（血爆类技能靠它出图）。
        /// </summary>
        public bool Render(
            AnimClipData clip,
            AnimOverlayConfig overlay,
            IReadOnlyList<AreaViewSample> areaViews,
            int timeMs,
            bool facingLeft,
            out int frameIndex,
            out string error)
        {
            frameIndex = 0;
            error = null;
            if (clip?.frames == null || clip.frames.Length == 0)
            {
                error = "clip 为空";
                return false;
            }
            EnsureCreated();

            // 按 delay 采样当前帧（与运行时 SpriteAnim 推帧一致；delay<=0 用 50ms）
            int frame = 0;
            int elapsed = 0;
            for (int i = 0; i < clip.frames.Length; i++)
            {
                int delay = clip.frames[i].delay > 0 ? clip.frames[i].delay : 50;
                if (timeMs < elapsed + delay)
                {
                    frame = i;
                    break;
                }
                elapsed += delay;
                frame = i;
            }
            frameIndex = frame;

            AnimFrameData frameData = clip.frames[frame];
            root.localScale = facingLeft ? new Vector3(-1f, 1f, 1f) : Vector3.one;

            // ---- 单位分层渲染（运行时 LSSpriteAnimViewComponentSystem 同构）----
            var debugParts = new List<string>
            {
                $"帧{frame + 1}/{clip.frames.Length}",
            };
            for (int i = 0; i < unitRenderers.Count; i++)
            {
                (string atlas, int _) = PlayerLayers[i];
                DrawUnitLayer(unitRenderers[i], atlas, frameData, facingLeft, debugParts, i == 0);
            }

            // ---- overlay（特效挂层，同帧号采样）----
            DrawOverlay(overlay, frame, debugParts);

            // ---- Area 预览实例（spawnEvent 的 createArea 视图）----
            DrawAreaViews(areaViews, debugParts);

            utility.camera.Render();
            LastDebugInfo = string.Join("  |  ", debugParts);
            return true;
        }

        private void DrawUnitLayer(
            SpriteRenderer renderer,
            string atlasPath,
            AnimFrameData frame,
            bool facingLeft,
            List<string> debugParts,
            bool isBody)
        {
            // 全层取 frame.image.index（换装管线：每层图集同帧号一一对应）
            if (!SkillNpkSpriteStore.TryGetEntry(atlasPath, frame.image.index, out Sprite sprite,
                    out Vector2 center, out string spriteError))
            {
                renderer.enabled = false;
                if (isBody) debugParts.Add($"本体缺失: {spriteError}");
                return;
            }
            renderer.enabled = true;
            renderer.sprite = sprite;
            // 每层独立 center + 共享 frame.imagePos（运行时 §2.1）；y 取负；奇数宽 .5px snap
            float offX = Mathf.Round(frame.imagePos.x + center.x) / PixelsPerUnit;
            float offY = Mathf.Round(frame.imagePos.y + center.y) / PixelsPerUnit;
            renderer.transform.localPosition = new Vector3(offX, -offY, 0f);
            renderer.transform.localScale = Vector3.one;   // 镜像在根上

            if (isBody)
                debugParts.Add($"{System.IO.Path.GetFileName(atlasPath)}#{frame.image.index}");
        }

        private void DrawOverlay(AnimOverlayConfig overlay, int frame, List<string> debugParts)
        {
            if (overlay?.overlays == null || overlayRenderer == null)
            {
                if (overlayRenderer != null) overlayRenderer.enabled = false;
                return;
            }
            foreach (AnimOverlayEntry entry in overlay.overlays)
            {
                if (entry.effectAnimId == AnimId.None) continue;
                if (entry.startFrame >= 0 && frame < entry.startFrame) continue;
                AnimClipData effectClip = SkillAnimCatalog.GetClip(entry.effectAnimId, out string clipError);
                if (effectClip?.frames == null || effectClip.frames.Length == 0)
                {
                    debugParts.Add($"overlay 缺失 animId={entry.effectAnimId}: {clipError}");
                    continue;
                }
                int effectFrame = Mathf.Clamp(frame, 0, effectClip.frames.Length - 1);
                AnimFrameData effectFrameData = effectClip.frames[effectFrame];
                if (SkillNpkSpriteStore.TryGetEntry(effectFrameData.image.path, effectFrameData.image.index,
                        out Sprite sprite, out Vector2 center, out string spriteError))
                {
                    overlayRenderer.enabled = true;
                    overlayRenderer.sprite = sprite;
                    float offX = Mathf.Round(effectFrameData.imagePos.x + center.x) / PixelsPerUnit;
                    float offY = Mathf.Round(effectFrameData.imagePos.y + center.y) / PixelsPerUnit;
                    overlayRenderer.transform.localPosition = new Vector3(offX, -offY, 0f);
                    debugParts.Add($"刀光 {System.IO.Path.GetFileName(effectFrameData.image.path)}#{effectFrameData.image.index}");
                    return;   // 首版一次一层
                }
                debugParts.Add($"overlay 贴图缺失: {spriteError}");
                return;
            }
            overlayRenderer.enabled = false;
        }

        private void DrawAreaViews(IReadOnlyList<AreaViewSample> areaViews, List<string> debugParts)
        {
            int used = 0;
            if (areaViews != null)
            {
                foreach (AreaViewSample view in areaViews)
                {
                    if (view?.Clip == null || used + 2 > areaRenderers.Count) continue;
                    // 区域动画从创建时刻起独立推帧（运行时 LSAreaViewComponentSystem 同构）
                    int frame = SampleFrame(view.Clip, view.ElapsedMs);
                    SpriteRenderer main = areaRenderers[used++];
                    SpriteRenderer back = areaRenderers[used++];
                    DrawSimpleRenderer(main, view.Clip.frames[frame]);
                    if (view.BackClip != null)
                    {
                        int backFrame = SampleFrame(view.BackClip, view.ElapsedMs);
                        DrawSimpleRenderer(back, view.BackClip.frames[backFrame]);
                    }
                    else
                    {
                        back.enabled = false;
                    }
                    debugParts.Add($"Area[{view.AreaId}] {view.Clip.frames.Length}帧 @{view.ElapsedMs}ms");
                }
            }
            for (int i = used; i < areaRenderers.Count; i++)
                areaRenderers[i].enabled = false;
        }

        private void DrawSimpleRenderer(SpriteRenderer renderer, AnimFrameData frameData)
        {
            if (frameData.image.path == null || frameData.image.path.Length == 0
                || !SkillNpkSpriteStore.TryGetEntry(frameData.image.path, frameData.image.index,
                    out Sprite sprite, out Vector2 center, out _))
            {
                renderer.enabled = false;
                return;
            }
            renderer.enabled = true;
            renderer.sprite = sprite;
            float offX = Mathf.Round(frameData.imagePos.x + center.x) / PixelsPerUnit;
            float offY = Mathf.Round(frameData.imagePos.y + center.y) / PixelsPerUnit;
            renderer.transform.localPosition = new Vector3(offX, -offY, 0f);
        }

        private static int SampleFrame(AnimClipData clip, int timeMs)
        {
            int elapsed = 0;
            for (int i = 0; i < clip.frames.Length; i++)
            {
                int delay = clip.frames[i].delay > 0 ? clip.frames[i].delay : 50;
                if (timeMs < elapsed + delay) return i;
                elapsed += delay;
            }
            return clip.frames.Length - 1;
        }

        /// <summary>Area 预览实例采样参数（窗口按 spawnEvent.atMs 计算 ElapsedMs 传入）。</summary>
        internal sealed class AreaViewSample
        {
            public int AreaId;
            public AnimClipData Clip;
            public AnimClipData BackClip;
            public int ElapsedMs;
        }

        public void Dispose()
        {
            if (utility != null)
            {
                utility.Cleanup();   // Unity 6000 PreviewRenderUtility 无 Dispose()，Cleanup 即释放
                utility = null;
            }
            if (Texture != null)
            {
                Texture.Release();
                Texture = null;
            }
            root = null;
            unitRenderers.Clear();
            overlayRenderer = null;
            areaRenderers.Clear();
        }
    }
}
