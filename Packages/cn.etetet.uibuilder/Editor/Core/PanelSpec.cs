namespace ET.UIBuilder
{
    /// <summary>
    /// panel 段：面板元信息（§3.1）。
    /// Layer 合法值 = EPanelLayer；StackOption 合法值 = EPanelStackOption。
    /// </summary>
    public class PanelSpec
    {
        public string Name;

        /// <summary>YIUI 包名（目录与生成代码归属），如 Skill</summary>
        public string Pkg;

        /// <summary>Top/Tips/Popup/Panel/Scene/Bottom</summary>
        public string Layer = "Panel";

        /// <summary>TimeCache 秒数，0 = 不缓存</summary>
        public int CacheSeconds;

        /// <summary>自动带 UIBlockBG 子节点</summary>
        public bool BlockBg = true;

        /// <summary>None/Visible/VisibleTween/Omit</summary>
        public string StackOption = "VisibleTween";

        public int Priority;

        /// <summary>输出 prefab 路径；空 = &lt;spec目录&gt;/Prefabs/&lt;Name&gt;.prefab</summary>
        public string PrefabPath;
    }
}
