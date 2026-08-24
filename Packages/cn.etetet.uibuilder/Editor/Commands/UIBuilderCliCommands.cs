using System;
using System.Collections.Generic;
using System.IO;
using Unity.Pipeline.Commands;

namespace ET.UIBuilder
{
    /// <summary>
    /// Unity CLI（com.unity.pipeline）命令注册（方案 §4.3，S5）。
    /// [CliCommand] 自动发现，无需注册步骤：`unity command` 列出、`unity command yiui_build_panel --spec ...` 执行。
    /// 编译验证走 pipeline 内建 recompile / recompile_status（从机 0003 单已验证），不再自研 compile_check。
    /// </summary>
    public static class UIBuilderCliCommands
    {
        public class BuildAllResult
        {
            public int Total;
            public int Failed;
            public readonly List<string> Results = new List<string>();
        }

        public class PreviewResult
        {
            public string Png;
        }

        public class TypesResult
        {
            public readonly List<TypeInfo> Types = new List<TypeInfo>();
        }

        public class TypeInfo
        {
            public string Name;
            public string Bind;
            public string DefaultSize;
            public readonly List<string> Props = new List<string>();
        }

        [CliCommand("yiui_build_panel", "按 .ui.yaml spec 构建 YIUI 面板：prefab + YIUIGen 代码生成 + 预览截图", Tags = new[] { "yiui" })]
        public static BuildResult BuildPanel(
            [CliArg("spec", "spec 文件路径（工程内相对路径）", Required = true)] string spec,
            [CliArg("preview", "是否生成预览截图，默认 true")] bool preview = true)
        {
            return UIBuildPipeline.Build(spec, preview);
        }

        [CliCommand("yiui_build_all", "批量构建目录下全部 .ui.yaml（量产用）", Tags = new[] { "yiui" })]
        public static BuildAllResult BuildAll(
            [CliArg("dir", "扫描目录（工程内相对路径，递归）", Required = true)] string dir,
            [CliArg("preview", "是否生成预览截图，默认 true")] bool preview = true)
        {
            var result = new BuildAllResult();
            string absDir = Path.Combine(SpecLoader.ProjectRoot, dir.Replace('\\', '/'));
            if (!Directory.Exists(absDir))
                throw new ArgumentException($"目录不存在: {absDir}");

            foreach (string file in Directory.GetFiles(absDir, "*.ui.yaml", SearchOption.AllDirectories))
            {
                BuildResult one = UIBuildPipeline.Build(ToRelative(file), preview);
                result.Total++;
                if (!one.Ok)
                {
                    result.Failed++;
                    result.Results.Add($"FAIL {ToRelative(file)} :: {string.Join("; ", one.Errors.ToArray())}");
                }
                else
                {
                    result.Results.Add($"OK {ToRelative(file)}");
                }
            }

            return result;
        }

        [CliCommand("yiui_preview_panel", "渲染 prefab 预览截图，返回 PNG 路径", Tags = new[] { "yiui" })]
        public static PreviewResult PreviewPanel(
            [CliArg("prefab", "prefab 路径（工程内相对路径）", Required = true)] string prefab,
            [CliArg("width", "宽度，默认 1920")] int width = 1920,
            [CliArg("height", "高度，默认 1080")] int height = 1080,
            [CliArg("out", "输出 PNG 路径（默认 Library/UIPreview/<名>_<宽>x<高>.png）")] string outPath = null)
        {
            string png = PreviewRenderer.Capture(prefab, width, height, outPath);
            return new PreviewResult { Png = png };
        }

        [CliCommand("yiui_list_types", "列出全部控件类型与 props 封闭集合（spec 编写参考，AI 自查用）", Tags = new[] { "yiui" })]
        public static TypesResult ListTypes()
        {
            var result = new TypesResult();
            foreach (KeyValuePair<string, List<SpecSchema.PropDef>> kv in SpecSchema.TypeProps)
            {
                var info = new TypeInfo { Name = kv.Key };
                if (SpecSchema.TypeBindComponents.TryGetValue(kv.Key, out string bind) && bind != null)
                    info.Bind = bind;
                else
                    info.Bind = "(须显式 bind.component)";
                info.DefaultSize = SpecSchema.DefaultSizes.TryGetValue(kv.Key, out float[] size)
                    ? $"{size[0]}x{size[1]}"
                    : "100x100";
                foreach (SpecSchema.PropDef def in kv.Value)
                    info.Props.Add($"{def.Name}({def.Kind}{(def.Required ? ",必填" : "")})");
                result.Types.Add(info);
            }

            return result;
        }

        private static string ToRelative(string absolute)
        {
            string p = absolute.Replace('\\', '/');
            string root = SpecLoader.ProjectRoot.Replace('\\', '/') + "/";
            return p.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? p.Substring(root.Length)
                : p;
        }
    }
}
