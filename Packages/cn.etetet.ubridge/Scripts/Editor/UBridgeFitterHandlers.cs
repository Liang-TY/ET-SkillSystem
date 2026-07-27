using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public static class UBridgeFitterGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<FitterGetRequest>(p);
            var resp = FitterGetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (!fitter) { resp.Error = 3; resp.Message = "No ContentSizeFitter component found"; return UBridgeJsonHelper.ToJson(resp); }

            resp.HorizontalFit = (int)fitter.horizontalFit;
            resp.VerticalFit = (int)fitter.verticalFit;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeFitterSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<FitterSetRequest>(p);
            var resp = FitterSetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (!fitter) { resp.Error = 3; resp.Message = "No ContentSizeFitter component found"; return UBridgeJsonHelper.ToJson(resp); }

            fitter.horizontalFit = (ContentSizeFitter.FitMode)r.HorizontalFit;
            fitter.verticalFit = (ContentSizeFitter.FitMode)r.VerticalFit;
            resp.Message = $"ContentSizeFitter set: hFit={r.HorizontalFit}, vFit={r.VerticalFit}";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
