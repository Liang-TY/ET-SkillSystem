using System.Collections.Generic;
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
