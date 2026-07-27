using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public static class UBridgeTextGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TextGetRequest>(p);
            var resp = TextGetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var txt = go.GetComponent<Text>();
            if (!txt) { resp.Error = 3; resp.Message = "No Text component found"; return UBridgeJsonHelper.ToJson(resp); }

            resp.Text = txt.text;
            resp.FontSize = txt.fontSize;
            resp.FontStyle = (int)txt.fontStyle;
            resp.Alignment = (int)txt.alignment;
            resp.ColorR = txt.color.r; resp.ColorG = txt.color.g; resp.ColorB = txt.color.b; resp.ColorA = txt.color.a;
            resp.BestFit = txt.resizeTextForBestFit;
            resp.RaycastTarget = txt.raycastTarget;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeTextSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TextSetRequest>(p);
            var resp = TextSetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var txt = go.GetComponent<Text>();
            if (!txt) { resp.Error = 3; resp.Message = "No Text component found"; return UBridgeJsonHelper.ToJson(resp); }

            if (r.Text != null) txt.text = r.Text;
            if (r.FontSize > 0) txt.fontSize = r.FontSize;
            txt.fontStyle = (FontStyle)r.FontStyle;
            txt.alignment = (TextAnchor)r.Alignment;
            txt.color = new Color((float)r.ColorR, (float)r.ColorG, (float)r.ColorB, (float)r.ColorA);
            txt.resizeTextForBestFit = r.BestFit;
            txt.raycastTarget = r.RaycastTarget;
            resp.Message = $"Text set: \"{txt.text}\" size={txt.fontSize} style={r.FontStyle} align={r.Alignment}";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
