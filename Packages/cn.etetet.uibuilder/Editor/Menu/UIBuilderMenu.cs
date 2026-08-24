using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.UIBuilder
{
    /// <summary>
    /// S1 验证入口（菜单驱动）。S5 将增加 [CliCommand] 与 ubridge 薄壳，复用同一 SpecLoader。
    /// </summary>
    public static class UIBuilderMenu
    {
        [MenuItem("Tools/YIUI Builder/Validate Spec (选中 .ui.yaml)")]
        public static void ValidateSelected()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".ui.yaml"))
            {
                Debug.LogWarning("[UIBuilder] 请先在 Project 窗口选中一个 .ui.yaml 文件");
                return;
            }

            ReportOne(path);
        }

        [MenuItem("Tools/YIUI Builder/Validate All Specs")]
        public static void ValidateAll()
        {
            var paths = new List<string>();
            foreach (string p in AssetDatabase.GetAllAssetPaths())
            {
                if (p.EndsWith(".ui.yaml"))
                    paths.Add(p);
            }

            if (paths.Count == 0)
            {
                Debug.Log("[UIBuilder] 项目内没有 .ui.yaml");
                return;
            }

            int invalid = 0;
            foreach (string p in paths)
            {
                if (!ReportOne(p))
                    invalid++;
            }

            Debug.Log($"[UIBuilder] 全量校验完成: {paths.Count} 个 spec，{invalid} 个无效");
        }

        /// <summary>校验单个 spec 并输出；返回是否通过</summary>
        private static bool ReportOne(string path)
        {
            (UISpec _, SpecValidationResult result) = SpecLoader.Load(path);
            string text = result.Format();

            if (result.Ok)
            {
                Debug.Log(result.WarningCount > 0
                    ? $"[UIBuilder]\n{text}"
                    : $"[UIBuilder] {path} OK");
            }
            else
            {
                Debug.LogError($"[UIBuilder]\n{text}");
            }

            return result.Ok;
        }
    }
}
