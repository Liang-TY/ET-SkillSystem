using System.Collections.Generic;

namespace ET.Editor
{
    /// <summary>CLI 结构化返回模型（03 §5）：单个 JSON对象、ok + 错误码，不返回自然语言日志。</summary>
    public sealed class SkillEditorListItem
    {
        public int Id;
        public string Name;
        public string Path;
        public string Error;
    }

    public sealed class SkillEditorListResult
    {
        public bool Ok = true;
        public string Command = "skill_editor_list";
        /// <summary>03 §5 错误码（invalid_request 等）；成功为 null。</summary>
        public string Code;
        public string Kind;
        public int Total;
        public readonly List<SkillEditorListItem> Items = new();
        public readonly List<string> Errors = new();
    }

    public sealed class SkillEditorGetResult
    {
        public bool Ok = true;
        public string Command = "skill_editor_get";
        /// <summary>03 §5 错误码（not_found / read_failed 等）；成功为 null。</summary>
        public string Code;
        public string Kind;
        public int Id;
        public string Name;
        public string Path;
        public string Json;
        public string Hash;
        public string Error;
    }

    public sealed class SkillEditorValidateResult
    {
        public bool Ok = true;
        public string Command = "skill_editor_validate";
        /// <summary>03 §5 错误码（not_found 等）；成功为 null。</summary>
        public string Code;
        public string Kind;
        public int Id;
        public int ErrorCount;
        public int WarningCount;
        public readonly List<string> Errors = new();
        public readonly List<string> Warnings = new();
    }
}
