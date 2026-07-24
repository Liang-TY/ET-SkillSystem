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

    /// <summary>
    /// YIUICreateCommon: 创建 Common 类型预制体
    /// </summary>
    public static class UBridgeYIUICreateCommonHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUICreateCommonRequest>(p);
            var resp = YIUICreateCommonResponse.Create();
            string folderPath = r?.Path ?? "";
            string commonName = !string.IsNullOrWhiteSpace(r?.Name) ? r.Name : "YIUICommon";

            string resPath = "Assets/GameRes/YIUI";
            try { var cfg = Resources.Load<UnityEngine.Object>("YIUIConstAsset"); if (cfg) { var f = cfg.GetType().GetField("UIProjectResPath"); if (f != null) resPath = f.GetValue(cfg)?.ToString() ?? resPath; } } catch { }
            if (string.IsNullOrWhiteSpace(folderPath) || !folderPath.Contains(resPath))
            {
                resp.Error = 3; resp.Message = $"Path must be under {resPath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var savePath = $"{folderPath}/{commonName}.prefab";
            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(UnityEngine.Object)) != null)
            {
                resp.Error = 3; resp.Message = $"Already exists: {savePath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var go = new GameObject(commonName);
            go.GetOrAddComponent<RectTransform>();
            go.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = go.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.Common;
            go.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            PrefabUtility.SaveAsPrefabAsset(go, savePath);
            UnityEngine.Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            resp.PrefabPath = savePath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUICreateView: 创建 View 类型预制体
    /// </summary>
    public static class UBridgeYIUICreateViewHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUICreateViewRequest>(p);
            var resp = YIUICreateViewResponse.Create();
            string folderPath = r?.Path ?? "";
            string viewName = !string.IsNullOrWhiteSpace(r?.Name) ? r.Name : "YIUIView";

            string resPath = "Assets/GameRes/YIUI";
            try { var cfg = Resources.Load<UnityEngine.Object>("YIUIConstAsset"); if (cfg) { var f = cfg.GetType().GetField("UIProjectResPath"); if (f != null) resPath = f.GetValue(cfg)?.ToString() ?? resPath; } } catch { }
            if (string.IsNullOrWhiteSpace(folderPath) || !folderPath.Contains(resPath))
            {
                resp.Error = 3; resp.Message = $"Path must be under {resPath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var savePath = $"{folderPath}/{viewName}.prefab";
            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(UnityEngine.Object)) != null)
            {
                resp.Error = 3; resp.Message = $"Already exists: {savePath}";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var go = new GameObject(viewName);
            var rt = go.GetOrAddComponent<RectTransform>();
            rt.ResetToFullScreen();
            go.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = go.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.View;
            go.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            PrefabUtility.SaveAsPrefabAsset(go, savePath);
            UnityEngine.Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            resp.PrefabPath = savePath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUICreateAllView: 在 Panel prefab 根节点下创建 UIAllViewParent
    /// </summary>
    public static class UBridgeYIUICreateAllViewHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUICreateAllViewRequest>(p);
            var resp = YIUICreateAllViewResponse.Create();
            string path = r?.PrefabPath ?? "";
            if (string.IsNullOrWhiteSpace(path))
            {
                resp.Error = 3; resp.Message = "PrefabPath required";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            var cde = root?.GetComponent<UIBindCDETable>();
            if (!cde)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "CDE Table not found";
                return UBridgeJsonHelper.ToJson(resp);
            }

            if (cde.UICodeType != EUICodeType.Panel)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "Must be a Panel type prefab";
                return UBridgeJsonHelper.ToJson(resp);
            }

            // 反射获取 internal PanelSplitData
            var psdField = typeof(UIBindCDETable).GetField("PanelSplitData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var psd = psdField?.GetValue(cde) as UIPanelSplitData;
            if (psd == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "PanelSplitData not found";
                return UBridgeJsonHelper.ToJson(resp);
            }

            if (psd.AllViewParent != null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "AllView already exists";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var allViewObject = new GameObject("UIAllViewParent");
            var viewRect = allViewObject.GetOrAddComponent<RectTransform>();
            allViewObject.GetOrAddComponent<CanvasRenderer>();
            viewRect.SetParent(cde.transform, false);
            viewRect.ResetToFullScreen();
            allViewObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            psd.AllViewParent = viewRect;
            psd.AllCommonView.Clear();
            psd.AllCreateView.Clear();
            EditorUtility.SetDirty(cde);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.Refresh();

            resp.Message = "AllView created";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUICreateUIView: 在 Panel 的 AllViewParent 下当场创建 ViewParent + View GameObject
    /// 参考 MenuItemYIUIView.CreateYIUIViewByGameObject，View 是当场新建的，不引用外部 prefab
    /// </summary>
    public static class UBridgeYIUICreateUIViewHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUICreateUIViewRequest>(p);
            var resp = YIUICreateUIViewResponse.Create();
            string panelPath = r?.PrefabPath ?? "";

            if (string.IsNullOrWhiteSpace(panelPath))
            {
                resp.Error = 3; resp.Message = "PrefabPath required";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var root = PrefabUtility.LoadPrefabContents(panelPath);
            var cde = root?.GetComponent<UIBindCDETable>();
            if (!cde)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "CDE Table not found";
                return UBridgeJsonHelper.ToJson(resp);
            }

            if (cde.UICodeType != EUICodeType.Panel)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "Must be a Panel type prefab";
                return UBridgeJsonHelper.ToJson(resp);
            }

            var psdField = typeof(UIBindCDETable).GetField("PanelSplitData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var psd = psdField?.GetValue(cde) as UIPanelSplitData;
            if (psd == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "PanelSplitData not found";
                return UBridgeJsonHelper.ToJson(resp);
            }

            if (psd.AllViewParent == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                resp.Error = 3; resp.Message = "AllViewParent not found. Run YIUICreateAllView first.";
                return UBridgeJsonHelper.ToJson(resp);
            }

            // ViewParent（全屏容器）
            var viewParentObject = new GameObject("ViewParent");
            var viewParentRect = viewParentObject.GetOrAddComponent<RectTransform>();
            viewParentObject.GetOrAddComponent<CanvasRenderer>();
            viewParentRect.SetParent(psd.AllViewParent, false);
            viewParentRect.ResetToFullScreen();

            // View（带 CDE 表，运行时 UIBind 扫描 AllChildCdeTable 找到它）
            var viewObject = new GameObject("TestScrollView");
            var viewRect = viewObject.GetOrAddComponent<RectTransform>();
            viewObject.GetOrAddComponent<CanvasRenderer>();
            viewRect.SetParent(viewParentRect, false);
            viewRect.ResetToFullScreen();
            var viewCde = viewObject.GetOrAddComponent<UIBindCDETable>();
            viewCde.UICodeType = EUICodeType.View;

            viewParentObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            viewObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            // 注册到 AllCreateView
            psd.AllCreateView.Add(viewParentRect);
            EditorUtility.SetDirty(cde);

            PrefabUtility.SaveAsPrefabAsset(root, panelPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.Refresh();

            resp.Message = "UIView created (in-place)";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}