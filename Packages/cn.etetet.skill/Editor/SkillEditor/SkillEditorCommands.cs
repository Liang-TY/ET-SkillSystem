using System;

namespace ET.Editor
{
    /// <summary>
    /// 所有编辑修改的唯一入口（02 §2.3：View 不直接散改 DTO）。
    /// Step 1 采用快照式 Undo：Session.Execute 捕获前后快照，Undo/Redo 由
    /// SkillEditorHistory 恢复，命令本身只实现 Do。后续 patch 类结构化命令
    /// （Step 2 CLI 白名单）同样走该接口。
    /// </summary>
    internal interface ISkillEditorCommand
    {
        string Description { get; }
        void Do(SkillEditorDocument document);
    }

    /// <summary>通用命令：修改逻辑以委托表达，可审计性由 Description + 前后快照保证。</summary>
    internal sealed class SkillEditorDelegateCommand : ISkillEditorCommand
    {
        private readonly string description;
        private readonly Action<SkillEditorDocument> apply;

        public SkillEditorDelegateCommand(string description, Action<SkillEditorDocument> apply)
        {
            this.description = description ?? "修改";
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        public string Description => description;

        public void Do(SkillEditorDocument document) => apply(document);
    }
}
