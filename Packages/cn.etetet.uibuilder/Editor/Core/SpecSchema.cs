using System.Collections.Generic;

namespace ET.UIBuilder
{
    /// <summary>
    /// spec 封闭集合定义（§3.3-3.6）。所有字段/属性名一律小写（加载时统一规范化）。
    /// 新增控件类型 = 在此注册属性表 + S2 的 ControlFactory 加构造函数。
    /// 注意：静态字段按声明顺序初始化，枚举数组必须排在 TypeProps 之前。
    /// </summary>
    public static class SpecSchema
    {
        public enum EPropKind
        {
            String,
            Number,
            Bool,
            Color,      // #RRGGBB 或 #RRGGBBAA
            Enum,       // EnumValues 给出合法值
            StringList,
        }

        public class PropDef
        {
            public string Name;
            public EPropKind Kind;
            public string[] EnumValues;
            public bool Required;
        }

        // ---- 枚举合法值（与 YIUI/Unity 枚举对齐）----

        public static readonly string[] NodeTypes =
        {
            "node", "text", "tmp", "image", "button", "input", "toggle", "slider",
            "dropdown", "scroll_view", "loop_scroll_v", "loop_scroll_h", "prefab", "block",
        };

        public static readonly string[] Anchors =
        {
            "center", "top", "bottom", "left", "right",
            "top_left", "top_right", "bottom_left", "bottom_right",
            "stretch", "full", "top_stretch", "bottom_stretch", "left_stretch", "right_stretch",
        };

        public static readonly string[] LayoutTypes = { "vertical", "horizontal", "grid" };
        public static readonly string[] Constraints = { "Flexible", "FixedColumnCount", "FixedRowCount" };
        public static readonly string[] Triggers = { "Click", "ClickDown", "ClickUp" };

        /// <summary>EUIEventParamType 全集（与框架枚举一致）</summary>
        public static readonly string[] ParamTypes =
        {
            "ParamVo", "Object", "Bool", "String", "Int", "Float", "Long",
            "Uint", "Ulong", "Double", "UnityGameObject",
        };

        /// <summary>EPanelLayer（Count/Any 为框架内部值，不可用）</summary>
        public static readonly string[] PanelLayers = { "Top", "Tips", "Popup", "Panel", "Scene", "Bottom" };

        /// <summary>EPanelStackOption</summary>
        public static readonly string[] StackOptions = { "None", "Visible", "VisibleTween", "Omit" };

        // props 枚举值
        private static readonly string[] TextAnchors =
        {
            "UpperLeft", "UpperCenter", "UpperRight",
            "MiddleLeft", "MiddleCenter", "MiddleRight",
            "LowerLeft", "LowerCenter", "LowerRight",
        };
        private static readonly string[] ImageTypes = { "Simple", "Sliced", "Filled", "Tiled" };
        private static readonly string[] FillMethods = { "Horizontal", "Vertical", "Radial90", "Radial180", "Radial360" };
        private static readonly string[] ContentTypes = { "Standard", "IntegerNumber", "DecimalNumber", "Alphanumeric", "Password" };
        private static readonly string[] LineTypes = { "SingleLine", "MultiLineNewline" };
        private static readonly string[] MovementTypes = { "Unrestricted", "Elastic", "Clamped" };

        // ---- 控件类型 → props 封闭集合 ----

        public static readonly Dictionary<string, List<PropDef>> TypeProps = new Dictionary<string, List<PropDef>>
        {
            { "node", Defs() },
            { "text", Defs(
                P("text"), P("fontsize", EPropKind.Number), P("color", EPropKind.Color),
                P("alignment", EPropKind.Enum, TextAnchors), P("raycast", EPropKind.Bool), P("bestfit", EPropKind.Bool)) },
            { "tmp", Defs(
                P("text"), P("fontsize", EPropKind.Number), P("color", EPropKind.Color),
                P("alignment", EPropKind.Enum, TextAnchors)) },
            { "image", Defs(
                P("color", EPropKind.Color), P("imagetype", EPropKind.Enum, ImageTypes), P("raycast", EPropKind.Bool),
                P("preserveaspect", EPropKind.Bool), P("fillamount", EPropKind.Number), P("fillmethod", EPropKind.Enum, FillMethods)) },
            { "button", Defs(
                P("text"), P("fontsize", EPropKind.Number), P("interactable", EPropKind.Bool), P("color", EPropKind.Color)) },
            { "input", Defs(
                P("text"), P("placeholder"), P("contenttype", EPropKind.Enum, ContentTypes), P("linetype", EPropKind.Enum, LineTypes)) },
            { "toggle", Defs(P("ison", EPropKind.Bool), P("label")) },
            { "slider", Defs(P("value", EPropKind.Number), P("minvalue", EPropKind.Number), P("maxvalue", EPropKind.Number)) },
            { "dropdown", Defs(P("options", EPropKind.StringList)) },
            { "scroll_view", Defs(
                P("horizontal", EPropKind.Bool), P("vertical", EPropKind.Bool),
                P("movementtype", EPropKind.Enum, MovementTypes)) },
            { "loop_scroll_v", Defs(
                P("item", EPropKind.String, null, true), P("reverse", EPropKind.Bool)) },
            { "loop_scroll_h", Defs(
                P("item", EPropKind.String, null, true), P("reverse", EPropKind.Bool)) },
            { "prefab", Defs(P("path", EPropKind.String, null, true)) },
            { "block", Defs(P("color", EPropKind.Color)) },
        };

        /// <summary>类型 → 默认绑定组件（C 表注册用）。null = 无默认，必须显式 bind.component</summary>
        public static readonly Dictionary<string, string> TypeBindComponents = new Dictionary<string, string>
        {
            { "node", "RectTransform" },
            { "text", "Text" },
            { "tmp", "TextMeshProUGUI" },
            { "image", "Image" },
            { "button", "Button" },
            { "input", "InputField" },
            { "toggle", "Toggle" },
            { "slider", "Slider" },
            { "dropdown", "Dropdown" },
            { "scroll_view", "ScrollRect" },
            { "loop_scroll_v", "LoopVerticalScrollRect" },
            { "loop_scroll_h", "LoopHorizontalScrollRect" },
            { "prefab", null },
            { "block", null },
        };

        /// <summary>类型默认尺寸（place.size 未指定时使用）；不在表内 = [100,100]</summary>
        public static readonly Dictionary<string, float[]> DefaultSizes = new Dictionary<string, float[]>
        {
            { "button", new[] { 160f, 48f } },
            { "text", new[] { 200f, 40f } },
            { "tmp", new[] { 200f, 40f } },
        };

        private static PropDef P(string name, EPropKind kind = EPropKind.String, string[] enumValues = null, bool required = false)
        {
            return new PropDef { Name = name, Kind = kind, EnumValues = enumValues, Required = required };
        }

        private static List<PropDef> Defs(params PropDef[] defs)
        {
            var list = new List<PropDef>();
            if (defs == null) return list;
            foreach (PropDef def in defs) list.Add(def);
            return list;
        }
    }
}
