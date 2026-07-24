using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ET
{
    public static class UBridgeSelectionGetHandler
    {
        public static string Handle(string p)
        {
            var resp = SelectionGetResponse.Create();
            foreach (var go in Selection.gameObjects)
                resp.Objects.Add(FillObject(go));
            foreach (var obj in Selection.objects)
                if (AssetDatabase.Contains(obj) && !(obj is GameObject))
                    resp.Assets.Add(FillAsset(obj));
            resp.Count = resp.Objects.Count + resp.Assets.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
        static BridgeObjectInfo FillObject(GameObject go)
        {
            var i = BridgeObjectInfo.Create();
            i.InstanceId = go.GetInstanceID(); i.Name = go.name; i.Tag = go.tag;
            i.Layer = go.layer; i.ActiveSelf = go.activeSelf; i.ActiveInHierarchy = go.activeInHierarchy;
            i.Transform = UBridgeSceneGetHierarchyHandler.BuildTransform(go.transform);
            return i;
        }
        static BridgeAssetInfo FillAsset(UnityEngine.Object obj)
        {
            var a = BridgeAssetInfo.Create();
            a.Path = AssetDatabase.GetAssetPath(obj);
            a.Guid = AssetDatabase.AssetPathToGUID(a.Path);
            a.Name = obj.name; a.Type = obj.GetType().Name;
            return a;
        }
    }

    public static class UBridgeSelectionSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SelectionSetRequest>(p);
            var list = new System.Collections.Generic.List<UObject>();
            if (r?.InstanceIds != null)
                foreach (var id in r.InstanceIds) { var go = EditorUtility.InstanceIDToObject(id); if (go) list.Add(go); }
            if (r?.AssetPaths != null)
                foreach (var ap in r.AssetPaths) { var o = AssetDatabase.LoadAssetAtPath<UObject>(ap); if (o) list.Add(o); }
            Selection.objects = list.ToArray();
            var resp = SelectionSetResponse.Create(); resp.Count = list.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeSelectionAddHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SelectionAddRequest>(p);
            var list = Selection.objects.ToList();
            if (r?.InstanceIds != null)
                foreach (var id in r.InstanceIds) { var go = EditorUtility.InstanceIDToObject(id); if (go && !list.Contains(go)) list.Add(go); }
            if (r?.AssetPaths != null)
                foreach (var ap in r.AssetPaths) { var o = AssetDatabase.LoadAssetAtPath<UObject>(ap); if (o && !list.Contains(o)) list.Add(o); }
            Selection.objects = list.ToArray();
            var resp = SelectionAddResponse.Create(); resp.Count = list.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeSelectionRemoveHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<SelectionRemoveRequest>(p);
            var list = Selection.objects.ToList();
            if (r?.InstanceIds != null)
                foreach (var id in r.InstanceIds) list.RemoveAll(x => x.GetInstanceID() == id);
            if (r?.AssetPaths != null)
                foreach (var ap in r.AssetPaths) { var o = AssetDatabase.LoadAssetAtPath<UObject>(ap); if (o) list.Remove(o); }
            Selection.objects = list.ToArray();
            var resp = SelectionRemoveResponse.Create(); resp.Count = list.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeSelectionClearHandler
    {
        public static string Handle(string p) { Selection.objects = new UObject[0]; var resp = SelectionClearResponse.Create(); return UBridgeJsonHelper.ToJson(resp); }
    }
}