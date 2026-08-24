namespace ET.UIBuilder
{
    /// <summary>
    /// 容器自动布局（§3.5）。Type = vertical/horizontal/grid；
    /// ChildAlignment = TextAnchor 枚举名；Constraint = GridLayoutGroup.Constraint 枚举名。
    /// </summary>
    public class LayoutSpec
    {
        public string Type;

        public float SpacingX;
        public float SpacingY;

        /// <summary>[左,右,上,下]（Padding 数组顺序，注意与 margins 的[左,上,右,下]不同）</summary>
        public float PaddingLeft;
        public float PaddingRight;
        public float PaddingTop;
        public float PaddingBottom;

        public string ChildAlignment = "UpperLeft";
        public bool ControlChildSize = true;
        public bool ChildForceExpand;

        // grid 专属
        public float CellWidth;
        public float CellHeight;
        public string Constraint = "Flexible";
        public int ConstraintCount = 1;
    }
}
