using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    static class GO
    {
        public static GameObject Find(int id, string path)
        {
            if (id != 0) return EditorUtility.InstanceIDToObject(id) as GameObject;
            if (!string.IsNullOrEmpty(path)) return GameObject.Find(path);
            return null;
        }
        public static BridgeObjectInfo Info(GameObject go)
        {
            var i = BridgeObjectInfo.Create();
            i.InstanceId = go.GetInstanceID(); i.Name = go.name; i.Tag = go.tag;
            i.Layer = go.layer; i.ActiveSelf = go.activeSelf; i.ActiveInHierarchy = go.activeInHierarchy;
            i.Transform = UBridgeSceneGetHierarchyHandler.BuildTransform(go.transform);
            return i;
        }
    }

    public static class UBridgeGameObjectCreateHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectCreateRequest>(p);
            var go = new GameObject(r?.Name ?? "GameObject");
            if (!string.IsNullOrEmpty(r?.Tag)) go.tag = r.Tag;
            if (r?.Layer > 0) go.layer = r.Layer;
            if (r?.Position != null) go.transform.position = new Vector3(r.Position.X, r.Position.Y, r.Position.Z);
            if (r?.Rotation != null) go.transform.rotation = new Quaternion(r.Rotation.X, r.Rotation.Y, r.Rotation.Z, r.Rotation.W);
            if (r?.Scale != null) go.transform.localScale = new Vector3(r.Scale.X, r.Scale.Y, r.Scale.Z);
            if (!string.IsNullOrEmpty(r?.ParentPath))
            {
                var parent = GameObject.Find(r.ParentPath);
                if (parent) go.transform.SetParent(parent.transform);
            }
            var resp = GameObjectCreateResponse.Create(); resp.Object = GO.Info(go);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectDestroyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectDestroyRequest>(p);
            var go = GO.Find(r?.InstanceId ?? 0, r?.Path);
            var resp = GameObjectDestroyResponse.Create();
            if (go) { UnityEngine.Object.DestroyImmediate(go); resp.Destroyed = true; }
            else { resp.Error = 3; resp.Message = "Object not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectFindHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectFindRequest>(p);
            var resp = GameObjectFindResponse.Create();
            int max = r?.MaxResults > 0 ? r.MaxResults : 20;
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (resp.Objects.Count >= max) break;
                if (!string.IsNullOrEmpty(r.Name) && !go.name.Contains(r.Name)) continue;
                if (!string.IsNullOrEmpty(r.Tag) && go.tag != r.Tag) continue;
                resp.Objects.Add(GO.Info(go));
            }
            resp.Count = resp.Objects.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectGetInfoHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectGetInfoRequest>(p);
            var go = GO.Find(r?.InstanceId ?? 0, r?.Path);
            var resp = GameObjectGetInfoResponse.Create();
            if (go) resp.Object = GO.Info(go);
            else { resp.Error = 3; resp.Message = "Object not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectRenameHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectRenameRequest>(p);
            var go = GO.Find(r?.InstanceId ?? 0, null);
            var resp = GameObjectRenameResponse.Create();
            if (go && !string.IsNullOrEmpty(r?.NewName)) { go.name = r.NewName; resp.Object = GO.Info(go); }
            else if (!go) { resp.Error = 3; resp.Message = "Object not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectDuplicateHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectDuplicateRequest>(p);
            var go = GO.Find(r?.InstanceId ?? 0, null);
            var resp = GameObjectDuplicateResponse.Create();
            if (go)
            {
                var dup = UnityEngine.Object.Instantiate(go, go.transform.parent);
                if (!string.IsNullOrEmpty(r?.NewName)) dup.name = r.NewName;
                resp.Object = GO.Info(dup);
            }
            else { resp.Error = 3; resp.Message = "Object not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameObjectSetActiveHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameObjectSetActiveRequest>(p);
            var go = GO.Find(r?.InstanceId ?? 0, null);
            var resp = GameObjectSetActiveResponse.Create();
            if (go) { go.SetActive(r?.Active ?? true); resp.Object = GO.Info(go); }
            else { resp.Error = 3; resp.Message = "Object not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}