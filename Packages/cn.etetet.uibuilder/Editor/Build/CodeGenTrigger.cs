using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YIUIFramework;

namespace ET.UIBuilder
{
    /// <summary>
    /// 触发 YIUI 代码生成（迁移自 ubridge UBridgeYIUIGenerateCodeHandler）：
    /// 反射调用 UICreateModule.CreatePackages(cde, true, false, pkg)。
    /// 必须传"资产态"prefab 的 CDE 表（AssetDatabase 加载，非 LoadPrefabContents），
    /// 因为 UICreateModule 会检查 IsPartOfPrefabAsset。
    /// 产出：YIUIGen 生成代码 + 首次生成时的可编辑 partial 模板（已存在则不覆盖）。
    /// </summary>
    public static class CodeGenTrigger
    {
        public static List<string> Run(string prefabPath, string pkg, BuildResult result)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                result.Errors.Add($"代码生成失败：prefab 未找到 {prefabPath}");
                return new List<string>();
            }

            UIBindCDETable cde = prefabAsset.GetComponent<UIBindCDETable>();
            if (cde == null)
            {
                result.Errors.Add("代码生成失败：prefab 根节点缺少 UIBindCDETable");
                return new List<string>();
            }

            var before = SnapshotCodeGenFiles();

            try
            {
                var moduleType = System.Reflection.Assembly.Load("ET.YIUIFramework.Editor")
                    ?.GetType("YIUIFramework.Editor.UICreateModule");
                var method = moduleType?.GetMethod("CreatePackages",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null)
                {
                    result.Errors.Add("代码生成失败：UICreateModule.CreatePackages 不存在（YIUI 版本变化？）");
                    return new List<string>();
                }

                method.Invoke(null, new object[] { cde, true, false, pkg });
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                result.Errors.Add($"代码生成异常: {ex.InnerException?.Message ?? ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"代码生成异常: {ex.Message}");
                return new List<string>();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 前后快照差分 = 本次生成/改动的文件
            var after = SnapshotCodeGenFiles();
            var changed = new List<string>();
            foreach (KeyValuePair<string, DateTime> kv in after)
            {
                if (!before.TryGetValue(kv.Key, out DateTime oldTime) || oldTime != kv.Value)
                    changed.Add(kv.Key);
            }

            return changed;
        }

        /// <summary>扫描所有包的 YIUI 代码生成目录（生成代码 + 可编辑 partial 所在处）</summary>
        private static Dictionary<string, DateTime> SnapshotCodeGenFiles()
        {
            var snapshot = new Dictionary<string, DateTime>();
            string packagesDir = Path.Combine(SpecLoader.ProjectRoot, "Packages");
            if (!Directory.Exists(packagesDir))
                return snapshot;

            string[] targets = { "YIUIGen", "YIUIComponent", "YIUISystem" };
            string[] layers = { "ModelView", "HotfixView" };

            foreach (string packageDir in Directory.GetDirectories(packagesDir))
            {
                string scriptsDir = Path.Combine(packageDir, "Scripts");
                if (!Directory.Exists(scriptsDir))
                    continue;

                foreach (string layer in layers)
                {
                    string clientDir = Path.Combine(scriptsDir, layer, "Client");
                    if (!Directory.Exists(clientDir))
                        continue;

                    foreach (string target in targets)
                    {
                        string targetDir = Path.Combine(clientDir, target);
                        if (!Directory.Exists(targetDir))
                            continue;

                        foreach (string file in Directory.GetFiles(targetDir, "*.cs", SearchOption.AllDirectories))
                        {
                            try
                            {
                                snapshot[file] = File.GetLastWriteTimeUtc(file);
                            }
                            catch (Exception)
                            {
                                // 文件被占用等情况忽略
                            }
                        }
                    }
                }
            }

            return snapshot;
        }
    }
}
