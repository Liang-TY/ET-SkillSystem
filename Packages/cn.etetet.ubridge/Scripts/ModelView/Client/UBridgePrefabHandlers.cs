using System.IO;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ET
{
    /// <summary>Prefab 命令共享工具</summary>
    static class PrefabHelper
    {
        public static GameObject FindGameObject(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return Selection.activeGameObject;
            var go = GameObject.Find(path);
            if (go) return go;
            foreach (var c in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!c || EditorUtility.IsPersistent(c) || !c.scene.IsValid()) continue;
                if (GetPath(c) == path) return c;
            }
            return null;
        }
        public static string GetPath(GameObject go)
        {
            string p = go.name;
            var t = go.transform.parent;
            while (t) { p = t.name + "/" + p; t = t.parent; }
            return p;
        }
        public static BridgeObjectInfo CreateObjectInfo(GameObject go, bool inclComponents = false)
        {
            var i = BridgeObjectInfo.Create();
            i.Name = go.name; i.Path = GetPath(go); i.InstanceId = go.GetInstanceID();
            i.ActiveSelf = go.activeSelf; i.ActiveInHierarchy = go.activeInHierarchy;
            i.Tag = go.tag; i.Layer = go.layer;
            if (inclComponents)
            {
                var comps = go.GetComponents<Component>();
                for (int ci = 0; ci < comps.Length; ci++)
                {
                    if (!comps[ci]) continue;
                    var ci2 = BridgeComponentInfo.Create();
                    ci2.Type = comps[ci].GetType().Name; ci2.Data = "";
                    i.Transform = UBridgeSceneGetHierarchyHandler.BuildTransform(go.transform); // placeholder, just for info
                }
            }
            else i.Transform = UBridgeSceneGetHierarchyHandler.BuildTransform(go.transform);
            return i;
        }
    }

    public static class UBridgePrefabInstantiateHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabInstantiateRequest>(p);
            var resp = PrefabInstantiateResponse.Create();
            resp.PrefabPath = r?.PrefabPath ?? "";
            if (string.IsNullOrWhiteSpace(resp.PrefabPath)) { resp.Error = 3; resp.Message = "PrefabPath required"; return UBridgeJsonHelper.ToJson(resp); }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(resp.PrefabPath);
            if (!prefab) { resp.Error = 3; resp.Message = "Prefab not found"; return UBridgeJsonHelper.ToJson(resp); }
            var inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (!inst) { resp.Error = 8; resp.Message = "Instantiate failed"; return UBridgeJsonHelper.ToJson(resp); }
            if (r?.Position != null) inst.transform.position = new Vector3(r.Position.X, r.Position.Y, r.Position.Z);
            Undo.RegisterCreatedObjectUndo(inst, "Instantiate " + prefab.name);
            Selection.activeGameObject = inst;
            resp.Instance = PrefabHelper.CreateObjectInfo(inst);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabSaveHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabSaveRequest>(p);
            var resp = PrefabSaveResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.SavePath)) { resp.Error = 3; resp.Message = "SavePath required"; return UBridgeJsonHelper.ToJson(resp); }
            var go = PrefabHelper.FindGameObject(r?.GameObjectPath);
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }
            string savePath = r.SavePath.EndsWith(".prefab") ? r.SavePath : r.SavePath + ".prefab";
            var saved = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            AssetDatabase.Refresh();
            resp.GameObjectName = go.name; resp.PrefabPath = savePath; resp.Saved = saved != null;
            if (saved)
            {
                var a = BridgeAssetInfo.Create();
                a.Path = savePath; a.Guid = AssetDatabase.AssetPathToGUID(savePath); a.Name = saved.name; a.Type = saved.GetType().Name;
                resp.Asset = a;
            }
            else { resp.Error = 8; resp.Message = "Save failed"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabApplyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabApplyRequest>(p);
            var resp = PrefabApplyResponse.Create();
            var go = PrefabHelper.FindGameObject(r?.GameObjectPath);
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }
            if (!PrefabUtility.IsPartOfPrefabInstance(go)) { resp.Error = 3; resp.Message = "Not a prefab instance"; return UBridgeJsonHelper.ToJson(resp); }
            resp.PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
            AssetDatabase.Refresh();
            resp.GameObjectName = go.name; resp.Applied = true;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabUnpackHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabUnpackRequest>(p);
            var resp = PrefabUnpackResponse.Create();
            var go = PrefabHelper.FindGameObject(r?.GameObjectPath);
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }
            if (!PrefabUtility.IsPartOfAnyPrefab(go)) { resp.Error = 3; resp.Message = "Not part of prefab"; return UBridgeJsonHelper.ToJson(resp); }
            var mode = (r?.Completely ?? false) ? PrefabUnpackMode.Completely : PrefabUnpackMode.OutermostRoot;
            PrefabUtility.UnpackPrefabInstance(go, mode, InteractionMode.AutomatedAction);
            resp.GameObjectName = go.name; resp.Unpacked = true; resp.Completely = r?.Completely ?? false;
            resp.Object = PrefabHelper.CreateObjectInfo(go);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabGetInfoHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabGetInfoRequest>(p);
            var resp = PrefabGetInfoResponse.Create();
            GameObject target = null;
            if (!string.IsNullOrWhiteSpace(r?.PrefabPath))
            {
                target = AssetDatabase.LoadAssetAtPath<GameObject>(r.PrefabPath);
                if (!target) { resp.Error = 3; resp.Message = "Prefab not found"; return UBridgeJsonHelper.ToJson(resp); }
            }
            else { target = PrefabHelper.FindGameObject(r?.GameObjectPath); if (!target) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); } }
            resp.Name = target.name;
            resp.IsPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
            resp.IsPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target);
            resp.PrefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);
            resp.PrefabType = PrefabUtility.GetPrefabAssetType(target).ToString();
            resp.PrefabStatus = PrefabUtility.GetPrefabInstanceStatus(target).ToString();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgePrefabGetHierarchyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<PrefabGetHierarchyRequest>(p);
            var resp = PrefabGetHierarchyResponse.Create();
            resp.PrefabPath = r?.PrefabPath ?? "";
            if (string.IsNullOrWhiteSpace(resp.PrefabPath)) { resp.Error = 3; resp.Message = "PrefabPath required"; return UBridgeJsonHelper.ToJson(resp); }
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(resp.PrefabPath);
            if (!root) { resp.Error = 3; resp.Message = "Prefab not found"; return UBridgeJsonHelper.ToJson(resp); }
            int depth = (r?.Depth ?? 0) > 0 ? r.Depth : 5;
            bool truncated = false;
            bool inclInactive = r?.IncludeInactive ?? false;
            bool inclComps = r?.IncludeComponents ?? false;
            if (inclInactive || root.activeSelf) resp.Roots.Add(BuildNode(root, root.name, depth, inclInactive, inclComps, ref truncated));
            resp.PrefabName = root.name; resp.RootCount = resp.Roots.Count; resp.Truncated = truncated;
            return UBridgeJsonHelper.ToJson(resp);
        }
        static BridgeSceneNode BuildNode(GameObject go, string path, int depth, bool inclInactive, bool inclComps, ref bool truncated)
        {
            var node = BridgeSceneNode.Create();
            var info = BridgeObjectInfo.Create();
            info.Name = go.name; info.Path = path; info.InstanceId = go.GetInstanceID();
            info.ActiveSelf = go.activeSelf; info.ActiveInHierarchy = go.activeInHierarchy;
            info.Tag = go.tag; info.Layer = go.layer;
            info.Transform = UBridgeSceneGetHierarchyHandler.BuildTransform(go.transform);
            if (inclComps)
            {
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (!comps[i]) continue;
                    var ci = BridgeComponentInfo.Create();
                    ci.Type = comps[i].GetType().Name; ci.Data = "";
                }
            }
            node.Object = info;
            if (depth <= 0) { truncated |= go.transform.childCount > 0; return node; }
            foreach (Transform child in go.transform)
            {
                if (!inclInactive && !child.gameObject.activeSelf) continue;
                node.Children.Add(BuildNode(child.gameObject, path + "/" + child.gameObject.name, depth - 1, inclInactive, inclComps, ref truncated));
            }
            return node;
        }
    }
}