using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET.UIBuilder
{
    /// <summary>
    /// 组件属性配置：按 type 把 props 应用到控件组件上（封闭集合，值已由 SpecLoader 校验）。
    /// 注意：loop_scroll 的 item 属性在此忽略——YIUI 体系下列表项由手写 System 经
    /// typeof(ItemComponent) 在运行时绑定，prefab 上无序列化槽位（item 仅用于 lint 与文档）。
    /// </summary>
    public static class PropConfigurator
    {
        /// <summary>spec 的 TextAnchor 式对齐名 → TMP 对齐枚举（TMP 枚举名不同：MiddleCenter→Center 等）</summary>
        private static readonly Dictionary<string, TextAlignmentOptions> TmpAlignmentMap =
            new Dictionary<string, TextAlignmentOptions>
            {
                { "UpperLeft", TextAlignmentOptions.TopLeft },
                { "UpperCenter", TextAlignmentOptions.Top },
                { "UpperRight", TextAlignmentOptions.TopRight },
                { "MiddleLeft", TextAlignmentOptions.Left },
                { "MiddleCenter", TextAlignmentOptions.Center },
                { "MiddleRight", TextAlignmentOptions.Right },
                { "LowerLeft", TextAlignmentOptions.BottomLeft },
                { "LowerCenter", TextAlignmentOptions.Bottom },
                { "LowerRight", TextAlignmentOptions.BottomRight },
            };

        public static void Configure(GameObject go, NodeSpec node)
        {
            switch (node.Type)
            {
                case "text":
                    ConfigureText(go.GetComponentInChildren<Text>(true), node.Props);
                    break;

                case "tmp":
                    ConfigureTmp(go.GetComponentInChildren<TextMeshProUGUI>(true), node.Props);
                    break;

                case "image":
                    ConfigureImage(go.GetComponent<Image>(), node.Props);
                    break;

                case "button":
                    ConfigureButton(go, node.Props);
                    break;

                case "input":
                    ConfigureInput(go.GetComponentInChildren<InputField>(true), node.Props);
                    break;

                case "toggle":
                    ConfigureToggle(go.GetComponentInChildren<Toggle>(true), node.Props);
                    break;

                case "slider":
                    ConfigureSlider(go.GetComponentInChildren<Slider>(true), node.Props);
                    break;

                case "dropdown":
                    ConfigureDropdown(go.GetComponentInChildren<Dropdown>(true), node.Props);
                    break;

                case "scroll_view":
                    ConfigureScroll(go.GetComponentInChildren<ScrollRect>(true), node.Props);
                    break;

                case "loop_scroll_v":
                case "loop_scroll_h":
                    ConfigureLoopScroll(go.GetComponent<LoopScrollRectBase>(), node.Props);
                    break;

                case "block":
                    UIBlock block = go.GetComponent<UIBlock>();
                    if (block != null && node.Props.ContainsKey("color"))
                        block.color = ParseColor(node.Props, "color", Color.white);
                    break;
            }
        }

        private static void ConfigureText(Text text, Dictionary<string, object> p)
        {
            if (text == null) return;
            text.text = GetStr(p, "text", "");
            text.fontSize = (int)GetNum(p, "fontsize", text.fontSize);
            text.color = ParseColor(p, "color", text.color);
            if (p.ContainsKey("alignment")) text.alignment = ParseEnum<TextAnchor>(GetStr(p, "alignment"), text.alignment);
            text.raycastTarget = GetBool(p, "raycast", text.raycastTarget);
            text.resizeToBestFit = GetBool(p, "bestfit", false);
        }

        private static void ConfigureTmp(TextMeshProUGUI text, Dictionary<string, object> p)
        {
            if (text == null) return;
            text.text = GetStr(p, "text", "");
            text.fontSize = GetNum(p, "fontsize", text.fontSize);
            text.color = ParseColor(p, "color", text.color);
            if (p.ContainsKey("alignment")
                && TmpAlignmentMap.TryGetValue(GetStr(p, "alignment", ""), out TextAlignmentOptions alignment))
                text.alignment = alignment;
        }

        private static void ConfigureImage(Image image, Dictionary<string, object> p)
        {
            if (image == null) return;
            image.color = ParseColor(p, "color", image.color);
            if (p.ContainsKey("imagetype"))
                image.type = ParseEnum<Image.Type>(GetStr(p, "imagetype"), image.type);
            image.raycastTarget = GetBool(p, "raycast", image.raycastTarget);
            image.preserveAspect = GetBool(p, "preserveaspect", image.preserveAspect);
            if (p.ContainsKey("fillamount")) image.fillAmount = GetNum(p, "fillamount", 1f);
            if (p.ContainsKey("fillmethod"))
                image.fillMethod = ParseEnum<Image.FillMethod>(GetStr(p, "fillmethod"), image.fillMethod);
        }

        private static void ConfigureButton(GameObject go, Dictionary<string, object> p)
        {
            Button button = go.GetComponent<Button>();
            if (button != null) button.interactable = GetBool(p, "interactable", true);

            Image image = go.GetComponent<Image>();
            if (image != null) image.color = ParseColor(p, "color", image.color);

            Text label = go.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = GetStr(p, "text", "");
                label.fontSize = (int)GetNum(p, "fontsize", label.fontSize);
            }
        }

        private static void ConfigureInput(InputField input, Dictionary<string, object> p)
        {
            if (input == null) return;
            input.text = GetStr(p, "text", "");
            if (p.ContainsKey("contenttype"))
                input.contentType = ParseEnum<InputField.ContentType>(GetStr(p, "contenttype"), input.contentType);
            if (p.ContainsKey("linetype"))
                input.lineType = ParseEnum<InputField.LineType>(GetStr(p, "linetype"), input.lineType);

            Text placeholder = input.placeholder as Text;
            if (placeholder != null && p.ContainsKey("placeholder"))
                placeholder.text = GetStr(p, "placeholder", "请输入...");
        }

        private static void ConfigureToggle(Toggle toggle, Dictionary<string, object> p)
        {
            if (toggle == null) return;
            toggle.isOn = GetBool(p, "ison", false);

            Text label = toggle.GetComponentInChildren<Text>(true);
            if (label != null && p.ContainsKey("label"))
                label.text = GetStr(p, "label", "Toggle");
        }

        private static void ConfigureSlider(Slider slider, Dictionary<string, object> p)
        {
            if (slider == null) return;
            slider.minValue = GetNum(p, "minvalue", 0f);
            slider.maxValue = GetNum(p, "maxvalue", 1f);
            slider.value = GetNum(p, "value", 0f);
        }

        private static void ConfigureDropdown(Dropdown dropdown, Dictionary<string, object> p)
        {
            if (dropdown == null) return;
            if (p.TryGetValue("options", out object optionsObj) && optionsObj is List<object> options)
            {
                dropdown.options.Clear();
                foreach (object option in options)
                    dropdown.options.Add(new Dropdown.OptionData(option?.ToString() ?? ""));
            }
        }

        private static void ConfigureScroll(ScrollRect scroll, Dictionary<string, object> p)
        {
            if (scroll == null) return;
            scroll.horizontal = GetBool(p, "horizontal", true);
            scroll.vertical = GetBool(p, "vertical", true);
            if (p.ContainsKey("movementtype"))
                scroll.movementType = ParseEnum<ScrollRect.MovementType>(GetStr(p, "movementtype"), scroll.movementType);
        }

        private static void ConfigureLoopScroll(LoopScrollRectBase scroll, Dictionary<string, object> p)
        {
            if (scroll == null) return;
            scroll.reverseDirection = GetBool(p, "reverse", false);
        }

        // ---------------- 属性取值 ----------------

        private static string GetStr(Dictionary<string, object> p, string key, string fallback)
        {
            return p.ContainsKey(key) ? p[key]?.ToString() ?? fallback : fallback;
        }

        private static float GetNum(Dictionary<string, object> p, string key, float fallback)
        {
            if (!p.TryGetValue(key, out object value) || value == null) return fallback;
            return (float)System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        private static bool GetBool(Dictionary<string, object> p, string key, bool fallback)
        {
            return p.TryGetValue(key, out object value) && value is bool b ? b : fallback;
        }

        private static Color ParseColor(Dictionary<string, object> p, string key, Color fallback)
        {
            if (!p.TryGetValue(key, out object value)) return fallback;
            return ColorUtility.TryParseHtmlString(value?.ToString(), out Color color) ? color : fallback;
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return System.Enum.TryParse(value, out T result) ? result : fallback;
        }
    }
}
