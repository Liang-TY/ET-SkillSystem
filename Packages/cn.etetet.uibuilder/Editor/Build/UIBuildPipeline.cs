using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YIUIFramework;

namespace ET.UIBuilder
{
    /// <summary>
    /// 构建管线核心（方案 §4.2）：spec → 校验 → 内存构建树 → CDE 绑定 → 落盘 prefab → YIUI 代码生成。
    /// 事务性：全部成功才 SaveAsPrefabAsset；中途异常丢弃内存对象，磁盘零污染。
    /// 被 Menu（本步骤）、CliCommand 与 ubridge 薄壳（S5）共用。
    /// </summary>
    public static class UIBuildPipeline
    {
        /// <summary>YIUI 资源根约束（与 ubridge CreatePanel 一致）</summary>
        private const string YIUIResPath = "Assets/GameRes/YIUI";

        public static BuildResult Build(string specPath)
        {
            var result = new BuildResult { SpecPath = specPath };

            // ① 加载 + 校验
            (UISpec spec, SpecValidationResult lint) = SpecLoader.Load(specPath);
            if (!lint.Ok)
            {
                result.Errors.Add("spec 校验未通过:\n" + lint.Format());
                return result;
            }

            foreach (SpecError issue in lint.Issues)
            {
                if (issue.Severity == ESpecSeverity.Warning)
                    result.Warnings.Add($"{issue.Path}: {issue.Message}");
            }

            // ② prefab 输出路径
            string relativeSpecPath = ToRelative(specPath);
            string prefabPath = ResolvePrefabPath(spec, relativeSpecPath, result);
            if (prefabPath == null)
                return result;
            if (!prefabPath.Replace('\\', '/').Contains(YIUIResPath))
            {
                result.Errors.Add($"prefabPath 必须位于 {YIUIResPath} 之下（当前: {prefabPath}）——YIUI 资源加载约定");
                return result;
            }

            // ③ 内存构建（骨架 → 节点树 → CDE 绑定）
            GameObject root = null;
            try
            {
                root = PanelAssembler.CreateSkeleton(spec);
                NodeTreeBuilder.Build(root.transform, spec.Nodes);
                CDEBinder.Bind(root, spec, result);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"构建异常: {ex.Message}\n{ex.StackTrace}");
                if (root != null)
                    UnityEngine.Object.DestroyImmediate(root);
                return result;
            }

            if (result.Errors.Count > 0)
            {
                UnityEngine.Object.DestroyImmediate(root);
                return result;
            }

            // ④ 落盘（先建目录再存 prefab）
            string prefabDir = Path.GetDirectoryName(prefabPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(prefabDir))
                Directory.CreateDirectory(Path.Combine(SpecLoader.ProjectRoot, prefabDir));

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            result.PrefabPath = prefabPath;

            // ⑤ YIUI 代码生成（基于资产态 prefab）
            result.GeneratedFiles.AddRange(CodeGenTrigger.Run(prefabPath, spec.Panel.Pkg, result));

            result.Ok = result.Errors.Count == 0;
            return result;
        }

        private static string ResolvePrefabPath(UISpec spec, string relativeSpecPath, BuildResult result)
        {
            if (!string.IsNullOrEmpty(spec.Panel.PrefabPath))
                return spec.Panel.PrefabPath.Replace('\\', '/');

            if (string.IsNullOrEmpty(relativeSpecPath))
            {
                result.Errors.Add($"无法从 spec 路径推导 prefabPath（{spec.SourcePath}），请在 spec 里显式指定 panel.prefabPath");
                return null;
            }

            string dir = Path.GetDirectoryName(relativeSpecPath)?.Replace('\\', '/');
            return $"{dir}/Prefabs/{spec.Panel.Name}.prefab";
        }

        private static string ToRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string p = path.Replace('\\', '/');
            string root = SpecLoader.ProjectRoot.Replace('\\', '/') + "/";
            if (p.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return p.Substring(root.Length);
            return p;
        }
    }
}
