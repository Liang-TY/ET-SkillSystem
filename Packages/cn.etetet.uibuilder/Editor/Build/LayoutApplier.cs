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
        public static void ApplyPlace(RectTransform rect, PlaceSpec place, float defaultWidth, float defaultHeight)
        {
            GetAnchors(place.Anchor, out Vector2 anchorMin, out Vector2 anchorMax, out bool stretchX, out bool stretchY);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(place.PivotX, place.PivotY);

            float width = place.Width >= 0 ? place.Width : defaultWidth;
            float height = place.Height >= 0 ? place.Height : defaultHeight;

            float minX, maxX;
            if (stretchX)
            {
                minX = place.MarginLeft;
                maxX = -place.MarginRight;
            }
            else
            {
                minX = place.OffsetX - width / 2f;
                maxX = place.OffsetX + width / 2f;
            }

            float minY, maxY;
            if (stretchY)
            {
                minY = place.MarginBottom;
                maxY = -place.MarginTop;
            }
            else
            {
                minY = place.OffsetY - height / 2f;
                maxY = place.OffsetY + height / 2f;
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

        /// <summary>锚点预设 → anchors + 拉伸轴标记。预设词集合与 SpecSchema.Anchors 一致（lint 已拦截非法值）。</summary>
        private static void GetAnchors(string anchor, out Vector2 anchorMin, out Vector2 anchorMax,
            out bool stretchX, out bool stretchY)
        {
            stretchX = stretchY = false;
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
                    stretchX = stretchY = true;
                    break;
                case "top_stretch":
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    stretchX = true;
                    break;
                case "bottom_stretch":
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(1f, 0f);
                    stretchX = true;
                    break;
                case "left_stretch":
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(0f, 1f);
                    stretchY = true;
                    break;
                case "right_stretch":
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = new Vector2(1f, 1f);
                    stretchY = true;
                    break;
                default: // center 及一切未匹配值
                    anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                    break;
            }
        }
    }
}
