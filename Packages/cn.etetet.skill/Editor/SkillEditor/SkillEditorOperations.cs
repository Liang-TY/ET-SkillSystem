using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// patch（03 §4/§5）：读取 → 应用（白名单）→ 校验 → 可选保存 的原子流程。
        /// dryRun 校验的是修改后的内存结果；写盘需 save=true 且 expectedHash 匹配。
        /// 失败不部分保存：DTO 从磁盘重读丢弃半成品。
        /// </summary>
        public static SkillEditorPatchResult Patch(SkillEditorPatchRequest request)
        {
            SkillEditorPatchResult result = new()
            {
                Kind = request?.kind,
                Id = request?.id ?? 0,
            };
            if (request == null)
            {
                result.Ok = false;
                result.Code = "invalid_request";
                result.Errors.Add("请求为空");
                return result;
            }
            if (!TryParseKind(request.kind, out SkillEditorAssetKind kind))
            {
                result.Ok = false;
                result.Code = "invalid_request";
                result.Errors.Add($"kind 无效: {request.kind}");
                return result;
            }

            SkillEditorDocumentStore store = new();
            SkillEditorAsset asset = store.Find(kind, request.id);
            if (asset == null)
            {
                result.Ok = false;
                result.Code = "not_found";
                result.Errors.Add($"not_found: {kind} id={request.id}");
                return result;
            }
            result.Path = asset.Path;

            string diskHash = SkillEditorDocumentStore.ComputeSha256(asset.Path);
            if (!string.Equals(diskHash, request.expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                result.Ok = false;
                result.Code = "conflict";
                result.Errors.Add("expectedHash 与磁盘不一致（先 get 取 baseHash）");
                return result;
            }
            result.BaseHash = $"sha256:{diskHash}";

            if (asset.Error != null
                || !SkillEditorDocument.TryLoad(asset, out SkillEditorDocument document, out string loadError))
            {
                result.Ok = false;
                result.Code = "validation_error";
                result.Errors.Add($"磁盘资产不可加载: {asset.Error ?? loadError}");
                return result;
            }

            SkillParamJson skill = document.Skill;
            if (skill == null)
            {
                result.Ok = false;
                result.Code = "invalid_request";
                result.Errors.Add("patch 只支持 Skill（Bullet/Area/Buff/Action 在 Step 4）");
                return result;
            }

            result.Changed = SkillEditorPatchEngine.Apply(skill, request, out string applyError);
            if (!string.IsNullOrEmpty(applyError))
            {
                result.Ok = false;
                result.Code = "invalid_request";
                result.Errors.Add(applyError);
                return result;
            }

            List<SkillEditorIssue> issues = new();
            SkillEditorValidation.ValidateSkillDto(skill, asset.Path, store, issues);
            foreach (SkillEditorIssue issue in issues)
            {
                if (issue.Severity == SkillEditorIssueSeverity.Error)
                    result.Errors.Add(issue.Message);
                else
                    result.Warnings.Add(issue.Message);
            }
            if (result.Errors.Count > 0)
            {
                result.Ok = false;
                result.Code = "validation_error";
                return result;
            }

            result.ResultHash = $"sha256:{ComputeHash(skill)}";

            if (request.dryRun) return result;

            if (!request.save)
            {
                result.Ok = false;
                result.Code = "invalid_request";
                result.Errors.Add("非 dryRun 必须显式 save=true（默认 dryRun，不写盘）");
                return result;
            }

            if (document.Save(skill))
            {
                result.Saved = true;
                result.ResultHash = $"sha256:{SkillEditorDocumentStore.ComputeSha256(asset.Path)}";
                return result;
            }
            result.Ok = false;
            result.Code = "save_failed";
            result.Errors.Add("原子写入失败（原文件保留）");
            return result;
        }

        private static string ComputeHash(SkillParamJson skill)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(skill, Formatting.None));
            using System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
