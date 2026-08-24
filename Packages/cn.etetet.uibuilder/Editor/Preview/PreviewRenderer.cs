using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ET.UIBuilder
{
    /// <summary>
    /// 预览渲染：prefab → PNG 截图（方案 §4.4）。
    /// 不依赖 GameView 状态：Additive 临时场景 + WorldSpace Canvas（与 YIUIRoot 同为 1920×1080 比例）
    /// + 正交相机渲染到 RenderTexture。
    /// 布局在编辑模式下不会自动刷新，必须 ForceRebuildLayoutImmediate（由深到浅）。
    /// 默认输出 Library/UIPreview/（不进 git、不被 Unity 导入）。
    /// </summary>
    public static class PreviewRenderer
    {
        private const float CameraDistance = 500f;

        public static string Capture(string prefabPath, int width = 1920, int height = 1080, string outPath = null)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
                throw new InvalidOperationException($"prefab 未找到: {prefabPath}");

            if (string.IsNullOrEmpty(outPath))
            {
                string dir = Path.Combine(SpecLoader.ProjectRoot, "Library", "UIPreview");
                Directory.CreateDirectory(dir);
                outPath = Path.Combine(dir, $"{prefabAsset.name}_{width}x{height}.png");
            }
            else
            {
                outPath = Path.IsPathRooted(outPath) ? outPath : Path.Combine(SpecLoader.ProjectRoot, outPath);
                string dir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            }

            Scene originalScene = SceneManager.GetActiveScene();
            Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(tempScene);

            try
            {
                // Canvas：WorldSpace，尺寸=像素尺寸，中心在世界原点，朝 +Z
                var canvasGo = new GameObject("PreviewCanvas");
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(width, height);

                // 相机：正交，位于 canvas 正前方，手动 Render
                var cameraGo = new GameObject("PreviewCamera");
                Camera camera = cameraGo.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = height / 2f;
                camera.nearClipPlane = 1f;
                camera.farClipPlane = CameraDistance * 2f;
                camera.transform.position = new Vector3(0f, 0f, -CameraDistance);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.13f, 0.13f, 0.15f, 1f);
                camera.enabled = false;

                // 实例化面板（stretch 根节点会铺满 canvas rect）
                // 注意：ET 子命名空间下裸写 Object 会解析到 ET.Object，必须全限定
                UnityEngine.Object instance = PrefabUtility.InstantiatePrefab(prefabAsset);
                var panelGo = (GameObject)instance;
                panelGo.transform.SetParent(canvasRect, false);

                // 编辑模式下布局不会自动跑：由深到浅强制重建所有 LayoutGroup，再刷 Canvas
                RebuildLayouts(canvasRect);

                // 渲染 → 读回 → PNG
                var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture previous = RenderTexture.active;
                try
                {
                    camera.targetTexture = rt;
                    camera.Render();

                    RenderTexture.active = rt;
                    var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();
                    File.WriteAllBytes(outPath, tex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tex);
                }
                finally
                {
                    camera.targetTexture = null;
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(rt);
                }

                return outPath;
            }
            finally
            {
                // 恢复：临时场景整体卸载，用户场景零污染
                SceneManager.SetActiveScene(originalScene);
                EditorSceneManager.CloseScene(tempScene, true);
            }
        }

        /// <summary>由深到浅强制重建布局（LayoutGroup 依赖子级先算完）</summary>
        private static void RebuildLayouts(RectTransform root)
        {
            List<LayoutGroup> groups = root.GetComponentsInChildren<LayoutGroup>(true).ToList();
            groups.Sort((a, b) => Depth(b.transform).CompareTo(Depth(a.transform)));
            foreach (LayoutGroup group in groups)
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>());

            Canvas.ForceUpdateCanvases();
        }

        private static int Depth(Transform t)
        {
            int depth = 0;
            while (t.parent != null)
            {
                depth++;
                t = t.parent;
            }

            return depth;
        }
    }
}
