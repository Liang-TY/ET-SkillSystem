using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public static class UBridgeElementGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<ElementGetRequest>(p);
            var resp = ElementGetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var el = go.GetComponent<LayoutElement>();
            if (!el) { resp.Error = 3; resp.Message = "No LayoutElement component found"; return UBridgeJsonHelper.ToJson(resp); }

            resp.MinWidth = el.minWidth;
            resp.MinHeight = el.minHeight;
            resp.PreferredWidth = el.preferredWidth;
            resp.PreferredHeight = el.preferredHeight;
            resp.FlexibleWidth = el.flexibleWidth;
            resp.FlexibleHeight = el.flexibleHeight;
            resp.IgnoreLayout = el.ignoreLayout;
            resp.LayoutPriority = el.layoutPriority;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeElementSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<ElementSetRequest>(p);
            var resp = ElementSetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var el = go.GetComponent<LayoutElement>();
            if (!el) { resp.Error = 3; resp.Message = "No LayoutElement component found"; return UBridgeJsonHelper.ToJson(resp); }

            el.minWidth = (float)r.MinWidth;
            el.minHeight = (float)r.MinHeight;
            el.preferredWidth = (float)r.PreferredWidth;
            el.preferredHeight = (float)r.PreferredHeight;
            el.flexibleWidth = (float)r.FlexibleWidth;
            el.flexibleHeight = (float)r.FlexibleHeight;
            el.ignoreLayout = r.IgnoreLayout;
            el.layoutPriority = r.LayoutPriority;
            resp.Message = $"LayoutElement set: min={r.MinWidth}x{r.MinHeight} pref={r.PreferredWidth}x{r.PreferredHeight} flex={r.FlexibleWidth}x{r.FlexibleHeight} ignore={r.IgnoreLayout} priority={r.LayoutPriority}";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
