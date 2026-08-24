using System.Collections.Generic;

namespace ET.UIBuilder
{
    /// <summary>
    /// 节点树中的一个控件描述（§3.2）。
    /// 命名约定：u_Com 开头的节点注册进 CDE 表 C 表，其余为纯布局节点。
    /// </summary>
    public class NodeSpec
    {
        public string Name;

        /// <summary>控件类型（SpecSchema.NodeTypes 封闭集合）</summary>
        public string Type;

        /// <summary>bind.component：覆盖类型默认绑定组件（prefab/block 类型必填）</summary>
        public string BindComponent;

        /// <summary>锚点定位；null = 默认（第一层子节点默认铺满父节点）</summary>
        public PlaceSpec Place;

        /// <summary>作为容器的自动布局；null = 无</summary>
        public LayoutSpec Layout;

        /// <summary>控件属性（值为 YAML 原始标量：string/int/double/bool，构建期消费）</summary>
        public readonly Dictionary<string, object> Props = new Dictionary<string, object>();

        /// <summary>预留：贴图 pass（v1 忽略并 warning）</summary>
        public string Image;

        /// <summary>预留：样式 token（v1 忽略并 warning）</summary>
        public string Style;

        public readonly List<NodeSpec> Children = new List<NodeSpec>();

        /// <summary>是否注册进 CDE 表 C 表</summary>
        public bool IsBound => !string.IsNullOrEmpty(Name) && Name.StartsWith("u_Com");
    }
}
