using UnityEditor;
using UnityEngine;
using YIUIFramework;

namespace ET
{
    public static class UBridgeYIUICreatePanelHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUICreatePanelRequest>(p);
            var resp = YIUICreatePanelResponse.Create();
            string folderPath = r?.Path ?? "";
            string panelName = !string.IsNullOrWhiteSpace(r?.Name) ? r.Name : "YIUIPanel";

            string resPath = "Assets/GameRes/YIUI";
            try { var cfg = Resources.Load<UnityEngine.Object>("YIUIConstAsset"); if (cfg) { var f = cfg.GetType().GetField("UIProjectResPath"); if (f != null) resPath = f.GetValue(cfg)?.ToString() ?? resPath; } } catch { }
            if (string.IsNullOrWhiteSpace(folderPath) || !folderPath.Contains(resPath))
            {
                resp.Error = 3; resp.Message = $"Path must be under {resPath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var savePath = $"{folderPath}/{panelName}.prefab";
            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                resp.Error = 3; resp.Message = $"Already exists: {savePath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            // Create Panel GameObject
            var panel = new GameObject(panelName);
            panel.GetOrAddComponent<RectTransform>().ResetToFullScreen();
            panel.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = panel.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.Panel;
            cdeTable.PanelOption |= EPanelOption.TimeCache;
            panel.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            // UIBlockBG child
            var bg = new GameObject("UIBlockBG");
            bg.GetOrAddComponent<RectTransform>().ResetToFullScreen();
            bg.GetOrAddComponent<CanvasRenderer>();
            bg.GetOrAddComponent<UIBlock>();
            bg.transform.SetParent(panel.transform, false);

            PrefabUtility.SaveAsPrefabAsset(panel, savePath);
            UnityEngine.Object.DestroyImmediate(panel);
            AssetDatabase.Refresh();

            resp.PrefabPath = savePath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabLoadForEditHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabLoadForEditRequest>(p);
            var resp = PrefabLoadForEditResponse.Create();
            string path = r?.PrefabPath ?? "";
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                resp.Error = 3; resp.Message = "Prefab not found: " + path;
                return UBridgeJsonHelper.ToJson(resp);
            }
            var root = PrefabUtility.LoadPrefabContents(path);
            resp.RootInstanceId = root.GetInstanceID();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabSaveModifiedHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabSaveModifiedRequest>(p);
            var resp = PrefabSaveModifiedResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "Object not found"; return UBridgeJsonHelper.ToJson(resp); }
            string path = r?.PrefabPath ?? AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrWhiteSpace(path)) { resp.Error = 3; resp.Message = "PrefabPath required"; return UBridgeJsonHelper.ToJson(resp); }
            PrefabUtility.SaveAsPrefabAsset(go, path);
            PrefabUtility.UnloadPrefabContents(go);
            AssetDatabase.Refresh();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}