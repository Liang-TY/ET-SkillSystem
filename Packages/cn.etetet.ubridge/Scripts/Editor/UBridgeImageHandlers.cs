using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public static class UBridgeImageGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<ImageGetRequest>(p);
            var resp = ImageGetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var img = go.GetComponent<Image>();
            if (!img) { resp.Error = 3; resp.Message = "No Image component found"; return UBridgeJsonHelper.ToJson(resp); }

            resp.Sprite = img.sprite ? AssetDatabase.GetAssetPath(img.sprite) : "";
            resp.ColorR = img.color.r; resp.ColorG = img.color.g; resp.ColorB = img.color.b; resp.ColorA = img.color.a;
            resp.ImageType = (int)img.type;
            resp.FillAmount = img.fillAmount;
            resp.FillMethod = (int)img.fillMethod;
            resp.RaycastTarget = img.raycastTarget;
            resp.PreserveAspect = img.preserveAspect;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeImageSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<ImageSetRequest>(p);
            var resp = ImageSetResponse.Create();
            var go = EditorUtility.InstanceIDToObject(r?.InstanceId ?? 0) as GameObject;
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var img = go.GetComponent<Image>();
            if (!img) { resp.Error = 3; resp.Message = "No Image component found"; return UBridgeJsonHelper.ToJson(resp); }

            if (!string.IsNullOrEmpty(r.Sprite))
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(r.Sprite);
                if (sp) img.sprite = sp;
            }
            img.color = new Color((float)r.ColorR, (float)r.ColorG, (float)r.ColorB, (float)r.ColorA);
            img.type = (Image.Type)r.ImageType;
            img.fillAmount = (float)r.FillAmount;
            img.fillMethod = (Image.FillMethod)r.FillMethod;
            img.raycastTarget = r.RaycastTarget;
            img.preserveAspect = r.PreserveAspect;
            resp.Message = $"Image set: color=({r.ColorR},{r.ColorG},{r.ColorB},{r.ColorA}) type={r.ImageType} fill={r.FillAmount}";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
