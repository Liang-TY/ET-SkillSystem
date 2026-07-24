using UnityEditor;
using UnityEngine;

namespace ET
{
    static class RectHelper
    {
        public static RectTransform Get(int id)
        {
            var go = EditorUtility.InstanceIDToObject(id) as GameObject;
            return go?.GetComponent<RectTransform>();
        }
    }

    public static class UBridgeRectGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectGetRequest>(p);
            var resp = RectGetResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not a RectTransform"; return UBridgeJsonHelper.ToJson(resp); }
            resp.AnchorMinX = rt.anchorMin.x; resp.AnchorMinY = rt.anchorMin.y;
            resp.AnchorMaxX = rt.anchorMax.x; resp.AnchorMaxY = rt.anchorMax.y;
            resp.SizeDeltaX = rt.sizeDelta.x; resp.SizeDeltaY = rt.sizeDelta.y;
            resp.AnchoredPosX = rt.anchoredPosition.x; resp.AnchoredPosY = rt.anchoredPosition.y;
            resp.PivotX = rt.pivot.x; resp.PivotY = rt.pivot.y;
            var euler = rt.localRotation.eulerAngles;
            resp.LocalRotX = euler.x; resp.LocalRotY = euler.y; resp.LocalRotZ = euler.z;
            resp.LocalScaleX = rt.localScale.x; resp.LocalScaleY = rt.localScale.y; resp.LocalScaleZ = rt.localScale.z;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetAnchorHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetAnchorRequest>(p);
            var resp = RectSetAnchorResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.anchorMin = new Vector2(r.MinX, r.MinY); rt.anchorMax = new Vector2(r.MaxX, r.MaxY);
            resp.Message = $"Anchor min({r.MinX},{r.MinY}) max({r.MaxX},{r.MaxY})";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetSizeHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetSizeRequest>(p);
            var resp = RectSetSizeResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.sizeDelta = new Vector2(r.RectWidth, r.RectHeight);
            resp.Message = $"Set to {r.RectWidth}x{r.RectHeight}";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetPosHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetPosRequest>(p);
            var resp = RectSetPosResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.anchoredPosition = new Vector2(r.X, r.Y);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetPivotHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetPivotRequest>(p);
            var resp = RectSetPivotResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.pivot = new Vector2(r.X, r.Y);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetRotationHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetRotationRequest>(p);
            var resp = RectSetRotationResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.localRotation = Quaternion.Euler(r.X, r.Y, r.Z);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeRectSetScaleHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<RectSetScaleRequest>(p);
            var resp = RectSetScaleResponse.Create();
            var rt = RectHelper.Get(r?.InstanceId ?? 0);
            if (!rt) { resp.Error = 3; resp.Message = "Not found"; return UBridgeJsonHelper.ToJson(resp); }
            rt.localScale = new Vector3(r.X, r.Y, r.Z);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}