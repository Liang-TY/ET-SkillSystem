using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ET.Editor
{
    /// <summary>
    /// P2 主窗口（02 §6 的 Step 1 HardAttack 切片）：左资产浏览器 / 中占位预览 /
    /// 右 Skill 基础 Inspector / 下校验问题 + 最小时间轴。
    /// 所有修改经 Session.Execute（命令入口）；Bullet/Area/Buff/Action 的
    /// Content Inspector 在 Step 4 开放；真实预览在 Step 3 接入。
    /// </summary>
    public sealed class SkillEditorWindow : EditorWindow
    {
        [MenuItem("ET/Skill/Editor")]
        public static void Open() => GetWindow<SkillEditorWindow>("技能编辑器");

        private static readonly SkillEditorAssetKind[] Kinds =
        {
            SkillEditorAssetKind.Skill,
            SkillEditorAssetKind.Bullet,
            SkillEditorAssetKind.Area,
            SkillEditorAssetKind.Buff,
            SkillEditorAssetKind.Action,
        };

        private static readonly Color ActiveTabColor = new(0.20f, 0.35f, 0.55f);
        private static readonly Color IssueErrorColor = new(1f, 0.45f, 0.45f);
        private static readonly Color IssueWarnColor = new(1f, 0.8f, 0.35f);
        private static readonly Color OkColor = new(0.6f, 0.9f, 0.6f);

        private readonly SkillEditorDocumentStore listStore = new();
        private readonly Dictionary<SkillEditorAssetKind, Button> kindButtons = new();
        private readonly List<SkillEditorAsset> listedAssets = new();

        private SkillEditorSession session;
        private SkillEditorAssetKind currentKind = SkillEditorAssetKind.Skill;
        private string searchQuery = string.Empty;
        private bool viewsDirty;

        private SkillEditorDocument lastOpened;
        private bool lastDirty;

        private TextField searchField;
        private ListView assetList;
        private Label dirtyLabel;
        private Button undoButton;
        private Button redoButton;
        private VisualElement inspectorContainer;
        private Label previewTitle;
        private TextField debugLabel;
        private bool playing;
        private bool loopPlayback;
        private int playAccumulatorMs;
        private double lastPlayTime;
        private Toggle showBoxesToggle;
        private Label legendLabel;
        private Image previewImage;
        private SkillPreviewController previewController;
        private int lastRenderedFrame = -1;
        private int lastRenderedTime = -1;
        private Label timeLabel;
        private SkillTimelineElement timeline;
        private ScrollView issuesScroll;

        public void CreateGUI()
        {
            session = new SkillEditorSession();
            session.Changed += MarkViewsDirty;
            BuildUI();
            RefreshAssetList();
            UpdateKindButtonStates();
            MarkViewsDirty();
        }

        private void TogglePlay()
        {
            if (!playing)
            {
                playing = true;
                playAccumulatorMs = session.Preview.TimeMs;
                lastPlayTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                playing = false;   // 暂停
            }
        }

        private void StopPlay()
        {
            playing = false;
            playAccumulatorMs = 0;
            session.Preview.TimeMs = 0;
            timeline.CurrentTimeMs = 0;
            lastRenderedTime = -1;
            lastPlayTime = 0;
        }

        private void TickPlay()
        {
            if (!playing) return;
            double now = EditorApplication.timeSinceStartup;
            int deltaMs = Mathf.RoundToInt((float)((now - lastPlayTime) * 1000.0));
            lastPlayTime = now;
            if (deltaMs <= 0) return;

            playAccumulatorMs += deltaMs;
            int total = timeline.DurationMs;
            if (playAccumulatorMs >= total)
            {
                if (loopPlayback) playAccumulatorMs %= Mathf.Max(1, total);
                else
                {
                    playAccumulatorMs = total;
                    playing = false;
                }
            }
            session.Preview.TimeMs = playAccumulatorMs;
            timeline.CurrentTimeMs = playAccumulatorMs;
            lastRenderedTime = -1;   // 触发 Update 重渲染
            timeLabel.text = TimeText();
            previewTitle.text = BuildPreviewText();
        }

        private void RenderPreviewIfReady()
        {
            if (session?.Document == null || previewController == null) return;
            SkillParamJson skill = session.Document.Skill;
            if (skill == null) return;

            // animId=0 = 沿用上一段动画继续推帧（运行时 if (AnimId > 0) PlayAnim 语义）
            int animId = ResolveAnimId(skill, session.Preview.TimeMs, out int _);
            if (animId <= 0)
            {
                previewImage.image = null;
                previewTitle.text = "整个技能无动画（所有 phase animId=0）";
                return;
            }

            AnimClipData clip = SkillAnimCatalog.GetClip(animId, out string clipError);
            if (clip == null)
            {
                previewImage.image = null;
                previewTitle.text = $"动画缺失 animId={animId}: {clipError}";
                return;
            }

            List<SkillPreviewController.SpawnViewSample> spawnViews =
                CollectSpawnViews(skill, clip, session.Preview.TimeMs);

            bool ok = previewController.Render(
                clip, SkillAnimCatalog.GetOverlay(animId), spawnViews, session.Preview.TimeMs,
                session.Preview.InheritedPhaseStartMs, session.Preview.FacingLeft,
                out int frameIndex, out string renderError);
            if (!ok)
            {
                previewImage.image = null;
                previewTitle.text = $"渲染失败 animId={animId}: {renderError ?? clipError}";
                debugLabel.value = renderError ?? clipError;
                return;
            }
            previewImage.image = previewController.Texture;
            lastRenderedFrame = frameIndex;
            lastRenderedTime = session.Preview.TimeMs;
            RefreshDiagnostics(animId, clip);
        }

        /// <summary>诊断面板刷新：Report + 事件/盒体上下文 → 可复制多行文本。</summary>
        private void RefreshDiagnostics(int animId, AnimClipData clip)
        {
            SkillPreviewController.RenderReport report = previewController.Report;
            var lines = new List<string>();
            if (report.Animation != null) lines.Add($"[动画] {report.Animation.Text}");

            SkillParamJson skill = session.Document?.Skill;
            int phaseIndex = timeline.Projection.LocatePhase(session.Preview.TimeMs);
            SkillPhaseJson[] phases = skill?.phases;
            string phaseText = phases != null && phaseIndex >= 0 && phaseIndex < phases.Length
                ? $"phase{phaseIndex}(animId={phases[phaseIndex].animId})"
                : "-";
            lines.Add($"[时间] t={session.Preview.TimeMs}ms / {timeline.DurationMs}ms  {phaseText}"
                + $"  地址={SkillAnimCatalog.GetAddress(animId) ?? "无"}");

            foreach (SkillPreviewController.LineInfo line in report.UnitLayers)
                lines.Add($"[{(line.IsWarning ? "层!" : "层")}] {line.Text}");
            foreach (SkillPreviewController.LineInfo line in report.Overlays)
                lines.Add($"[{(line.IsWarning ? "特效!" : "特效")}] {line.Text}");
            foreach (SkillPreviewController.LineInfo line in report.Spawns)
                lines.Add($"[生成] {line.Text}");

            // 未触发的 spawnEvent（检查"这个技能应该出什么特效"）
            if (skill?.spawnEvents != null)
            {
                foreach (SkillSpawnEventJson spawn in skill.spawnEvents)
                {
                    if (spawn == null) continue;
                    if (spawn.kind != "createArea" && spawn.kind != "createBullet") continue;
                    int trigger = ResolveTriggerMs(spawn, clip);
                    if (trigger < 0)
                        lines.Add($"[等待] {spawn.kind} {(spawn.areaId ?? spawn.bulletId ?? 0)}"
                            + $" {spawn.timeBase}"
                            + (spawn.timeBase == "AnimationFrame" ? $" F{spawn.atFrame}" : $" @{spawn.atMs}ms")
                            + $" at={spawn.at}（语义时刻，预览不定时）");
                    else if (trigger > session.Preview.TimeMs)
                        lines.Add($"[等待] {spawn.kind} {(spawn.areaId ?? spawn.bulletId ?? 0)}"
                            + $" @{trigger}ms（还有 {trigger - session.Preview.TimeMs}ms）");
                }
            }

            // 当前帧盒体原始数据（DNF 像素）；未渲染过(lastRenderedFrame=-1)或越界则跳过
            if (lastRenderedFrame >= 0 && lastRenderedFrame < clip.frames.Length)
            {
                AnimFrameData frameData = clip.frames[lastRenderedFrame];
                AnimBox[] dmg = frameData.damageBoxes;
                if ((dmg == null || dmg.Length == 0)
                    && (frameData.damageBox.min.x != 0 || frameData.damageBox.max.x != 0))
                    dmg = new[] { frameData.damageBox };   // 旧 JSON 单数兼容
                lines.Add("[盒子] 受击=" + FormatBoxes(dmg) + "  攻击=" + FormatBoxes(frameData.attackBoxes));
            }

            debugLabel.value = string.Join("\n", lines);
        }

        private static string FormatBoxes(AnimBox[] boxes)
        {
            if (boxes == null || boxes.Length == 0) return "无";
            var parts = new List<string>();
            foreach (AnimBox box in boxes)
                parts.Add($"({box.min.x},{box.min.y},{box.min.z})~({box.max.x},{box.max.y},{box.max.z})");
            return string.Join(" ", parts);
        }

        /// <summary>
        /// 当前时刻应采样的 animId：animId=0 的 phase 继承最近一个非 0 phase 的动画，
        /// 时间基准换到该 phase 起点（运行时 PlayAnim 不被 0 段打断、帧连续推进）。
        /// </summary>
        private int ResolveAnimId(SkillParamJson skill, int timeMs, out int phaseIndex)
        {
            phaseIndex = -1;
            if (timeline.Projection.PhaseCount == 0) return 0;
            int current = timeline.Projection.LocatePhase(timeMs);
            phaseIndex = current;
            SkillPhaseJson[] phases = skill.phases;
            if (phases == null || current < 0 || current >= phases.Length) return 0;
            if (phases[current].animId > 0)
            {
                session.Preview.InheritedPhaseStartMs = -1;
                return phases[current].animId;
            }

            // 向前找最近非 0：动画从那个 phase 起点起播（预览按该起点累计采样时间）
            for (int i = current - 1; i >= 0; i--)
            {
                if (phases[i].animId <= 0) continue;
                session.Preview.InheritedPhaseStartMs = timeline.Projection.PhaseStart(i);
                return phases[i].animId;
            }
            session.Preview.InheritedPhaseStartMs = -1;
            return 0;
        }

        /// <summary>
        /// 按 spawnEvents 采样当前时刻应显示的 Area/Bullet 视图（运行时 LSAreaView/BulletView 同构：
        /// 到点创建 → 独立推帧 → total 后消失；at=inFront 时横移 dist；Bullet 再按速度推进）。
        /// AnimationFrame 精确定时：FrameToMs 按 clip 帧 delay 累计（血爆 atFrame=22 → 910ms）。
        /// </summary>
        private List<SkillPreviewController.SpawnViewSample> CollectSpawnViews(
            SkillParamJson skill, AnimClipData bodyClip, int timeMs)
        {
            var result = new List<SkillPreviewController.SpawnViewSample>();
            if (skill.spawnEvents == null || skill.spawnEvents.Length == 0) return result;

            foreach (SkillSpawnEventJson spawn in skill.spawnEvents)
            {
                if (spawn == null) continue;
                if (spawn.kind != "createArea" && spawn.kind != "createBullet") continue;

                int triggerMs = ResolveTriggerMs(spawn, bodyClip);
                if (triggerMs < 0 || timeMs < triggerMs) continue;
                int elapsed = timeMs - triggerMs;

                if (spawn.kind == "createArea")
                {
                    AreaParamJson area = FindAsset(SkillEditorAssetKind.Area, spawn.areaId ?? 0, d => d.Area);
                    if (area == null) continue;

                    var sample = new SkillPreviewController.SpawnViewSample
                    {
                        AreaId = area.id,
                        Name = area.name,
                        Kind = "Area",
                        ElapsedMs = elapsed,
                        TotalMs = area.totalTimeMs,
                        OffsetX = spawn.at == "inFront" ? spawn.dist : 0f,
                    };
                    if (area.viewAnimId > 0)
                        sample.Clip = SkillAnimCatalog.GetClip(area.viewAnimId, out string _);
                    if (area.viewBackAnimId > 0)
                        sample.BackClip = SkillAnimCatalog.GetClip(area.viewBackAnimId ?? 0, out string _);
                    if (sample.Clip != null) result.Add(sample);
                }
                else
                {
                    BulletParamJson bullet = FindAsset(SkillEditorAssetKind.Bullet, spawn.bulletId ?? 0, d => d.Bullet);
                    if (bullet == null) continue;

                    // 弹体按速度推进：x = 出生偏移 + speed * elapsed（inFront 叠加）
                    float distance = bullet.speed * elapsed / 1000f;
                    var sample = new SkillPreviewController.SpawnViewSample
                    {
                        AreaId = bullet.id,
                        Name = bullet.name,
                        Kind = "Bullet",
                        ElapsedMs = elapsed,
                        TotalMs = bullet.totalTimeMs,
                        OffsetX = (spawn.at == "inFront" ? spawn.dist : 0f)
                            + (bullet.spawnOffset != null && bullet.spawnOffset.Length > 0 ? bullet.spawnOffset[0] : 0f)
                            + distance,
                    };
                    if (bullet.viewAnimId > 0)
                        sample.Clip = SkillAnimCatalog.GetClip(bullet.viewAnimId, out string _);
                    if (sample.Clip != null) result.Add(sample);
                }
            }
            return result;
        }

        /// <summary>
        /// spawnEvent 触发时刻（cast 全局 ms）。AnimationFrame = FrameToMs（帧 delay 累计，
        /// 继承动画时基准回继承相位起点）；PhaseTime/Enter/End/CastTime 走投影；
        /// Landing/Input 无确定时刻返回 -1（诊断面板显示"等待触发"）。
        /// </summary>
        private int ResolveTriggerMs(SkillSpawnEventJson spawn, AnimClipData bodyClip)
        {
            SkillParamTimeBase timeBase = Enum.TryParse(spawn.timeBase, true, out SkillParamTimeBase tb)
                ? tb
                : SkillParamTimeBase.CastTime;
            switch (timeBase)
            {
                case SkillParamTimeBase.AnimationFrame:
                {
                    if (spawn.atFrame > 0)
                    {
                        int frameMs = SkillAnimCatalog.FrameToMs(bodyClip, spawn.atFrame);
                        if (frameMs < 0) return -1;
                        // 默认基准 = 事件所属 phase 起点；若该 phase 是继承动画，动画实际
                        // 从最近非 0 相位起播，帧时刻相对继承起点
                        int baseMs = spawn.phase >= 0 ? timeline.Projection.PhaseStart(spawn.phase) : 0;
                        if (spawn.phase >= 0)
                        {
                            SkillPhaseJson[] phases = session.Document?.Skill?.phases;
                            if (phases != null && spawn.phase < phases.Length && phases[spawn.phase].animId <= 0)
                            {
                                for (int i = spawn.phase - 1; i >= 0; i--)
                                {
                                    if (phases[i].animId <= 0) continue;
                                    baseMs = timeline.Projection.PhaseStart(i);
                                    break;
                                }
                            }
                        }
                        return baseMs + frameMs;
                    }
                    return spawn.phase >= 0
                        ? timeline.Projection.PhaseStart(spawn.phase) + spawn.atMs
                        : spawn.atMs;
                }
                case SkillParamTimeBase.PhaseTime:
                    return spawn.phase >= 0
                        ? timeline.Projection.PhaseStart(spawn.phase) + spawn.atMs
                        : spawn.atMs;
                case SkillParamTimeBase.PhaseEnter:
                    return spawn.phase >= 0 ? timeline.Projection.PhaseStart(spawn.phase) : 0;
                case SkillParamTimeBase.PhaseEnd:
                    return spawn.phase >= 0 ? timeline.Projection.PhaseEnd(spawn.phase) : 0;
                case SkillParamTimeBase.CastTime:
                    return spawn.atMs;
                case SkillParamTimeBase.Landing:
                    // 预览无物理：近似"当前动画播完"（DNF 落地事件本质=空中段结束）
                    return bodyClip != null
                        ? (spawn.phase >= 0 ? timeline.Projection.PhaseStart(spawn.phase) : 0)
                            + SkillAnimCatalog.FrameToMs(bodyClip, bodyClip.frames.Length) // = clip 总时长
                        : -1;
                default:
                    return -1;   // Input：语义事件无确定时刻
            }
        }

        /// <summary>按 kind+id 从磁盘目录直查资产（不走全局 Loader——ISSUE-014 约束）。</summary>
        private T FindAsset<T>(SkillEditorAssetKind kind, int id, Func<SkillEditorDocument, T> pick)
            where T : class
        {
            if (id <= 0) return null;
            SkillEditorDocumentStore store = new();
            SkillEditorAsset asset = store.Find(kind, id);
            if (asset == null || !SkillEditorDocument.TryLoad(asset, out SkillEditorDocument document, out string _))
                return null;
            return pick(document);
        }

        private void OnDisable()
        {
            previewController?.Dispose();
            previewController = null;
            previewImage = null;
            SkillNpkSpriteStore.ClearCache();
            if (session == null) return;
            session.Changed -= MarkViewsDirty;
            session.Dispose();
            session = null;
        }

        /// <summary>session.Changed 可能在 UI 事件回调内触发，延迟一帧再改 UI 树。</summary>
        private void MarkViewsDirty() => viewsDirty = true;

        private void Update()
        {
            TickPlay();
            if (!viewsDirty && session != null
                && lastRenderedTime != session.Preview.TimeMs)
            {
                RenderPreviewIfReady();   // 播放头移动即采样（时间轴拖动/播放循环共用）
            }
            if (!viewsDirty) return;
            viewsDirty = false;
            RefreshDynamic();
            RenderPreviewIfReady();
        }

        private void BuildUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            // 顶部工具条
            VisualElement toolbar = new();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.marginTop = 4;
            toolbar.style.marginBottom = 4;
            toolbar.style.paddingLeft = 6;
            toolbar.style.paddingRight = 6;
            foreach (SkillEditorAssetKind kind in Kinds)
            {
                Button button = new(() => OnKindSelected(kind)) { text = kind.ToString() };
                kindButtons[kind] = button;
                toolbar.Add(button);
            }

            searchField = new TextField("搜索") { style = { flexGrow = 1 } };
            searchField.RegisterValueChangedCallback(_ =>
            {
                searchQuery = searchField.value;
                RefreshAssetList();
            });
            toolbar.Add(searchField);

            dirtyLabel = new Label(string.Empty)
            {
                style = { marginLeft = 8, marginRight = 8, unityTextAlign = TextAnchor.MiddleRight },
            };
            undoButton = new Button(() => session?.Undo()) { text = "撤销" };
            redoButton = new Button(() => session?.Redo()) { text = "重做" };
            Button saveButton = new(OnSaveClicked) { text = "保存" };
            Button reloadButton = new(OnReloadClicked) { text = "Reload" };
            toolbar.Add(dirtyLabel);
            toolbar.Add(undoButton);
            toolbar.Add(redoButton);
            toolbar.Add(saveButton);
            toolbar.Add(reloadButton);
            root.Add(toolbar);

            // 三栏主体
            VisualElement body = new();
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1;

            VisualElement left = new();
            left.style.width = 260;
            left.style.flexShrink = 0;
            left.style.borderRightWidth = 1;
            left.style.borderRightColor = new Color(0.25f, 0.27f, 0.3f);
            assetList = new ListView { selectionType = SelectionType.Single };
            assetList.style.flexGrow = 1;
            assetList.makeItem = () =>
            {
                Label label = new()
                {
                    style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 },
                };
                return label;
            };
            assetList.bindItem = (element, index) =>
            {
                if (index < 0 || index >= listedAssets.Count) return;
                SkillEditorAsset asset = listedAssets[index];
                Label label = (Label)element;
                string marker = "  ";
                if (session?.Document != null
                    && string.Equals(session.Document.Asset.Path, asset.Path, StringComparison.OrdinalIgnoreCase))
                {
                    marker = session.IsDirty ? "* " : "• ";
                }
                label.text = marker + asset.DisplayName;
                label.style.color = asset.Error != null ? IssueErrorColor : Color.white;
            };
            assetList.itemsSource = listedAssets;
            assetList.selectionChanged += items =>
            {
                foreach (object item in items)
                {
                    if (item is SkillEditorAsset asset) OpenAsset(asset);
                    break;
                }
            };
            left.Add(assetList);
            body.Add(left);

            VisualElement center = new();
            center.style.flexGrow = 1;
            center.style.paddingLeft = 6;
            center.style.paddingRight = 6;
            VisualElement previewArea = new();
            previewArea.style.flexGrow = 1;
            previewArea.style.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            previewArea.style.justifyContent = Justify.FlexStart;
            previewArea.style.paddingTop = 8;
            previewTitle = new Label("预览");
            previewArea.Add(previewTitle);
            // 6000.0.25f1 IStyle 无 aspectRatioWidthHeight（也无 aspectRatio），用 ScaleToFit 防拉伸
            previewImage = new Image { image = null, scaleMode = ScaleMode.ScaleToFit };
            previewImage.style.flexGrow = 1;
            previewArea.Add(previewImage);
            VisualElement previewToolbar = new() { style = { flexDirection = FlexDirection.Row } };
            Button faceButton = new(() =>
            {
                session.Preview.FacingLeft = !session.Preview.FacingLeft;
                lastRenderedTime = -1;   // 强制重渲染
                RenderPreviewIfReady();
                previewTitle.text = BuildPreviewText();
            }) { text = "翻转朝向" };
            previewToolbar.Add(faceButton);
            Button playButton = new(TogglePlay) { text = "播放" };
            previewToolbar.Add(playButton);
            Button stopButton = new(StopPlay) { text = "停止" };
            previewToolbar.Add(stopButton);
            Toggle loopToggle = new("循环") { value = loopPlayback };
            loopToggle.RegisterValueChangedCallback(evt =>
            {
                loopPlayback = evt.newValue;
            });
            previewToolbar.Add(loopToggle);
            showBoxesToggle = new Toggle("攻击/受击盒") { value = false };
            showBoxesToggle.RegisterValueChangedCallback(evt =>
            {
                if (previewController != null) previewController.ShowBoxes = evt.newValue;
                lastRenderedTime = -1;
            });
            previewToolbar.Add(showBoxesToggle);
            previewArea.Add(previewToolbar);
            debugLabel = new TextField
            {
                value = "渲染后显示诊断",
                isReadOnly = true,
                multiline = true,   // TextField 无 (int,int) 构造；多行只读诊断
            };
            debugLabel.style.flexGrow = 0;
            debugLabel.style.maxHeight = 110;
            debugLabel.style.whiteSpace = WhiteSpace.Normal;
            center.Add(debugLabel);
            previewController = new SkillPreviewController();
            center.Add(previewArea);
            timeLabel = new Label("t=0ms / 0ms");
            center.Add(timeLabel);
            body.Add(center);

            VisualElement right = new();
            right.style.width = 340;
            right.style.flexShrink = 0;
            right.style.borderLeftWidth = 1;
            right.style.borderLeftColor = new Color(0.25f, 0.27f, 0.3f);
            ScrollView inspectorScroll = new(ScrollViewMode.Vertical);
            inspectorScroll.style.flexGrow = 1;
            inspectorContainer = new VisualElement { style = { paddingLeft = 6, paddingRight = 6 } };
            inspectorScroll.Add(inspectorContainer);
            right.Add(inspectorScroll);
            body.Add(right);
            root.Add(body);

            // 下方：校验问题 + 时间轴
            legendLabel = new Label("图例：色块=Phase 段（第1/2/3段循环配色）  绿条=手动攻击盒窗口(onMs-offMs)  "
                + "橙菱形=SpawnEvent 橙条=输入窗口(Input untilMs)  黄菱形=HitEvent  黄竖线=播放头  拖动=设时间 滚轮=缩放")
            {
                style =
                {
                    color = new Color(0.55f, 0.6f, 0.65f),
                    fontSize = 10,
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 6,
                    flexShrink = 0,
                },
            };
            root.Add(legendLabel);
            issuesScroll = new ScrollView(ScrollViewMode.Vertical) { style = { maxHeight = 90, flexShrink = 0 } };
            root.Add(issuesScroll);
            timeline = new SkillTimelineElement { style = { flexShrink = 0 } };
            timeline.TimeChanged += OnTimelineTimeChanged;
            root.Add(timeline);

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKindSelected(SkillEditorAssetKind kind)
        {
            currentKind = kind;
            RefreshAssetList();
            UpdateKindButtonStates();
        }

        private void UpdateKindButtonStates()
        {
            foreach (KeyValuePair<SkillEditorAssetKind, Button> pair in kindButtons)
            {
                pair.Value.style.backgroundColor = pair.Key == currentKind
                    ? ActiveTabColor
                    : StyleKeyword.Null;
            }
        }

        private void RefreshAssetList()
        {
            listedAssets.Clear();
            foreach (SkillEditorAsset asset in SkillEditorOperations.List(currentKind, searchQuery, true, listStore))
                listedAssets.Add(asset);
            assetList.itemsSource = listedAssets;
            assetList.RefreshItems();
        }

        private void OpenAsset(SkillEditorAsset asset)
        {
            if (session.IsDirty
                && !EditorUtility.DisplayDialog("打开", "当前文档有未保存修改，丢弃并打开新文档？", "丢弃", "取消"))
            {
                return;
            }
            if (!session.TryOpen(asset, true, out string error))
                EditorUtility.DisplayDialog("打开失败", error, "确定");
        }

        private void OnSaveClicked()
        {
            if (session.TrySave(out string error)) return;
            EditorUtility.DisplayDialog("保存失败", error, "确定");
        }

        private void OnReloadClicked()
        {
            if (session.Document == null) return;
            if (session.IsDirty
                && !EditorUtility.DisplayDialog("Reload", "存在未保存修改，丢弃并从磁盘重新加载？", "丢弃", "取消"))
            {
                return;
            }
            if (!session.TryReload(session.IsDirty, out string error))
                EditorUtility.DisplayDialog("Reload 失败", error, "确定");
        }

        private void OnTimelineTimeChanged(int timeMs)
        {
            session.Preview.TimeMs = timeMs;
            timeLabel.text = TimeText();
            previewTitle.text = BuildPreviewText();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!evt.ctrlKey || evt.keyCode != KeyCode.Z) return;
            // 焦点在文本控件内时交给控件自身 Undo；空焦区域走命令历史
            if (!ReferenceEquals(evt.target, rootVisualElement)) return;
            bool handled = evt.shiftKey ? session.Redo() : session.Undo();
            if (handled) evt.StopPropagation();
        }

        private void RefreshDynamic()
        {
            if (session == null) return;

            dirtyLabel.text = session.Document == null ? "未打开" : session.IsDirty ? "● 未保存" : "已保存";
            dirtyLabel.style.color = session.IsDirty ? new Color(1f, 0.6f, 0.2f) : OkColor;
            undoButton.SetEnabled(session.Document != null && session.History.CanUndo);
            redoButton.SetEnabled(session.Document != null && session.History.CanRedo);
            UpdateKindButtonStates();

            bool documentChanged = !ReferenceEquals(session.Document, lastOpened);
            bool dirtyCleared = lastDirty && !session.IsDirty;
            if (documentChanged || dirtyCleared)
            {
                lastOpened = session.Document;
                listStore.InvalidateCatalog();
                RefreshAssetList();
            }
            lastDirty = session.IsDirty;

            timeline.SetDocument(session.Document?.Skill);
            timeline.CurrentTimeMs = session.Preview.TimeMs;   // 换文档后播放头与预览时间同步
            timeLabel.text = TimeText();
            previewTitle.text = BuildPreviewText();
            RebuildInspector();
            RebuildIssues();
        }

        private string TimeText()
        {
            if (session?.Document == null) return "t=0ms / 0ms";
            return $"t={session.Preview.TimeMs}ms / {timeline.DurationMs}ms";
        }

        private string BuildPreviewText()
        {
            if (session?.Document == null) return "未打开文档";
            SkillParamJson skill = session.Document.Skill;
            if (skill == null)
                return $"{session.Document.Asset.Kind} 预览在 Step 4 接入（当前为 Skill 纵向切片）";
            int phase = timeline.Projection.LocatePhase(session.Preview.TimeMs);
            return $"预览（Step 3 接入真实渲染）  t={session.Preview.TimeMs}ms  "
                + $"phase={phase}/{timeline.Projection.PhaseCount}  "
                + $"朝向={(session.Preview.FacingLeft ? "左" : "右")}";
        }

        private void RebuildInspector()
        {
            inspectorContainer.Clear();
            if (session.Document == null)
            {
                inspectorContainer.Add(new Label("左侧选择一个资产打开"));
                return;
            }
            switch (session.Document.Asset.Kind)
            {
                case SkillEditorAssetKind.Skill:
                    BuildSkillInspector(inspectorContainer);
                    break;
                default:
                    inspectorContainer.Add(new Label(
                        $"{session.Document.Asset.Kind} 的 Content Inspector 在 Step 4 开放（当前为 Skill 纵向切片）"));
                    break;
            }
        }

        private void BuildSkillInspector(VisualElement parent)
        {
            SkillParamJson skill = session.Document.Skill;
            if (skill == null)
            {
                parent.Add(new Label("文档不是有效的 Skill"));
                return;
            }

            parent.Add(new IntegerField("id") { isReadOnly = true, value = skill.id });
            parent.Add(SkillInspectorFields.Text(
                "name", skill.name,
                () => session.Document?.Skill?.name,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 name → {value}", document => document.Skill.name = value))));
            parent.Add(SkillInspectorFields.Enum<SkillParamType>(
                "type", skill.type,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 type → {value}", document => document.Skill.type = value))));
            parent.Add(SkillInspectorFields.Int(
                "cooldownMs", skill.cooldownMs, true,
                () => session.Document?.Skill?.cooldownMs ?? 0,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 cooldownMs → {value}", document => document.Skill.cooldownMs = value))));
            parent.Add(SkillInspectorFields.Int(
                "totalTimeMs", skill.totalTimeMs, true,
                () => session.Document?.Skill?.totalTimeMs ?? 0,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 totalTimeMs → {value}", document => document.Skill.totalTimeMs = value))));
            parent.Add(SkillInspectorFields.Bool(
                "requireAirborne", skill.requireAirborne,
                () => session.Document?.Skill?.requireAirborne ?? false,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 requireAirborne → {value}", document => document.Skill.requireAirborne = value))));
            parent.Add(SkillInspectorFields.Bool(
                "manualCooldown", skill.manualCooldown,
                () => session.Document?.Skill?.manualCooldown ?? false,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 manualCooldown → {value}", document => document.Skill.manualCooldown = value))));

            BuildPhasesBlock(parent, skill);
            BuildReactionsBlock(parent, skill);
            BuildManualBoxesBlock(parent, skill);
            BuildSpawnEventsBlock(parent, skill);
            BuildHitEventsBlock(parent, skill);
        }
        private void BuildPhasesBlock(VisualElement parent, SkillParamJson skill)
        {
            SkillEditorSelection selection = session.Selection;
            SkillInspectorListBlock block = new(
                "Phases",
                skill.phases?.Length ?? 0,
                () => session.Execute(SkillEditorListCommands.AddPhase((skill.phases?.Length ?? 1) - 1)),
                index => session.Execute(SkillEditorListCommands.RemoveAt("phases", index)),
                index => session.Execute(SkillEditorListCommands.DuplicateAt("phases", index)));
            parent.Add(block.Root);

            SkillPhaseJson[] phases = skill.phases ?? Array.Empty<SkillPhaseJson>();
            for (int i = 0; i < phases.Length; i++)
            {
                int index = i;
                SkillPhaseJson phase = phases[i];
                bool selected = selection.PhaseIndex == index && selection.SpawnEventIndex is -3 or -1;
                block.AddItemRow($"[{index}] {phase?.durationMs ?? 0}ms anim={phase?.animId ?? 0}", index, selected,
                    () => SelectPhase(index));
                if (!selected) continue;

                VisualElement detail = new() { style = { paddingLeft = 12, marginBottom = 6 } };
                block.ItemsContainer.Add(detail);
                detail.Add(SkillInspectorFields.Int(
                    "animId", phase.animId, true,
                    () => PhaseAt(index)?.animId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].animId = {value}",
                        document => PhaseAt(document, index).animId = value))));
                detail.Add(SkillInspectorFields.Int(
                    "durationMs", phase.durationMs, true,
                    () => PhaseAt(index)?.durationMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].durationMs = {value}",
                        document => PhaseAt(document, index).durationMs = value))));
                detail.Add(SkillInspectorFields.Int(
                    "cancelMs", phase.cancelMs, true,
                    () => PhaseAt(index)?.cancelMs ?? -1,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].cancelMs = {value}",
                        document => PhaseAt(document, index).cancelMs = value))));
                detail.Add(SkillInspectorFields.Bool(
                    "clearHitTargets", phase.clearHitTargets,
                    () => PhaseAt(index)?.clearHitTargets ?? false,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].clearHitTargets = {value}",
                        document => PhaseAt(document, index).clearHitTargets = value))));
                detail.Add(SkillInspectorFields.Int(
                    "superArmorMs", phase.superArmorMs, true,
                    () => PhaseAt(index)?.superArmorMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].superArmorMs = {value}",
                        document => PhaseAt(document, index).superArmorMs = value))));
                detail.Add(SkillInspectorFields.Int(
                    "nextPhase", phase.nextPhase ?? -1, true,
                    () => PhaseAt(index)?.nextPhase ?? -1,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].nextPhase = {value}",
                        document => PhaseAt(document, index).nextPhase = value < 0 ? null : value))));
                detail.Add(SkillInspectorFields.Int(
                    "nextSkillId", phase.nextSkillId, true,
                    () => PhaseAt(index)?.nextSkillId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].nextSkillId = {value}",
                        document => PhaseAt(document, index).nextSkillId = value))));
                if (phase.nextSkillId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Skill, phase.nextSkillId)}"));
                detail.Add(SkillInspectorFields.Enum<SkillParamNextTrigger>(
                    "nextTrigger", phase.nextTrigger,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"phases[{index}].nextTrigger = {value}",
                        document => PhaseAt(document, index).nextTrigger = value))));

                VisualElement rowButtons = new() { style = { flexDirection = FlexDirection.Row, marginTop = 2 } };
                Button moveUp = new(() => session.Execute(SkillEditorListCommands.MoveWithin("phases", index, index - 1))) { text = "上移" };
                moveUp.SetEnabled(index > 0);
                Button moveDown = new(() => session.Execute(SkillEditorListCommands.MoveWithin("phases", index, index + 1))) { text = "下移" };
                moveDown.SetEnabled(index < phases.Length - 1);
                rowButtons.Add(moveUp);
                rowButtons.Add(moveDown);
                detail.Add(rowButtons);
            }
        }

        private void BuildReactionsBlock(VisualElement parent, SkillParamJson skill)
        {
            SkillEditorSelection selection = session.Selection;
            SkillInspectorListBlock block = new(
                "HitReactions",
                skill.hitReactions?.Length ?? 0,
                () => session.Execute(SkillEditorListCommands.AddHitReaction()),
                index => session.Execute(SkillEditorListCommands.RemoveAt("hitReactions", index)),
                index => session.Execute(SkillEditorListCommands.DuplicateAt("hitReactions", index)));
            parent.Add(block.Root);

            SkillHitReactionJson[] reactions = skill.hitReactions ?? Array.Empty<SkillHitReactionJson>();
            for (int i = 0; i < reactions.Length; i++)
            {
                int index = i;
                SkillHitReactionJson reaction = reactions[i];
                bool selected = selection.PhaseIndex == index && selection.SpawnEventIndex == -2;
                block.AddItemRow($"[phase {reaction?.phase ?? 0}] {reaction?.damage ?? 0} 伤", index, selected,
                    () => SelectReaction(index));
                if (!selected) continue;

                VisualElement detail = new() { style = { paddingLeft = 12, marginBottom = 6 } };
                block.ItemsContainer.Add(detail);
                detail.Add(SkillInspectorFields.Int(
                    "damage", reaction.damage, true,
                    () => ReactionAt(index)?.damage ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitReactions[{index}].damage = {value}",
                        document => ReactionAt(document, index).damage = value))));
                detail.Add(SkillInspectorFields.Int(
                    "hitstunMs", reaction.hitstunMs, true,
                    () => ReactionAt(index)?.hitstunMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitReactions[{index}].hitstunMs = {value}",
                        document => ReactionAt(document, index).hitstunMs = value))));
                detail.Add(SkillInspectorFields.Int(
                    "kbX", (int)reaction.kbX, true,
                    () => (int)(ReactionAt(index)?.kbX ?? 0),
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitReactions[{index}].kbX = {value}",
                        document => ReactionAt(document, index).kbX = value))));
                detail.Add(SkillInspectorFields.Int(
                    "launchY", (int)reaction.launchY, true,
                    () => (int)(ReactionAt(index)?.launchY ?? 0),
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitReactions[{index}].launchY = {value}",
                        document => ReactionAt(document, index).launchY = value))));
                detail.Add(SkillInspectorFields.Int(
                    "procBuffId", reaction.procBuffId, true,
                    () => ReactionAt(index)?.procBuffId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitReactions[{index}].procBuffId = {value}",
                        document => ReactionAt(document, index).procBuffId = value))));
                if (reaction.procBuffId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Buff, reaction.procBuffId)}"));
            }
        }

        private void BuildManualBoxesBlock(VisualElement parent, SkillParamJson skill)
        {
            SkillEditorSelection selection = session.Selection;
            SkillInspectorListBlock block = new(
                "ManualBoxes",
                skill.manualBoxes?.Length ?? 0,
                () => session.Execute(SkillEditorListCommands.AddManualBox()),
                index => session.Execute(SkillEditorListCommands.RemoveAt("manualBoxes", index)),
                index => session.Execute(SkillEditorListCommands.DuplicateAt("manualBoxes", index)));
            parent.Add(block.Root);

            SkillManualBoxJson[] boxes = skill.manualBoxes ?? Array.Empty<SkillManualBoxJson>();
            for (int i = 0; i < boxes.Length; i++)
            {
                int index = i;
                SkillManualBoxJson box = boxes[i];
                bool selected = selection.ManualBoxIndex == index;
                block.AddItemRow($"[phase {box?.phase ?? 0}] {box?.onMs ?? 0}-{box?.offMs ?? 0}ms", index, selected,
                    () => SelectManualBox(index));
                if (!selected) continue;

                VisualElement detail = new() { style = { paddingLeft = 12, marginBottom = 6 } };
                block.ItemsContainer.Add(detail);
                detail.Add(SkillInspectorFields.Int(
                    "phase", box.phase, true,
                    () => BoxAt(index)?.phase ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"manualBoxes[{index}].phase = {value}",
                        document => BoxAt(document, index).phase = value))));
                detail.Add(SkillInspectorFields.Int(
                    "onMs", box.onMs, true,
                    () => BoxAt(index)?.onMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"manualBoxes[{index}].onMs = {value}",
                        document => BoxAt(document, index).onMs = value))));
                detail.Add(SkillInspectorFields.Int(
                    "offMs", box.offMs, true,
                    () => BoxAt(index)?.offMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"manualBoxes[{index}].offMs = {value}",
                        document => BoxAt(document, index).offMs = value))));
                detail.Add(SkillInspectorFields.Hint(
                    $"offset=({Fmt(box.offset, 0)},{Fmt(box.offset, 1)},{Fmt(box.offset, 2)})  "
                    + $"half=({Fmt(box.half, 0)},{Fmt(box.half, 1)},{Fmt(box.half, 2)})  (float[] 编辑随 Step 3)"));
            }
        }

        private static string Fmt(float[] array, int index)
            => array != null && array.Length > index ? array[index].ToString("0.##") : "?";

        private void BuildSpawnEventsBlock(VisualElement parent, SkillParamJson skill)
        {
            SkillEditorSelection selection = session.Selection;
            SkillInspectorListBlock block = new(
                "SpawnEvents",
                skill.spawnEvents?.Length ?? 0,
                () => session.Execute(SkillEditorListCommands.AddSpawnEvent()),
                index => session.Execute(SkillEditorListCommands.RemoveAt("spawnEvents", index)),
                index => session.Execute(SkillEditorListCommands.DuplicateAt("spawnEvents", index)));
            parent.Add(block.Root);

            SkillSpawnEventJson[] spawns = skill.spawnEvents ?? Array.Empty<SkillSpawnEventJson>();
            for (int i = 0; i < spawns.Length; i++)
            {
                int index = i;
                SkillSpawnEventJson spawn = spawns[i];
                bool selected = selection.SpawnEventIndex == index && index >= 0;
                block.AddItemRow(
                    $"[{index}] {(spawn.phase < 0 ? "cast" : $"p{spawn.phase}")} @{spawn.atMs}ms {spawn.kind}"
                    + (spawn.bulletId > 0 ? $" bullet={spawn.bulletId}" : spawn.areaId > 0 ? $" area={spawn.areaId}" : string.Empty),
                    index, selected,
                    () => SelectSpawnEvent(index));
                if (!selected) continue;

                VisualElement detail = new() { style = { paddingLeft = 12, marginBottom = 6 } };
                block.ItemsContainer.Add(detail);
                detail.Add(SkillInspectorFields.Int(
                    "phase", spawn.phase, true,
                    () => SpawnAt(index)?.phase ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].phase = {value}",
                        document => SpawnAt(document, index).phase = value))));
                detail.Add(SkillInspectorFields.Int(
                    "atMs", spawn.atMs, true,
                    () => SpawnAt(index)?.atMs ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].atMs = {value}",
                        document => SpawnAt(document, index).atMs = value))));
                detail.Add(SkillInspectorFields.Enum<SkillParamTimeBase>(
                    "timeBase", spawn.timeBase,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].timeBase = {value}",
                        document => SpawnAt(document, index).timeBase = value))));
                detail.Add(SkillInspectorFields.Enum<SkillParamSpawnKind>(
                    "kind", spawn.kind,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].kind = {value}",
                        document => SpawnAt(document, index).kind = value))));
                detail.Add(SkillInspectorFields.Int(
                    "areaId", spawn.areaId ?? 0, true,
                    () => SpawnAt(index)?.areaId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].areaId = {value}",
                        document => SpawnAt(document, index).areaId = value > 0 ? value : (int?)null))));
                if (spawn.areaId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Area, spawn.areaId ?? 0)}"));
                detail.Add(SkillInspectorFields.Int(
                    "bulletId", spawn.bulletId ?? 0, true,
                    () => SpawnAt(index)?.bulletId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].bulletId = {value}",
                        document => SpawnAt(document, index).bulletId = value > 0 ? value : (int?)null))));
                if (spawn.bulletId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Bullet, spawn.bulletId ?? 0)}"));
                detail.Add(SkillInspectorFields.Int(
                    "buffId", spawn.buffId ?? 0, true,
                    () => SpawnAt(index)?.buffId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].buffId = {value}",
                        document => SpawnAt(document, index).buffId = value > 0 ? value : (int?)null))));
                if (spawn.buffId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Buff, spawn.buffId ?? 0)}"));
                detail.Add(SkillInspectorFields.Int(
                    "untilMs", spawn.untilMs ?? -1, true,
                    () => SpawnAt(index)?.untilMs ?? -1,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"spawnEvents[{index}].untilMs = {value}",
                        document => SpawnAt(document, index).untilMs = value < 0 ? null : value))));
                detail.Add(SkillInspectorFields.Hint("-1 = 开放窗口"));
            }
        }

        private void BuildHitEventsBlock(VisualElement parent, SkillParamJson skill)
        {
            SkillEditorSelection selection = session.Selection;
            SkillInspectorListBlock block = new(
                "HitEvents",
                skill.hitEvents?.Length ?? 0,
                () => session.Execute(SkillEditorListCommands.AddHitEvent()),
                index => session.Execute(SkillEditorListCommands.RemoveAt("hitEvents", index)),
                index => session.Execute(SkillEditorListCommands.DuplicateAt("hitEvents", index)));
            parent.Add(block.Root);

            SkillHitEventJson[] hits = skill.hitEvents ?? Array.Empty<SkillHitEventJson>();
            for (int i = 0; i < hits.Length; i++)
            {
                int index = i;
                SkillHitEventJson hitEvent = hits[i];
                bool selected = selection.HitEventIndex == index;
                block.AddItemRow(
                    $"[{index}] p{hitEvent?.phase ?? 0} {hitEvent?.kind} ({hitEvent?.hitPolicy})",
                    index, selected,
                    () => SelectHitEvent(index));
                if (!selected) continue;

                VisualElement detail = new() { style = { paddingLeft = 12, marginBottom = 6 } };
                block.ItemsContainer.Add(detail);
                detail.Add(SkillInspectorFields.Int(
                    "phase", hitEvent.phase, true,
                    () => HitAt(index)?.phase ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].phase = {value}",
                        document => HitAt(document, index).phase = value))));
                detail.Add(SkillInspectorFields.Enum<SkillParamHitTrigger>(
                    "on", hitEvent.on,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].on = {value}",
                        document => HitAt(document, index).on = value))));
                detail.Add(SkillInspectorFields.Enum<SkillParamHitPolicy>(
                    "hitPolicy", hitEvent.hitPolicy,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].hitPolicy = {value}",
                        document => HitAt(document, index).hitPolicy = value))));
                detail.Add(SkillInspectorFields.Enum<SkillParamHitEventKind>(
                    "kind", hitEvent.kind,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].kind = {value}",
                        document => HitAt(document, index).kind = value))));
                detail.Add(SkillInspectorFields.Int(
                    "nextPhase", hitEvent.nextPhase ?? -1, true,
                    () => HitAt(index)?.nextPhase ?? -1,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].nextPhase = {value}",
                        document => HitAt(document, index).nextPhase = value < 0 ? null : value))));
                detail.Add(SkillInspectorFields.Int(
                    "nextSkillId", hitEvent.nextSkillId ?? 0, true,
                    () => HitAt(index)?.nextSkillId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].nextSkillId = {value}",
                        document => HitAt(document, index).nextSkillId = value > 0 ? value : (int?)null))));
                if (hitEvent.nextSkillId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Skill, hitEvent.nextSkillId ?? 0)}"));
                detail.Add(SkillInspectorFields.Int(
                    "buffId", hitEvent.buffId ?? 0, true,
                    () => HitAt(index)?.buffId ?? 0,
                    value => session.Execute(new SkillEditorDelegateCommand(
                        $"hitEvents[{index}].buffId = {value}",
                        document => HitAt(document, index).buffId = value > 0 ? value : (int?)null))));
                if (hitEvent.buffId > 0)
                    detail.Add(SkillInspectorFields.Hint($"-> {ContentName(SkillEditorAssetKind.Buff, hitEvent.buffId ?? 0)}"));
            }
        }

        // ---- selection helpers ----

        private void SelectPhase(int index)
        {
            SkillEditorSelection selection = session.Selection;
            bool same = selection.PhaseIndex == index && selection.SpawnEventIndex is -3 or -1;
            selection.Clear();
            if (!same)
            {
                selection.PhaseIndex = index;
                selection.SpawnEventIndex = -3;   // -3 = 选中 phase 列表项
            }
            MarkViewsDirty();
        }

        private void SelectReaction(int index)
        {
            SkillEditorSelection selection = session.Selection;
            bool same = selection.PhaseIndex == index && selection.SpawnEventIndex == -2;
            selection.Clear();
            if (!same)
            {
                selection.PhaseIndex = index;
                selection.SpawnEventIndex = -2;   // -2 = 选中 reaction 列表项
            }
            MarkViewsDirty();
        }

        private void SelectManualBox(int index)
        {
            SkillEditorSelection selection = session.Selection;
            bool same = selection.ManualBoxIndex == index;
            selection.Clear();
            if (!same) selection.ManualBoxIndex = index;
            MarkViewsDirty();
        }

        private void SelectSpawnEvent(int index)
        {
            SkillEditorSelection selection = session.Selection;
            bool same = selection.SpawnEventIndex == index && index >= 0;
            selection.Clear();
            if (!same) selection.SpawnEventIndex = index;
            MarkViewsDirty();
        }

        private void SelectHitEvent(int index)
        {
            SkillEditorSelection selection = session.Selection;
            bool same = selection.HitEventIndex == index;
            selection.Clear();
            if (!same) selection.HitEventIndex = index;
            MarkViewsDirty();
        }

        private SkillPhaseJson PhaseAt(int index) => PhaseAt(session.Document, index);

        private static SkillPhaseJson PhaseAt(SkillEditorDocument document, int index)
        {
            SkillPhaseJson[] phases = document?.Skill?.phases;
            return index >= 0 && phases != null && index < phases.Length ? phases[index] : null;
        }

        private SkillHitReactionJson ReactionAt(int index)
        {
            SkillHitReactionJson[] items = session.Document?.Skill?.hitReactions;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private static SkillHitReactionJson ReactionAt(SkillEditorDocument document, int index)
        {
            SkillHitReactionJson[] items = document?.Skill?.hitReactions;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private SkillManualBoxJson BoxAt(int index)
        {
            SkillManualBoxJson[] items = session.Document?.Skill?.manualBoxes;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private static SkillManualBoxJson BoxAt(SkillEditorDocument document, int index)
        {
            SkillManualBoxJson[] items = document?.Skill?.manualBoxes;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private SkillSpawnEventJson SpawnAt(int index)
        {
            SkillSpawnEventJson[] items = session.Document?.Skill?.spawnEvents;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private static SkillSpawnEventJson SpawnAt(SkillEditorDocument document, int index)
        {
            SkillSpawnEventJson[] items = document?.Skill?.spawnEvents;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private SkillHitEventJson HitAt(int index)
        {
            SkillHitEventJson[] items = session.Document?.Skill?.hitEvents;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private static SkillHitEventJson HitAt(SkillEditorDocument document, int index)
        {
            SkillHitEventJson[] items = document?.Skill?.hitEvents;
            return index >= 0 && items != null && index < items.Length ? items[index] : null;
        }

        private string ContentName(SkillEditorAssetKind kind, int id)
        {
            if (id <= 0) return "未设置";
            string name = ContentIds.GetName(MapKind(kind), id);
            return name != null ? $"{id} - {name}" : $"{id} (ContentIds 未加载)";
        }

        private static ContentIdKind MapKind(SkillEditorAssetKind kind) => kind switch
        {
            SkillEditorAssetKind.Skill => ContentIdKind.Skill,
            SkillEditorAssetKind.Bullet => ContentIdKind.Bullet,
            SkillEditorAssetKind.Area => ContentIdKind.Area,
            SkillEditorAssetKind.Buff => ContentIdKind.Buff,
            SkillEditorAssetKind.Action => ContentIdKind.Action,
            _ => ContentIdKind.Skill,
        };
        private void RebuildIssues()
        {
            issuesScroll.Clear();
            if (session.Document == null)
            {
                issuesScroll.Add(new Label("校验：未打开文档"));
                return;
            }

            int errors = 0;
            int warnings = 0;
            foreach (SkillEditorIssue issue in session.Issues)
            {
                if (issue.Severity == SkillEditorIssueSeverity.Error) errors++;
                else if (issue.Severity == SkillEditorIssueSeverity.Warning) warnings++;
                Label label = new Label($"{issue.Severity}: {issue.Message}")
                {
                    style =
                    {
                        color = issue.Severity == SkillEditorIssueSeverity.Error
                            ? IssueErrorColor
                            : IssueWarnColor,
                        whiteSpace = WhiteSpace.Normal,
                    },
                };
                issuesScroll.Add(label);
            }

            if (errors == 0 && warnings == 0)
                issuesScroll.Add(new Label("校验通过") { style = { color = OkColor } });
            else
                issuesScroll.Add(new Label($"校验：{errors} 错误 / {warnings} 警告"));
        }
    }
}
