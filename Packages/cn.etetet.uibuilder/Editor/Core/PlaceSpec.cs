namespace ET.UIBuilder
{
    /// <summary>
    /// 锚点定位（§3.4）。anchor 预设词在 SpecSchema.Anchors。
    /// Width/Height &lt; 0 表示未指定（构建时用类型默认尺寸）。
    /// </summary>
    public class PlaceSpec
    {
        public string Anchor = "center";

        /// <summary>非 stretch 系：相对锚点偏移</summary>
        public float OffsetX;
        public float OffsetY;

        /// <summary>stretch 系：[左,上,右,下] 边距（Margins 数组顺序）</summary>
        public float MarginLeft;
        public float MarginTop;
        public float MarginRight;
        public float MarginBottom;

        public float Width = -1f;
        public float Height = -1f;

        public float PivotX = 0.5f;
        public float PivotY = 0.5f;

        /// <summary>z 轴角度</summary>
        public float Rotation;

        public float ScaleX = 1f;
        public float ScaleY = 1f;
    }
}
