using Unity.Pipeline.Commands;

namespace ET.Editor
{
    /// <summary>
    /// AI/Unity CLI 命令（03 §3）。Step 1 只读三命令；patch/save/reload/open/
    /// preview/regression 随 Step 2-4 增加。整数 id 寻址，名称只用于返回信息。
    /// </summary>
    public static class SkillEditorCliCommands
    {
        [CliCommand("skill_editor_list", "列出 SkillParams 资产（kind=Skill/Bullet/Area/Buff/Action）", Tags = new[] { "skill_editor" })]
        public static SkillEditorListResult List(
            [CliArg("kind", "资产类型", Required = true)] string kind,
            [CliArg("query", "按 id/名称/路径模糊搜索")] string query = null,
            [CliArg("includeInvalid", "包含解析失败的坏文件，默认 false")] bool includeInvalid = false)
        {
            SkillEditorListResult result = new();
            if (!SkillEditorOperations.TryParseKind(kind, out SkillEditorAssetKind parsed))
            {
                result.Ok = false;
                result.Errors.Add($"invalid_request: kind={kind}");
                return result;
            }
            result.Kind = parsed.ToString();
            foreach (SkillEditorAsset asset in SkillEditorOperations.List(parsed, query, includeInvalid))
            {
                result.Items.Add(new SkillEditorListItem
                {
                    Id = asset.Id,
                    Name = asset.Name,
                    Path = asset.Path,
                    Error = asset.Error,
                });
            }
            result.Total = result.Items.Count;
            return result;
        }

        [CliCommand("skill_editor_get", "读取一个资产：原文 JSON + sha256 文件哈希", Tags = new[] { "skill_editor" })]
        public static SkillEditorGetResult Get(
            [CliArg("kind", "资产类型", Required = true)] string kind,
            [CliArg("id", "整数 id", Required = true)] int id)
        {
            if (!SkillEditorOperations.TryParseKind(kind, out SkillEditorAssetKind parsed))
            {
                return new SkillEditorGetResult
                {
                    Ok = false,
                    Error = $"invalid_request: kind={kind}",
                };
            }
            return SkillEditorOperations.Get(parsed, id);
        }

        [CliCommand("skill_editor_validate", "校验一个资产或全目录（id<=0 为全量），结构 + 跨表引用", Tags = new[] { "skill_editor" })]
        public static SkillEditorValidateResult Validate(
            [CliArg("kind", "资产类型", Required = true)] string kind,
            [CliArg("id", "整数 id，0 表示该类型全部", Required = true)] int id)
        {
            if (!SkillEditorOperations.TryParseKind(kind, out SkillEditorAssetKind parsed))
            {
                SkillEditorValidateResult invalid = new() { Ok = false };
                invalid.Errors.Add($"invalid_request: kind={kind}");
                return invalid;
            }
            return SkillEditorOperations.Validate(parsed, id);
        }

        /// <summary>
        /// patch（03 §4）：请求文件传 --request（工程内相对路径），避免长 JSON 进命令行。
        /// 请求体含 operations/dryRun/expectedHash/save；默认 dryRun 不写盘。
        /// </summary>
        [CliCommand("skill_editor_patch", "按白名单 JSON Pointer 操作修改 Skill 参数：读取→应用→校验→可选保存", Tags = new[] { "skill_editor" })]
        public static SkillEditorPatchResult Patch(
            [CliArg("request", "patch 请求 JSON 文件（工程内相对路径）", Required = true)] string request)
        {
            string path = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(),
                request.Replace('\\', '/'));
            if (!System.IO.File.Exists(path))
            {
                return new SkillEditorPatchResult { Ok = false, Code = "invalid_request" };
            }
            SkillEditorPatchRequest parsed;
            try
            {
                parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<SkillEditorPatchRequest>(
                    System.IO.File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                return new SkillEditorPatchResult { Ok = false, Code = "invalid_request", Errors = { $"请求解析失败: {e.Message}" } };
            }
            if (parsed == null)
                return new SkillEditorPatchResult { Ok = false, Code = "invalid_request", Errors = { "请求为空" } };
            return SkillEditorOperations.Patch(parsed);
        }
    }
}
