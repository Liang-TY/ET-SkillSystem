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

        private void OnDisable()
        {
            if (session == null) return;
            session.Changed -= MarkViewsDirty;
            session.Dispose();
            session = null;
        }

        /// <summary>session.Changed 可能在 UI 事件回调内触发，延迟一帧再改 UI 树。</summary>
        private void MarkViewsDirty() => viewsDirty = true;

        private void Update()
        {
            if (!viewsDirty) return;
            viewsDirty = false;
            RefreshDynamic();
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
            previewTitle = new Label("预览（Step 3 接入真实渲染）");
            previewArea.Add(previewTitle);
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

            TextField nameField = new TextField("name") { value = skill.name ?? string.Empty };
            BindDeferred(
                nameField,
                () => session.Document?.Skill?.name,
                value => session.Execute(new SkillEditorDelegateCommand(
                    $"修改 name → {value}", document => document.Skill.name = value)));
            parent.Add(nameField);

            SkillParamType parsedType = Enum.TryParse(skill.type, true, out SkillParamType type)
                ? type
                : SkillParamType.MeleeCombo;
            EnumField typeField = new EnumField("type", parsedType);
            typeField.RegisterValueChangedCallback(_ => session.Execute(new SkillEditorDelegateCommand(
                $"修改 type → {typeField.value}",
                document => document.Skill.type = typeField.value.ToString())));
            parent.Add(typeField);

            IntegerField cooldownField = new IntegerField("cooldownMs") { isDelayed = true, value = skill.cooldownMs };
            cooldownField.RegisterValueChangedCallback(_ => session.Execute(new SkillEditorDelegateCommand(
                $"修改 cooldownMs → {cooldownField.value}",
                document => document.Skill.cooldownMs = cooldownField.value)));
            parent.Add(cooldownField);

            IntegerField totalField = new IntegerField("totalTimeMs") { isDelayed = true, value = skill.totalTimeMs };
            totalField.RegisterValueChangedCallback(_ => session.Execute(new SkillEditorDelegateCommand(
                $"修改 totalTimeMs → {totalField.value}",
                document => document.Skill.totalTimeMs = totalField.value)));
            parent.Add(totalField);

            Toggle airborneToggle = new Toggle("requireAirborne") { value = skill.requireAirborne };
            airborneToggle.RegisterValueChangedCallback(_ => session.Execute(new SkillEditorDelegateCommand(
                $"修改 requireAirborne → {airborneToggle.value}",
                document => document.Skill.requireAirborne = airborneToggle.value)));
            parent.Add(airborneToggle);

            Toggle manualCooldownToggle = new Toggle("manualCooldown") { value = skill.manualCooldown };
            manualCooldownToggle.RegisterValueChangedCallback(_ => session.Execute(new SkillEditorDelegateCommand(
                $"修改 manualCooldown → {manualCooldownToggle.value}",
                document => document.Skill.manualCooldown = manualCooldownToggle.value)));
            parent.Add(manualCooldownToggle);

            Label hint = new Label("Phase / 手动盒 / 事件编辑在 Step 2 开放");
            hint.style.color = new Color(0.6f, 0.65f, 0.7f);
            parent.Add(hint);
        }

        /// <summary>文本输入在失焦时形成一次命令（02 §7：不为每个字符建历史）。</summary>
        private static void BindDeferred(TextField field, Func<string> getValue, Action<string> commit)
        {
            string original = getValue();
            field.RegisterCallback<FocusInEvent>(_ => original = getValue());
            field.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (!string.Equals(field.value, original, StringComparison.Ordinal))
                    commit(field.value);
                original = null;
            });
        }

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
