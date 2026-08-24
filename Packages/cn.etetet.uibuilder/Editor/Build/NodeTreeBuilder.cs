using System.Collections.Generic;
using UnityEngine;

namespace ET.UIBuilder
{
    /// <summary>
    /// 节点树递归构建：Create → SetParent → Place → Layout → Props → 递归子节点。
    /// 被 S2 场景预览与 S3 BuildPipeline 共用。
    /// place 未写 = 铺满父节点（与 YIUI ResetToFullScreen 习惯一致）；
    /// 处于 LayoutGroup 容器下的子节点 place 会被布局覆盖（SpecLoader 已 warning）。
    /// </summary>
    public static class NodeTreeBuilder
    {
        public static void Build(Transform parent, List<NodeSpec> nodes)
        {
            foreach (NodeSpec node in nodes)
            {
                BuildNode(parent, node);
            }
        }

        public static int CountNodes(List<NodeSpec> nodes)
        {
            int count = nodes.Count;
            foreach (NodeSpec node in nodes)
            {
                count += CountNodes(node.Children);
            }

            return count;
        }

        private static void BuildNode(Transform parent, NodeSpec node)
        {
            GameObject go = ControlFactory.Create(node);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                PlaceSpec place = node.Place ?? new PlaceSpec { Anchor = "stretch" };
                GetDefaultSize(node.Type, out float defaultWidth, out float defaultHeight);
                LayoutApplier.ApplyPlace(rect, place, defaultWidth, defaultHeight);
            }

            if (node.Layout != null)
                LayoutApplier.ApplyLayout(go, node.Layout);

            PropConfigurator.Configure(go, node);

            Build(go.transform, node.Children);
        }

        private static void GetDefaultSize(string type, out float width, out float height)
        {
            if (SpecSchema.DefaultSizes.TryGetValue(type, out float[] size))
            {
                width = size[0];
                height = size[1];
            }
            else
            {
                width = height = 100f;
            }
        }
    }
}
