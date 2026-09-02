using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>S2 预览：把选中 spec 构建成场景内 GameObject 树（不落盘、不生成代码），供人工验证。</summary>
        [MenuItem("Tools/YIUI Builder/Build Spec To Scene (选中 .ui.yaml)")]
        public static void BuildToScene()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".ui.yaml"))
            {
                Debug.LogWarning("[UIBuilder] 请先在 Project 窗口选中一个 .ui.yaml 文件");
                return;
            }

            (UISpec spec, SpecValidationResult result) = SpecLoader.Load(path);
            if (!result.Ok)
            {
                Debug.LogError($"[UIBuilder] spec 校验未通过，已取消构建:\n{result.Format()}");
                return;
            }

            GameObject root = PanelAssembler.CreateSkeleton(spec);
            NodeTreeBuilder.Build(root.transform, spec.Nodes);

            // 挂到场景 Canvas：没有则建预览 Canvas（与 YIUIRoot 同配置）
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("UIBuilderPreviewCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0f;
                canvasGo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGo, "UIBuilder PreviewCanvas");
            }

            root.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(root, $"UIBuilder BuildToScene {spec.Panel.Name}");
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);

            Debug.Log($"[UIBuilder] 已在场景构建 '{spec.Panel.Name}'" +
                      $"（{NodeTreeBuilder.CountNodes(spec.Nodes)} 个节点，S2 预览模式：不落盘不生成代码）");
        }

        /// <summary>S3：完整构建——prefab 落盘 + YIUIGen 代码生成（首次含可编辑 partial 模板）。</summary>
        [MenuItem("Tools/YIUI Builder/Build Spec To Prefab (选中 .ui.yaml)")]
        public static void BuildToPrefab()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".ui.yaml"))
            {
                Debug.LogWarning("[UIBuilder] 请先在 Project 窗口选中一个 .ui.yaml 文件");
                return;
            }

            BuildResult result = UIBuildPipeline.Build(path);
            if (result.Ok)
            {
                string files = result.GeneratedFiles.Count > 0
                    ? "\n  " + string.Join("\n  ", result.GeneratedFiles.ToArray())
                    : "\n  （无新文件，partial 已存在）";
                string preview = string.IsNullOrEmpty(result.PreviewPath)
                    ? "\n预览: 失败（见 warnings）"
                    : $"\n预览: {result.PreviewPath}";
                Debug.Log($"[UIBuilder] 构建成功: {result.PrefabPath}\n生成/更新文件:{files}{preview}\n" +
                          "提示：可编辑 partial（YIUIComponent/YIUISystem 下）已存在则未覆盖；新 .cs 触发编译，错误看 Console。");
                if (!string.IsNullOrEmpty(result.PreviewPath) && File.Exists(result.PreviewPath))
                    EditorUtility.RevealInFinder(result.PreviewPath);
            }
            else
            {
                Debug.LogError($"[UIBuilder] 构建失败:\n{string.Join("\n", result.Errors.ToArray())}");
            }
        }

        /// <summary>批量重建：cn.etetet.lockstep 的 YIUI 全部 .ui.yaml → prefab（字体/模板/映射变更后全量刷新，不生成预览截图）。</summary>
        [MenuItem("Tools/YIUI Builder/Build All LockStep Specs To Prefab")]
        public static void BuildAllLockStepToPrefab()
        {
            const string Dir = "Packages/cn.etetet.lockstep/Assets/GameRes/YIUI";
            string absDir = Path.Combine(SpecLoader.ProjectRoot, Dir);
            if (!Directory.Exists(absDir))
            {
                Debug.LogWarning($"[UIBuilder] 目录不存在: {absDir}");
                return;
            }

            string[] files = Directory.GetFiles(absDir, "*.ui.yaml", SearchOption.AllDirectories);
            int ok = 0, fail = 0;

            foreach (string file in files)
            {
                string abs = file.Replace('\\', '/');
                string root = SpecLoader.ProjectRoot.Replace('\\', '/') + "/";
                string rel = abs.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                    ? abs.Substring(root.Length) : abs;

                BuildResult r = UIBuildPipeline.Build(rel, runPreview: false);
                if (r.Ok)
                {
                    ok++;
                }
                else
                {
                    fail++;
                    Debug.LogError($"[UIBuilder] FAIL {rel} :: {string.Join("; ", r.Errors.ToArray())}");
                }
            }

            Debug.Log($"[UIBuilder] 批量构建完成: {files.Length} 个 spec，成功 {ok}，失败 {fail}");
        }

        /// <summary>S4：预览截图（选中 .prefab 直接截；选中 .ui.yaml 则截其对应 prefab，需先 Build）。</summary>
        [MenuItem("Tools/YIUI Builder/Preview Prefab (选中 .prefab 或 .ui.yaml)")]
        public static void PreviewPrefab()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[UIBuilder] 请先选中 .prefab 或 .ui.yaml");
                return;
            }

            string prefabPath;
            if (path.EndsWith(".prefab"))
            {
                prefabPath = path;
            }
            else if (path.EndsWith(".ui.yaml"))
            {
                string dir = Path.GetDirectoryName(path)?.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)); // 去掉 .ui
                prefabPath = $"{dir}/Prefabs/{name}.prefab";
                if (!File.Exists(Path.Combine(SpecLoader.ProjectRoot, prefabPath)))
                {
                    Debug.LogWarning($"[UIBuilder] 对应 prefab 不存在（先跑 Build Spec To Prefab）: {prefabPath}");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[UIBuilder] 请选中 .prefab 或 .ui.yaml");
                return;
            }

            try
            {
                string png = PreviewRenderer.Capture(prefabPath);
                Debug.Log($"[UIBuilder] 预览完成: {png}");
                EditorUtility.RevealInFinder(png);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIBuilder] 预览失败: {ex.Message}\n{ex.StackTrace}");
            }
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
