using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// P2 Step 3 真实预览控制器：PreviewRenderUtility 隐藏场景 + 玩家分层渲染
    /// （身体 + 武器×2，对应运行时 UnitRenderConfig 三层）+ overlay 层 + Spawn 实例
    /// （Area/Bullet 视图），按 SkillPreviewState（TimeMs/FacingLeft）确定性采样渲染。
    /// 不创建 LSCast/LSWorld，不执行碰撞/伤害（02 §1）；坐标/摆位与运行时同公式
    /// （LSSpriteAnimViewComponentSystem §2.1：每层独立 center + 共享 frame.imagePos）。
    /// 本轮新增（诊断轮）：LINEARDODGE 加法混合（graphicEffect==1 → ET/SpriteAdditive，
    /// 与运行时 LSAnimResComponentSystem 同 shader）、Spawn 实例 inFront/Bullet 位移、
    /// 空帧（image.path 空串）正常跳过不报错、结构化诊断 RenderReport。
    /// </summary>
    internal sealed class SkillPreviewController
    {
        private const int TextureWidth = 960;
        private const int TextureHeight = 540;
        private const float PixelsPerUnit = 100f;   // 与 Sprite.Create 一致

        /// <summary>玩家分层渲染配置（与运行时 LSUnitViewComponentSystem 玩家三层一致）。
        /// 首 path 匹配才启用分层；怪物 clip 自适应单层（见 DetermineLayers）。</summary>
        private static readonly (string atlas, int sortingOrder)[] PlayerLayers =
        {
            ("sprite/character/swordman/equipment/avatar/skin/sm_body0000.img", 10),
            ("sprite/character/swordman/equipment/weapon/katana/katana9200b.img", 16),
            ("sprite/character/swordman/equipment/weapon/katana/katana9200c.img", 17),
        };

        private PreviewRenderUtility utility;
        private Transform root;
        private readonly List<SpriteRenderer> unitRenderers = new();
        private SpriteRenderer overlayRenderer;
        private readonly List<SpriteRenderer> spawnRenderers = new();
        private Material additiveMaterial;

        /// <summary>上一帧结构化诊断（窗口诊断面板展示）。</summary>
        public RenderReport Report { get; } = new();

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

            foreach ((string _, int sortingOrder) in PlayerLayers)
            {
                GameObject go = new($"Unit_{sortingOrder}");
                go.transform.SetParent(root, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = sortingOrder;
                unitRenderers.Add(renderer);
            }

            GameObject overlayGo = new("Overlay");
            overlayGo.transform.SetParent(root, false);
            overlayRenderer = overlayGo.AddComponent<SpriteRenderer>();
            overlayRenderer.sortingOrder = 20;

            // Spawn 实例池（Area 主/背 + 弹体；主层=5 背层=4 与 LSAreaView 一致）
            for (int i = 0; i < 4; i++)
            {
                GameObject areaGo = new($"Spawn_{i}");
                areaGo.transform.SetParent(root, false);
                GameObject mainGo = new("Main");
                mainGo.transform.SetParent(areaGo.transform, false);
                SpriteRenderer main = mainGo.AddComponent<SpriteRenderer>();
                main.sortingOrder = 5;
                GameObject backGo = new("Back");
                backGo.transform.SetParent(areaGo.transform, false);
                SpriteRenderer back = backGo.AddComponent<SpriteRenderer>();
                back.sortingOrder = 4;
                spawnRenderers.Add(main);
                spawnRenderers.Add(back);
            }

            Shader additiveShader = Shader.Find("ET/SpriteAdditive");
            if (additiveShader != null) additiveMaterial = new Material(additiveShader);
        }

        /// <summary>
        /// 渲染一帧：clip/overlay/spawn 视图由调用方取好传入；inheritedStartMs 为继承动画的
        /// 实际起播时刻（animId=0 phase），采样时间 = timeMs - inheritedStartMs。
        /// </summary>
        /// <summary>盒体显示开关（窗口 Toggle 驱动）：true 画攻击盒(红)/受击盒(绿)线框。</summary>
        public bool ShowBoxes { get; set; }

        public bool Render(
            AnimClipData clip,
            AnimOverlayConfig overlay,
            IReadOnlyList<SpawnViewSample> spawnViews,
            int timeMs,
            int inheritedStartMs,
            bool facingLeft,
            out int frameIndex,
            out string error)
        {
            frameIndex = 0;
            error = null;
            Report.Reset(timeMs);
            if (clip?.frames == null || clip.frames.Length == 0)
            {
                error = "clip 为空";
                return false;
            }
            EnsureCreated();

            int sampleMs = inheritedStartMs >= 0 ? timeMs - inheritedStartMs : timeMs;
            int frame = SampleFrame(clip, sampleMs);
            frameIndex = frame;

            AnimFrameData frameData = clip.frames[frame];
            root.localScale = facingLeft ? new Vector3(-1f, 1f, 1f) : Vector3.one;

            Report.Animation = new LineInfo
            {
                Text = $"animId 帧 {frame + 1}/{clip.frames.Length}（采样 {sampleMs}ms"
                    + (inheritedStartMs >= 0 ? $"，继承自 {inheritedStartMs}ms" : "") + "）",
            };

            // ---- 单位分层：按首帧 path 自适应（玩家三层 / 怪物单层）----
            bool isPlayerClip = frameData.image.path != null
                && frameData.image.path.EndsWith("sm_body0000.img");
            debugLayerUsed = 0;
            if (isPlayerClip)
            {
                for (int i = 0; i < unitRenderers.Count; i++)
                {
                    (string atlas, int _) = PlayerLayers[i];
                    DrawUnitLayer(unitRenderers[i], atlas, frameData, i == 0);
                }
            }
            else
            {
                // 怪物/单层 clip：直接用帧自带 path（不再套鬼剑士图集）
                for (int i = 0; i < unitRenderers.Count; i++)
                    unitRenderers[i].enabled = false;
                DrawSingleFromFrame(unitRenderers[0], frameData, "本体");
            }

            // ---- overlay（同帧号采样；空帧正常跳过）----
            DrawOverlay(overlay, frame);

            // ---- Spawn 实例（Area/Bullet）----
            DrawSpawnViews(spawnViews, facingLeft);

            // ---- 盒体线框（勾选时；DNF 像素 → /100 → y/z 对调）----
            DrawBoxes(frameData, facingLeft);

            utility.camera.Render();
            return true;
        }

        private readonly List<SpriteRenderer> boxRenderers = new();

        /// <summary>当前帧盒体线框：attackBoxes 红 / damageBoxes 绿。DNF 坐标 x=横向 y=纵深 z=高度，
        /// 与运行时 LSHitboxComponentSystem 采样同构：/100 后 y/z 对调，盒心 x=imagePos.x 修正。</summary>
        private void DrawBoxes(AnimFrameData frame, bool facingLeft)
        {
            EnsureBoxRenderers();
            int used = 0;
            if (ShowBoxes)   // AnimFrameData 为 struct，恒非空
            {
                used = DrawBoxSet(frame.attackBoxes, new Color(1f, 0.25f, 0.25f, 0.9f), used, "攻");
                used = DrawBoxSet(frame.damageBoxes, new Color(0.3f, 1f, 0.4f, 0.9f), used, "受");
            }
            for (int i = used; i < boxRenderers.Count; i++)
                boxRenderers[i].enabled = false;
        }

        private int DrawBoxSet(AnimBox[] boxes, Color color, int used, string label)
        {
            if (boxes == null) return used;
            foreach (AnimBox box in boxes)
            {
                if (used >= boxRenderers.Count) return used;
                // DNF 像素 → 游戏单位：/100，y(纵深)/z(高度) 对调（LSHitboxComponentSystem 同构）
                float xMin = box.min.x / 100f, xMax = box.max.x / 100f;
                float yMin = box.min.z / 100f, yMax = box.max.z / 100f;   // z=高度 → Unity y
                float zMin = box.min.y / 100f, zMax = box.max.y / 100f;   // y=纵深 → Unity z
                Sprite boxSprite = MakeBoxSprite(xMin, xMax, yMin, yMax, color);
                if (boxSprite == null) continue;
                SpriteRenderer renderer = boxRenderers[used++];
                renderer.enabled = true;
                renderer.sprite = boxSprite;
                renderer.sortingOrder = 50;   // 一切之上
                // 盒子锚定角色锚点（imagePos 由本体层承担；盒心=盒几何中心，x 以 DNF 原点）
                float cx = (xMin + xMax) / 2f;
                float cy = (yMin + yMax) / 2f;
                renderer.transform.localPosition = new Vector3(cx, cy, -0.1f - (zMin + zMax) / 200f);
            }
            return used;
        }

        private void EnsureBoxRenderers()
        {
            if (boxRenderers.Count > 0) return;
            for (int i = 0; i < 8; i++)
            {
                GameObject go = new($"Box_{i}");
                go.transform.SetParent(root, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.enabled = false;
                boxRenderers.Add(renderer);
            }
        }

        private static Sprite MakeBoxSprite(float xMin, float xMax, float yMin, float yMax, Color color)
        {
            int w = Mathf.Max(2, Mathf.CeilToInt((xMax - xMin) * 100f));
            int h = Mathf.Max(2, Mathf.CeilToInt((yMax - yMin) * 100f));
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool border = x < 2 || y < 2 || x >= w - 2 || y >= h - 2;
                pixels[y * w + x] = border ? color : new Color(color.r, color.g, color.b, 0.08f);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private int debugLayerUsed;

        private void DrawUnitLayer(
            SpriteRenderer renderer,
            string atlasPath,
            AnimFrameData frame,
            bool isBody)
        {
            debugLayerUsed++;
            if (!SkillNpkSpriteStore.TryGetEntry(atlasPath, frame.image.index, out Sprite sprite,
                    out Vector2 center, out string spriteError))
            {
                renderer.enabled = false;
                Report.UnitLayers.Add(new LineInfo
                {
                    Text = $"{LayerLabel(atlasPath)} {ShortName(atlasPath)}#{frame.image.index} 缺失: {spriteError}",
                    IsWarning = true,
                });
                return;
            }
            ApplySprite(renderer, sprite, frame.imagePos.x + center.x, frame.imagePos.y + center.y);
            Report.UnitLayers.Add(BuildLayerLine(LayerLabel(atlasPath), atlasPath, frame.image.index, sprite, center, frame.imagePos));
        }

        private void DrawSingleFromFrame(SpriteRenderer renderer, AnimFrameData frame, string label)
        {
            if (frame.image.path == null || frame.image.path.Length == 0)
            {
                renderer.enabled = false;
                Report.UnitLayers.Add(new LineInfo { Text = $"{label} 空帧（无贴图）" });
                return;
            }
            if (!SkillNpkSpriteStore.TryGetEntry(frame.image.path, frame.image.index, out Sprite sprite,
                    out Vector2 center, out string spriteError))
            {
                renderer.enabled = false;
                Report.UnitLayers.Add(new LineInfo
                {
                    Text = $"{label} {ShortName(frame.image.path)}#{frame.image.index} 缺失: {spriteError}",
                    IsWarning = true,
                });
                return;
            }
            ApplySprite(renderer, sprite, frame.imagePos.x + center.x, frame.imagePos.y + center.y);
            Report.UnitLayers.Add(BuildLayerLine(label, frame.image.path, frame.image.index, sprite, center, frame.imagePos));
        }

        private void DrawOverlay(AnimOverlayConfig overlay, int frame)
        {
            if (overlay?.overlays == null || overlayRenderer == null)
            {
                if (overlayRenderer != null) overlayRenderer.enabled = false;
                return;
            }
            foreach (AnimOverlayEntry entry in overlay.overlays)
            {
                if (entry.effectAnimId == AnimId.None) continue;
                if (entry.startFrame >= 0 && frame < entry.startFrame)
                {
                    Report.Overlays.Add(new LineInfo
                    {
                        Text = $"特效 {entry.effectAni}（animId={entry.effectAnimId}）等待 F{entry.startFrame} 起",
                    });
                    continue;
                }
                AnimClipData effectClip = SkillAnimCatalog.GetClip(entry.effectAnimId, out string clipError);
                if (effectClip?.frames == null || effectClip.frames.Length == 0)
                {
                    Report.Overlays.Add(new LineInfo
                    {
                        Text = $"特效 {entry.effectAni}（animId={entry.effectAnimId}）clip 缺失: {clipError}",
                        IsWarning = true,
                    });
                    continue;
                }
                int effectFrame = Mathf.Clamp(frame, 0, effectClip.frames.Length - 1);
                AnimFrameData effectFrameData = effectClip.frames[effectFrame];

                // 空帧 = DNF 数据本意（该帧无特效图），正常跳过不报错
                if (effectFrameData.image.path == null || effectFrameData.image.path.Length == 0)
                {
                    Report.Overlays.Add(new LineInfo
                    {
                        Text = $"特效 {entry.effectAni} 帧{effectFrame + 1}/{effectClip.frames.Length} 空帧（无贴图，正常）",
                    });
                    overlayRenderer.enabled = false;
                    return;
                }
                if (!SkillNpkSpriteStore.TryGetEntry(effectFrameData.image.path, effectFrameData.image.index,
                        out Sprite sprite, out Vector2 center, out string spriteError))
                {
                    Report.Overlays.Add(new LineInfo
                    {
                        Text = $"特效 {entry.effectAni} {ShortName(effectFrameData.image.path)}#{effectFrameData.image.index} 缺失: {spriteError}",
                        IsWarning = true,
                    });
                    overlayRenderer.enabled = false;
                    return;
                }

                overlayRenderer.enabled = true;
                overlayRenderer.sprite = sprite;
                // graphicEffect==1 = LINEARDODGE 加法混合（运行时同 shader，消黑底）
                overlayRenderer.sharedMaterial = effectFrameData.graphicEffect == 1 && additiveMaterial != null
                    ? additiveMaterial
                    : null;
                float offX = Mathf.Round(effectFrameData.imagePos.x + center.x) / PixelsPerUnit;
                float offY = Mathf.Round(effectFrameData.imagePos.y + center.y) / PixelsPerUnit;
                overlayRenderer.transform.localPosition = new Vector3(offX, -offY, 0f);
                Report.Overlays.Add(BuildLayerLine(
                    $"特效 {entry.effectAni}", effectFrameData.image.path, effectFrameData.image.index,
                    sprite, center, effectFrameData.imagePos));
                return;   // 首版一次一层
            }
            overlayRenderer.enabled = false;
        }

        private void DrawSpawnViews(IReadOnlyList<SpawnViewSample> spawnViews, bool facingLeft)
        {
            int used = 0;
            if (spawnViews != null)
            {
                foreach (SpawnViewSample view in spawnViews)
                {
                    if (view?.Clip == null || used + 2 > spawnRenderers.Count) continue;
                    if (view.ElapsedMs > view.TotalMs) continue;   // 已到期消失

                    int frame = SampleFrame(view.Clip, view.ElapsedMs);
                    AnimFrameData frameData = view.Clip.frames[frame];

                    SpriteRenderer main = spawnRenderers[used++];
                    SpriteRenderer back = spawnRenderers[used++];
                    DrawSpawnLayer(main, view, frameData);
                    if (view.BackClip != null)
                    {
                        int backFrame = SampleFrame(view.BackClip, view.ElapsedMs);
                        DrawSpawnLayer(back, view, view.BackClip.frames[backFrame]);
                    }
                    else
                    {
                        back.enabled = false;
                    }

                    // inFront/Bullet 位移：x 横移（面向左时镜像根已翻转，localPosition.x 不需取反）
                    main.transform.parent.localPosition = new Vector3(view.OffsetX, 0f, 0f);
                    back.transform.parent.localPosition = new Vector3(view.OffsetX, 0f, 0f);

                    Report.Spawns.Add(new LineInfo
                    {
                        Text = $"{view.Kind}[{view.AreaId}] {view.Name} @{view.ElapsedMs}/{view.TotalMs}ms"
                            + $" 帧{frame + 1}/{view.Clip.frames.Length}"
                            + (view.OffsetX != 0 ? $" x={view.OffsetX:0.##}" : ""),
                    });
                }
            }
            for (int i = used; i < spawnRenderers.Count; i++)
                spawnRenderers[i].enabled = false;
        }

        private void DrawSpawnLayer(SpriteRenderer renderer, SpawnViewSample view, AnimFrameData frameData)
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
            renderer.sharedMaterial = frameData.graphicEffect == 1 && additiveMaterial != null
                ? additiveMaterial
                : null;
            float offX = Mathf.Round(frameData.imagePos.x + center.x) / PixelsPerUnit;
            float offY = Mathf.Round(frameData.imagePos.y + center.y) / PixelsPerUnit;
            renderer.transform.localPosition = new Vector3(offX, -offY, 0f);
        }

        private static void ApplySprite(SpriteRenderer renderer, Sprite sprite, float px, float py)
        {
            renderer.enabled = true;
            renderer.sprite = sprite;
            float offX = Mathf.Round(px) / PixelsPerUnit;
            float offY = Mathf.Round(py) / PixelsPerUnit;
            renderer.transform.localPosition = new Vector3(offX, -offY, 0f);
        }

        private static LineInfo BuildLayerLine(
            string label, string path, int index, Sprite sprite, Vector2 center, AnimFramePos imagePos)
        {
            string archive = SkillNpkSpriteStore.GetArchiveName(path);
            return new LineInfo
            {
                Text = $"{label}  {System.IO.Path.GetFileName(path)}#{index}"
                    + $"  NPK={archive ?? "?"}"
                    + $"  {sprite.textureRect.width:0}×{sprite.textureRect.height:0}"
                    + $"  center({center.x:0.#},{center.y:0.#})"
                    + $"  pos({imagePos.x},{imagePos.y})",
            };
        }

        private static string LayerLabel(string atlasPath)
            => atlasPath.Contains("katana") ? "武器" : "本体";

        private static string ShortName(string path) => System.IO.Path.GetFileName(path);

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

        // ---- 诊断结构 ----

        internal sealed class LineInfo
        {
            public string Text;
            public bool IsWarning;
        }

        internal sealed class RenderReport
        {
            public LineInfo Animation;
            public readonly List<LineInfo> UnitLayers = new();
            public readonly List<LineInfo> Overlays = new();
            public readonly List<LineInfo> Spawns = new();

            public void Reset(int timeMs)
            {
                Animation = null;
                UnitLayers.Clear();
                Overlays.Clear();
                Spawns.Clear();
            }
        }

        /// <summary>Spawn 预览实例采样参数（窗口按 spawnEvent 触发时刻计算 ElapsedMs/OffsetX）。</summary>
        internal sealed class SpawnViewSample
        {
            public int AreaId;
            public string Name;
            public string Kind;
            public AnimClipData Clip;
            public AnimClipData BackClip;
            public int ElapsedMs;
            public int TotalMs;
            /// <summary>横向偏移（单位）：Area=inFront dist；Bullet=出生偏移+速度推进。</summary>
            public float OffsetX;
        }

        public void Dispose()
        {
            if (utility != null)
            {
                utility.Cleanup();
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
            spawnRenderers.Clear();
            foreach (SpriteRenderer renderer in boxRenderers)
            {
                if (renderer?.sprite != null)
                {
                    UnityEngine.Object.DestroyImmediate(renderer.sprite.texture);
                    UnityEngine.Object.DestroyImmediate(renderer.sprite);
                }
            }
            boxRenderers.Clear();
            if (additiveMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(additiveMaterial);
                additiveMaterial = null;
            }
        }
    }
}
