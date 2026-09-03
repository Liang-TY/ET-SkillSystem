using System;
using System.Collections.Generic;

namespace ET.Editor
{
    /// <summary>
    /// 前后 JSON 快照式命令历史（02 §7 初版方案）。
    /// 粒度由调用方控制：文本输入失焦/提交一次，时间轴拖动 pointer-up 一次。
    /// </summary>
    internal sealed class SkillEditorHistory
    {
        internal sealed class Entry
        {
            public string Description;
            public string Before;
            public string After;
        }

        private const int MaxEntries = 200;

        private readonly List<Entry> undoStack = new();
        private readonly List<Entry> redoStack = new();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        /// <summary>前后快照相同则丢弃（no-op 命令不进历史）。</summary>
        public void Push(string description, string before, string after)
        {
            if (string.Equals(before, after, StringComparison.Ordinal)) return;
            undoStack.Add(new Entry { Description = description, Before = before, After = after });
            if (undoStack.Count > MaxEntries) undoStack.RemoveAt(0);
            redoStack.Clear();
        }

        public bool TryPopUndo(out Entry entry)
        {
            entry = null;
            if (undoStack.Count == 0) return false;
            entry = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            return true;
        }

        public bool TryPopRedo(out Entry entry)
        {
            entry = null;
            if (redoStack.Count == 0) return false;
            entry = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            return true;
        }

        public void OnUndo(Entry entry) => redoStack.Add(entry);

        public void OnRedo(Entry entry) => undoStack.Add(entry);

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}
