using UnityEditor;
using UnityEngine;

namespace ET
{
    static class TF
    {
        public static Transform Find(int id, string path)
        {
            if (id != 0) { var go = EditorUtility.InstanceIDToObject(id) as GameObject; if (go) return go.transform; }
            if (!string.IsNullOrEmpty(path)) { var go = GameObject.Find(path); if (go) return go.transform; }
            return null;
        }
        public static BridgeTransformInfo Info(Transform t)
        {
            return UBridgeSceneGetHierarchyHandler.BuildTransform(t);
        }
    }

    public static class UBridgeTransformGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformGetRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, r?.Path);
            var resp = TransformGetResponse.Create();
            if (t) resp.Transform = TF.Info(t);
            else { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformSetPositionHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformSetPositionRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformSetPositionResponse.Create();
            if (t && r?.Position != null)
            {
                var pos = new Vector3(r.Position.X, r.Position.Y, r.Position.Z);
                if (r?.Local == true) t.localPosition = pos; else t.position = pos;
                resp.Transform = TF.Info(t);
            }
            else if (!t) { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformSetRotationHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformSetRotationRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformSetRotationResponse.Create();
            if (t && r?.Rotation != null)
            {
                var rot = new Quaternion(r.Rotation.X, r.Rotation.Y, r.Rotation.Z, r.Rotation.W);
                if (r?.Local == true) t.localRotation = rot; else t.rotation = rot;
                resp.Transform = TF.Info(t);
            }
            else if (!t) { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformSetScaleHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformSetScaleRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformSetScaleResponse.Create();
            if (t && r?.Scale != null) { t.localScale = new Vector3(r.Scale.X, r.Scale.Y, r.Scale.Z); resp.Transform = TF.Info(t); }
            else if (!t) { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformSetParentHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformSetParentRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformSetParentResponse.Create();
            if (t)
            {
                Transform parent = r?.ParentInstanceId > 0 ? TF.Find(r.ParentInstanceId, null) : null;
                t.SetParent(parent, r?.WorldPositionStays ?? true);
                resp.Transform = TF.Info(t);
            }
            else { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformSetSiblingIndexHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformSetSiblingIndexRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformSetSiblingIndexResponse.Create();
            if (t) { t.SetSiblingIndex(r?.SiblingIndex ?? 0); resp.Transform = TF.Info(t); }
            else { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformLookAtHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformLookAtRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformLookAtResponse.Create();
            if (t && r?.Target != null)
            {
                var up = r?.WorldUp != null ? new Vector3(r.WorldUp.X, r.WorldUp.Y, r.WorldUp.Z) : Vector3.up;
                t.LookAt(new Vector3(r.Target.X, r.Target.Y, r.Target.Z), up);
                resp.Transform = TF.Info(t);
            }
            else if (!t) { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTransformResetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TransformResetRequest>(p);
            var t = TF.Find(r?.InstanceId ?? 0, null);
            var resp = TransformResetResponse.Create();
            if (t)
            {
                t.localPosition = Vector3.zero; t.localRotation = Quaternion.identity; t.localScale = Vector3.one;
                resp.Transform = TF.Info(t);
            }
            else { resp.Error = 3; resp.Message = "Transform not found"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}