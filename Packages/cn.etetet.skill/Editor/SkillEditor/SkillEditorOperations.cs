using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Editor
{
    /// <summary>
    /// 无 UI 的应用层门面（02 §3 / 03 §2）：UI、CLI、Batchmode 三入口共用。
    /// CLI/Batchmode 每次创建短生命周期上下文，不在命令间保留可变 DTO。
    /// Step 1 先开放只读 list/get/validate；patch/save/open/preview/regression
    /// 按 03 §8 顺序随 Step 2-4 增加。
    /// </summary>
    /// <summary>internal：门面只在本程序集内使用，避免 public 成员暴露 internal 类型（CS0050/0051）。</summary>
    internal static class SkillEditorOperations
    {
        public static bool TryParseKind(string value, out SkillEditorAssetKind kind)
        {
            kind = default;
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Enum.TryParse(value, true, out kind) && Enum.IsDefined(typeof(SkillEditorAssetKind), kind);
        }

        public static List<SkillEditorAsset> List(
            SkillEditorAssetKind kind,
            string query,
            bool includeInvalid,
            SkillEditorDocumentStore store = null)
        {
            store ??= new SkillEditorDocumentStore();
            List<SkillEditorAsset> result = new();
            foreach (SkillEditorAsset asset in store.Scan(kind))
            {
                if (!includeInvalid && asset.Error != null) continue;
                if (!MatchQuery(asset, query)) continue;
                result.Add(asset);
            }
            return result;
        }

        private static bool MatchQuery(SkillEditorAsset asset, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            return asset.Id.ToString().Contains(query)
                || (asset.Name != null && asset.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                || (asset.Path != null && asset.Path.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public static SkillEditorGetResult Get(SkillEditorAssetKind kind, int id)
        {
            SkillEditorGetResult result = new() { Kind = kind.ToString(), Id = id };
            SkillEditorDocumentStore store = new();
            SkillEditorAsset asset = store.Find(kind, id);
            if (asset == null)
            {
                result.Ok = false;
                result.Code = "not_found";
                result.Error = $"not_found: {kind} id={id}";
                return result;
            }
            try
            {
                result.Name = asset.Name;
                result.Path = asset.Path;
                result.Json = File.ReadAllText(asset.Path);
                string hash = SkillEditorDocumentStore.ComputeSha256(asset.Path);
                result.Hash = hash == null ? null : $"sha256:{hash}";
            }
            catch (Exception e)
            {
                result.Ok = false;
                result.Code = "read_failed";
                result.Error = $"read_failed: {e.Message}";
            }
            return result;
        }

        /// <summary>id &gt; 0 校验单个资产；id &lt;= 0 校验该类型全部目录。</summary>
        public static SkillEditorValidateResult Validate(SkillEditorAssetKind kind, int id)
        {
            SkillEditorValidateResult result = new() { Kind = kind.ToString(), Id = id };
            SkillEditorDocumentStore store = new();
            List<SkillEditorAsset> targets = new();
            if (id > 0)
            {
                SkillEditorAsset asset = store.Find(kind, id);
                if (asset == null)
                {
                    result.Ok = false;
                    result.Code = "not_found";
                    result.Errors.Add($"not_found: {kind} id={id}");
                    result.ErrorCount = result.Errors.Count;
                    return result;
                }
                targets.Add(asset);
            }
            else
            {
                targets.AddRange(store.Scan(kind));
            }

            foreach (SkillEditorAsset asset in targets)
            {
                if (asset.Error != null)
                {
                    result.Errors.Add($"{asset.Path}: {asset.Error}");
                    continue;
                }
                if (!SkillEditorDocument.TryLoad(asset, out SkillEditorDocument document, out string loadError))
                {
                    result.Errors.Add($"{asset.Path}: {loadError}");
                    continue;
                }
                List<SkillEditorIssue> issues = new();
                SkillEditorValidation.ValidateDocument(document, store, issues);
                foreach (SkillEditorIssue issue in issues)
                {
                    if (issue.Severity == SkillEditorIssueSeverity.Error)
                        result.Errors.Add($"{asset.Path}: {issue.Message}");
                    else if (issue.Severity == SkillEditorIssueSeverity.Warning)
                        result.Warnings.Add($"{asset.Path}: {issue.Message}");
                }
            }

            result.ErrorCount = result.Errors.Count;
            result.WarningCount = result.Warnings.Count;
            result.Ok = result.ErrorCount == 0;
            return result;
        }
    }
}
