using UnityEngine;
using UnityEngine.UI;

namespace ET.UIBuilder
{
    /// <summary>
    /// place → RectTransform（锚点预设展开）；layout → LayoutGroup。
    /// 矩形统一用 offsetMin/offsetMax 表达（与 pivot 无关，pivot 只影响旋转/缩放中心）。
    /// </summary>
    public static class LayoutApplier
    {
        private enum AxisMode
        {
            Center,     // 点锚居中：offset 定中心，size 定宽高
            Stretch,    // 拉伸：两侧 margin 内缩
            EdgeLeft,   // 贴左边：MarginLeft 内缩 + 厚度(宽)=size
            EdgeRight,  // 贴右边：MarginRight 内缩 + 厚度(宽)=size
            EdgeTop,    // 贴顶边：MarginTop 内缩 + 厚度(高)=size
            EdgeBottom, // 贴底边：MarginBottom 内缩 + 厚度(高)=size
        }

        public static void ApplyPlace(RectTransform rect, PlaceSpec place, float defaultWidth, float defaultHeight)
        {
            GetAnchors(place.Anchor,
                out Vector2 anchorMin, out Vector2 anchorMax,
                out AxisMode xMode, out AxisMode yMode);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(place.PivotX, place.PivotY);

            float width = place.Width >= 0 ? place.Width : defaultWidth;
            float height = place.Height >= 0 ? place.Height : defaultHeight;

            float minX, maxX;
            switch (xMode)
            {
                case AxisMode.Stretch:
                    minX = place.MarginLeft;
                    maxX = -place.MarginRight;
                    break;
                case AxisMode.EdgeLeft:
                    minX = place.MarginLeft;
                    maxX = place.MarginLeft + width;
                    break;
                case AxisMode.EdgeRight:
                    maxX = -place.MarginRight;
                    minX = maxX - width;
                    break;
                default: // Center
                    minX = place.OffsetX - width / 2f;
                    maxX = place.OffsetX + width / 2f;
                    break;
            }

            float minY, maxY;
            switch (yMode)
            {
                case AxisMode.Stretch:
                    minY = place.MarginBottom;
                    maxY = -place.MarginTop;
                    break;
                case AxisMode.EdgeTop:
                    maxY = -place.MarginTop;
                    minY = maxY - height;
                    break;
                case AxisMode.EdgeBottom:
                    minY = place.MarginBottom;
                    maxY = minY + height;
                    break;
                default: // Center
                    minY = place.OffsetY - height / 2f;
                    maxY = place.OffsetY + height / 2f;
                    break;
            }

            rect.offsetMin = new Vector2(minX, minY);
            rect.offsetMax = new Vector2(maxX, maxY);
            rect.localRotation = Quaternion.Euler(0f, 0f, place.Rotation);
            rect.localScale = new Vector3(place.ScaleX, place.ScaleY, 1f);
        }

        public static void ApplyLayout(GameObject go, LayoutSpec layout)
        {
            switch (layout.Type)
            {
                case "vertical":
                    VerticalLayoutGroup vertical = go.AddComponent<VerticalLayoutGroup>();
                    ConfigureLinear(vertical, layout, layout.SpacingY);
                    break;

                case "horizontal":
                    HorizontalLayoutGroup horizontal = go.AddComponent<HorizontalLayoutGroup>();
                    ConfigureLinear(horizontal, layout, layout.SpacingX);
                    break;

                case "grid":
                    GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
                    grid.cellSize = new Vector2(layout.CellWidth, layout.CellHeight);
                    grid.spacing = new Vector2(layout.SpacingX, layout.SpacingY);
                    grid.padding = Padding(layout);
                    grid.childAlignment = ParseAnchor(layout.ChildAlignment);
                    grid.constraint = ParseConstraint(layout.Constraint);
                    grid.constraintCount = layout.ConstraintCount;
                    break;
            }
        }

        private static void ConfigureLinear(HorizontalOrVerticalLayoutGroup group, LayoutSpec layout, float spacing)
        {
            group.spacing = spacing;
            group.padding = Padding(layout);
            group.childAlignment = ParseAnchor(layout.ChildAlignment);
            group.childControlWidth = layout.ControlChildSize;
            group.childControlHeight = layout.ControlChildSize;
            group.childForceExpandWidth = layout.ChildForceExpand;
            group.childForceExpandHeight = layout.ChildForceExpand;
        }

        private static RectOffset Padding(LayoutSpec layout)
        {
            return new RectOffset(
                (int)layout.PaddingLeft, (int)layout.PaddingRight,
                (int)layout.PaddingTop, (int)layout.PaddingBottom);
        }

        private static TextAnchor ParseAnchor(string value)
        {
            return System.Enum.TryParse(value, out TextAnchor anchor) ? anchor : TextAnchor.UpperLeft;
        }

        private static GridLayoutGroup.Constraint ParseConstraint(string value)
        {
            return System.Enum.TryParse(value, out GridLayoutGroup.Constraint constraint)
                ? constraint
                : GridLayoutGroup.Constraint.Flexible;
        }

        /// <summary>
        /// 锚点预设 → anchors + 各轴模式。预设词集合与 SpecSchema.Anchors 一致（lint 已拦截非法值）。
        /// 部分拉伸预设 = 贴边定厚条：点轴以对应 margin 内缩、按 size(或类型默认)确定厚度，该轴忽略 offset。
        /// </summary>
        private static void GetAnchors(string anchor, out Vector2 anchorMin, out Vector2 anchorMax,
            out AxisMode xMode, out AxisMode yMode)
        {
            xMode = yMode = AxisMode.Center;
            switch (anchor)
            {
                case "top":
                    anchorMin = anchorMax = new Vector2(0.5f, 1f);
                    break;
                case "bottom":
                    anchorMin = anchorMax = new Vector2(0.5f, 0f);
                    break;
                case "left":
                    anchorMin = anchorMax = new Vector2(0f, 0.5f);
                    break;
                case "right":
                    anchorMin = anchorMax = new Vector2(1f, 0.5f);
                    break;
                case "top_left":
                    anchorMin = anchorMax = new Vector2(0f, 1f);
                    break;
                case "top_right":
                    anchorMin = anchorMax = new Vector2(1f, 1f);
                    break;
                case "bottom_left":
                    anchorMin = anchorMax = new Vector2(0f, 0f);
                    break;
                case "bottom_right":
                    anchorMin = anchorMax = new Vector2(1f, 0f);
                    break;
                case "stretch":
                case "full":
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.one;
                    xMode = yMode = AxisMode.Stretch;
                    break;
                case "top_stretch":
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    xMode = AxisMode.Stretch;
                    yMode = AxisMode.EdgeTop;
                    break;
                case "bottom_stretch":
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(1f, 0f);
                    xMode = AxisMode.Stretch;
                    yMode = AxisMode.EdgeBottom;
                    break;
                case "left_stretch":
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(0f, 1f);
                    xMode = AxisMode.EdgeLeft;
                    yMode = AxisMode.Stretch;
                    break;
                case "right_stretch":
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = new Vector2(1f, 1f);
                    xMode = AxisMode.EdgeRight;
                    yMode = AxisMode.Stretch;
                    break;
                default: // center 及一切未匹配值
                    anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                    break;
            }
        }
    }
}
