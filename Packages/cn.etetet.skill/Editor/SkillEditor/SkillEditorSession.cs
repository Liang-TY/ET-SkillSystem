using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>当前选中对象（时间轴/Inspector 共享）。-1 表示未选中。</summary>
    internal sealed class SkillEditorSelection
    {
        public int PhaseIndex = -1;
        public int SpawnEventIndex = -1;
        public int HitEventIndex = -1;
        public int ManualBoxIndex = -1;

        public void Clear()
        {
            PhaseIndex = -1;
            SpawnEventIndex = -1;
            HitEventIndex = -1;
            ManualBoxIndex = -1;
        }
    }

    /// <summary>预览状态（02 §2.2）：确定性的采样参数，不驱动任何 LSCast/LSWorld。</summary>
    internal sealed class SkillPreviewState
    {
        public int TimeMs;
        public bool Playing;
        public float Speed = 1f;
        public bool FacingLeft;
        /// <summary>当前动画为继承（animId=0 phase）时的实际起播全局时刻；-1=非继承。</summary>
        public int InheritedPhaseStartMs = -1;
    }

    /// <summary>
    /// 单文档编辑会话（02 §7）：编辑状态、预览状态与运行时状态分离。
    /// 校验全部走 SkillEditorValidation（无副作用）；全局 SkillParamLoader 只在
    /// 显式菜单/回归流程（SkillParamEditorLoader）中更新，本类不碰。
    /// </summary>
    internal sealed class SkillEditorSession : IDisposable
    {
        private readonly SkillEditorDocumentStore store = new();

        /// <summary>打开/最近保存时的磁盘哈希（03 §5 冲突检测基准）。</summary>
        private string diskHash;

        /// <summary>最近保存/打开时的文档快照（dirty 判定基准）。</summary>
        private string savedSnapshot;

        public SkillEditorDocument Document { get; private set; }
        public SkillEditorSelection Selection { get; } = new();
        public SkillPreviewState Preview { get; } = new();
        public SkillEditorHistory History { get; } = new();
        public List<SkillEditorIssue> Issues { get; } = new();

        public bool IsDirty
            => Document != null
                && !string.Equals(Document.CaptureSnapshot(), savedSnapshot, StringComparison.Ordinal);

        public bool HasErrors
        {
            get
            {
                foreach (SkillEditorIssue issue in Issues)
                {
                    if (issue.Severity == SkillEditorIssueSeverity.Error) return true;
                }
                return false;
            }
        }

        /// <summary>文档/选择/预览参数变化后触发；View 订阅并标记刷新，不在回调里直接改 UI 树。</summary>
        public event Action Changed;

        public bool TryOpen(SkillEditorAsset asset, bool discardDirty, out string error)
        {
            error = null;
            if (asset == null)
            {
                error = "资产为空";
                return false;
            }
            if (Document != null && IsDirty && !discardDirty)
            {
                error = "当前文档有未保存修改，先保存或确认丢弃";
                return false;
            }
            if (!SkillEditorDocument.TryLoad(asset, out SkillEditorDocument loaded, out error)) return false;

            Document = loaded;
            Selection.Clear();
            History.Clear();
            Preview.TimeMs = 0;
            Preview.Playing = false;
            store.InvalidateCatalog();   // 目录快照随打开重建，避免跨表引用/重复 id 用陈旧缓存
            diskHash = SkillEditorDocumentStore.ComputeSha256(asset.Path);
            savedSnapshot = Document.CaptureSnapshot();
            Validate();
            Changed?.Invoke();
            return true;
        }

        /// <summary>所有修改的唯一入口：捕获前后快照 → 历史校验 → 重校验 → 通知视图。</summary>
        public void Execute(ISkillEditorCommand command)
        {
            if (command == null || Document == null) return;
            string before = Document.CaptureSnapshot();
            command.Do(Document);
            History.Push(command.Description, before, Document.CaptureSnapshot());
            Validate();
            Changed?.Invoke();
        }

        public bool Undo()
        {
            if (Document == null || !History.TryPopUndo(out SkillEditorHistory.Entry entry)) return false;
            if (!Document.RestoreSnapshot(entry.Before, out string error))
            {
                Debug.LogWarning($"[SkillEditor] Undo 恢复快照失败: {error}");
                return false;
            }
            History.OnUndo(entry);
            Validate();
            Changed?.Invoke();
            return true;
        }

        public bool Redo()
        {
            if (Document == null || !History.TryPopRedo(out SkillEditorHistory.Entry entry)) return false;
            if (!Document.RestoreSnapshot(entry.After, out string error))
            {
                Debug.LogWarning($"[SkillEditor] Redo 恢复快照失败: {error}");
                return false;
            }
            History.OnRedo(entry);
            Validate();
            Changed?.Invoke();
            return true;
        }

        /// <summary>校验（Error 阻止）→ 外部变更冲突检查 → 原子落盘。</summary>
        public bool TrySave(out string error)
        {
            error = null;
            if (Document == null)
            {
                error = "没有打开的文档";
                return false;
            }
            Validate();
            foreach (SkillEditorIssue issue in Issues)
            {
                if (issue.Severity != SkillEditorIssueSeverity.Error) continue;
                error = $"校验未通过，阻止保存: {issue.Message}";
                return false;
            }

            string currentDiskHash = SkillEditorDocumentStore.ComputeSha256(Document.Asset.Path);
            if (!string.Equals(currentDiskHash, diskHash, StringComparison.OrdinalIgnoreCase))
            {
                error = "文件已被外部修改（磁盘哈希与打开时不一致），请先 Reload 处理冲突";
                return false;
            }

            if (!store.Save(Document, out error)) return false;

            diskHash = SkillEditorDocumentStore.ComputeSha256(Document.Asset.Path);
            savedSnapshot = Document.CaptureSnapshot();
            store.InvalidateCatalog();
            Changed?.Invoke();
            return true;
        }

        public bool TryReload(bool discardDirty, out string error)
        {
            error = null;
            if (Document == null)
            {
                error = "没有打开的文档";
                return false;
            }
            if (IsDirty && !discardDirty)
            {
                error = "存在未保存修改";
                return false;
            }
            if (!SkillEditorDocument.TryLoad(Document.Asset, out SkillEditorDocument reloaded, out error)) return false;

            Document = reloaded;
            Selection.Clear();
            History.Clear();
            store.InvalidateCatalog();
            diskHash = SkillEditorDocumentStore.ComputeSha256(Document.Asset.Path);
            savedSnapshot = Document.CaptureSnapshot();
            Validate();
            Changed?.Invoke();
            return true;
        }

        public void Validate() => SkillEditorValidation.ValidateDocument(Document, store, Issues);

        public void Dispose()
        {
            Document = null;
            History.Clear();
            Issues.Clear();
            Changed = null;
        }
    }
}
