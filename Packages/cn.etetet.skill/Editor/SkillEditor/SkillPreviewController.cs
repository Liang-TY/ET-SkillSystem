using UnityEditor;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// P2 Step 3 真实预览控制器：PreviewRenderUtility 隐藏场景 + 本体/overlay 双层
    /// SpriteRenderer，按 SkillPreviewState（TimeMs/FacingLeft）确定性采样渲染到 RenderTexture。
    /// 不创建 LSCast/LSWorld，不执行碰撞/伤害（02 §1）；坐标/摆位与运行时同公式（SkillNpkSpriteStore 注释）。
    /// </summary>
    internal sealed class SkillPreviewController
    {
        private const int TextureWidth = 960;
        private const int TextureHeight = 540;
        private const float PixelsPerUnit = 100f;   // 与 Sprite.Create 一致

        private PreviewRenderUtility utility;
        private Transform root;
        private SpriteRenderer bodyRenderer;
        private SpriteRenderer overlayRenderer;

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

            // Unity 6000：PreviewRenderUtility 无 InstantiateGameObjectInScene，用 AddSingleGO 把
            // 已有 GameObject 挂进预览场景（子物体 SetParent 自动随 root 入场景）。
            GameObject rootGo = new("SkillPreviewRoot");
            utility.AddSingleGO(rootGo);
            root = rootGo.transform;
            GameObject bodyGo = new("Body");
            bodyGo.transform.SetParent(root, false);
            bodyRenderer = bodyGo.AddComponent<SpriteRenderer>();
            GameObject overlayGo = new("Overlay");
            overlayGo.transform.SetParent(root, false);
            overlayRenderer = overlayGo.AddComponent<SpriteRenderer>();
            overlayRenderer.sortingOrder = 1;   // overlay 在本体前（与运行时层序一致）
        }

        /// <summary>
        /// 渲染一帧。clip/overlay 由调用方经 SkillAnimCatalog 取；timeMs 按帧 delay 采样。
        /// </summary>
        public bool Render(
            AnimClipData clip,
            AnimOverlayConfig overlay,
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

            // 按 delay 采样当前帧（与运行时 SpriteAnim 推帧一致；delay<=0 用 50ms，01 §8）
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
            root.localPosition = Vector3.zero;

            // 本体层
            DrawLayer(bodyRenderer, frameData, out error);

            // overlay：按配置帧窗口叠加特效（首版一次一层，startFrame -1 全帧生效）
            if (overlay?.overlays != null && overlayRenderer != null)
            {
                foreach (AnimOverlayEntry entry in overlay.overlays)
                {
                    if (entry.effectAnimId == AnimId.None) continue;
                    if (entry.startFrame >= 0 && frame < entry.startFrame) continue;
                    AnimClipData effectClip = SkillAnimCatalog.GetClip(entry.effectAnimId, out string clipError);
                    if (effectClip?.frames != null && effectClip.frames.Length > 0)
                    {
                        int effectFrame = Mathf.Clamp(frame, 0, effectClip.frames.Length - 1);
                        DrawLayer(overlayRenderer, effectClip.frames[effectFrame], out error);
                        break;
                    }
                    error = clipError;
                }
            }

            utility.camera.Render();
            return true;
        }

        private void DrawLayer(SpriteRenderer renderer, AnimFrameData frame, out string error)
        {
            error = null;
            if (frame.image.path == null || frame.image.path.Length == 0)
            {
                renderer.enabled = false;
                return;
            }
            if (!SkillNpkSpriteStore.TryGetEntry(frame.image.path, frame.image.index, out Sprite sprite,
                    out Vector2 center, out error))
            {
                renderer.enabled = false;
                return;
            }

            renderer.enabled = true;
            renderer.sprite = sprite;
            // 运行时同款摆位：round(imagePos + center) / 100，y 取负（§2.1 + 奇数宽 snap）
            float offX = Mathf.Round(frame.imagePos.x + center.x) / PixelsPerUnit;
            float offY = Mathf.Round(frame.imagePos.y + center.y) / PixelsPerUnit;
            renderer.transform.localPosition = new Vector3(offX, -offY, 0f);
            renderer.transform.localScale = Vector3.one;   // 镜像在根上（与运行时朝向翻转同构）
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
            bodyRenderer = null;
            overlayRenderer = null;
        }
    }
}
