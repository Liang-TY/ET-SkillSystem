using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace ET
{
    /// <summary>
    /// AddControl: 创建标准 Unity UI 控件（Button/Text/Image/Toggle/InputField 等）
    /// 实现方式：UnityEngine.UI.DefaultControls.CreateXxx()
    /// 创建后自动按类型加前缀重命名（如 Button → Btn_xxx）
    /// </summary>
    public static class UBridgeAddControlHandler
    {
        [StaticField]
        private static readonly DefaultControls.Resources s_Resources = new DefaultControls.Resources();

        /// <summary>控件类型 → 命名前缀</summary>
        [StaticField]
        private static readonly Dictionary<string, string> s_TypePrefixes = new()
        {
            {"Button", "Btn"},
            {"Text", "Txt"},
            {"Image", "Img"},
            {"RawImage", "RawImg"},
            {"InputField", "Input"},
            {"Toggle", "Tog"},
            {"Slider", "Sld"},
            {"ScrollView", "Scroll"},
            {"Dropdown", "Drop"},
            {"Scrollbar", "Bar"},
            {"Panel", "Panel"},
        };

        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AddControlRequest>(p);
            var resp = AddControlResponse.Create();
            var parent = EditorUtility.InstanceIDToObject(r?.ParentId ?? 0) as GameObject;
            if (!parent)
            {
                resp.Error = 3; resp.Message = "Parent not found";
                return UBridgeJsonHelper.ToJson(resp);
            }

            string controlType = r?.Type ?? "";
            GameObject instance;
            switch (controlType)
            {
                case "Button":     instance = DefaultControls.CreateButton(s_Resources); break;
                case "Text":       instance = DefaultControls.CreateText(s_Resources); break;
                case "Image":      instance = DefaultControls.CreateImage(s_Resources); break;
                case "RawImage":   instance = DefaultControls.CreateRawImage(s_Resources); break;
                case "InputField": instance = DefaultControls.CreateInputField(s_Resources); break;
                case "Toggle":     instance = DefaultControls.CreateToggle(s_Resources); break;
                case "Slider":     instance = DefaultControls.CreateSlider(s_Resources); break;
                case "ScrollView": instance = DefaultControls.CreateScrollView(s_Resources); break;
                case "Dropdown":   instance = DefaultControls.CreateDropdown(s_Resources); break;
                case "Scrollbar":  instance = DefaultControls.CreateScrollbar(s_Resources); break;
                case "Panel":      instance = DefaultControls.CreatePanel(s_Resources); break;
                default:
                    resp.Error = 3; resp.Message = $"Unknown control type: {controlType}. Supported: Button, Text, Image, RawImage, InputField, Toggle, Slider, ScrollView, Dropdown, Scrollbar, Panel";
                    return UBridgeJsonHelper.ToJson(resp);
            }

            instance.transform.SetParent(parent.transform, false);

            // 自动加前缀重命名：如 --type Button --name Login → Btn_Login
            string userGivenName = r?.Name ?? "";
            if (!string.IsNullOrWhiteSpace(userGivenName))
            {
                if (s_TypePrefixes.TryGetValue(controlType, out var prefix))
                {
                    var parts = userGivenName.Split('_');
                    // 首段已是正确前缀 → 保留原名；否则加前缀
                    instance.name = (parts.Length > 0 && parts[0] == prefix)
                        ? userGivenName
                        : $"{prefix}_{userGivenName}";
                }
                else
                {
                    instance.name = userGivenName;
                }
            }
            EditorUtility.SetDirty(parent);

            resp.InstanceId = instance.GetInstanceID();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIAddControl: 通过克隆 YIUI 模板 prefab 创建控件
    /// TODO: 后续完善 — 克隆 TemplatePrefabs/YIUI/ 下的 .prefab 文件
    /// </summary>
    public static class UBridgeYIUIAddControlHandler
    {
        public static string Handle(string p)
        {
            var resp = YIUIAddControlResponse.Create();
            resp.Error = 3; resp.Message = "TODO: YIUIAddControl not yet implemented. Will clone from TemplatePrefabs/YIUI/*.prefab";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
