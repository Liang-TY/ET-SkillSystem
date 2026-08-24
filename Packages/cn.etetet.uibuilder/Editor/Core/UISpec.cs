using System.Collections.Generic;

namespace ET.UIBuilder
{
    /// <summary>
    /// spec 根：一个 .ui.yaml 的内存表示。
    /// 结构定义见 Notes/UIBuilder-P1实施方案.md §3。
    /// </summary>
    public class UISpec
    {
        public PanelSpec Panel = new PanelSpec();
        public readonly List<NodeSpec> Nodes = new List<NodeSpec>();
        public readonly List<EventSpec> Events = new List<EventSpec>();

        /// <summary>spec 文件的项目内路径（Packages/... 或 Assets/...），加载失败时为空</summary>
        public string SourcePath;
    }
}
