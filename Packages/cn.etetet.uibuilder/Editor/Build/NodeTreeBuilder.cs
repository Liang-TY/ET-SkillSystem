using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.UIBuilder
{
    /// <summary>
    /// 节点树递归构建：Create → SetParent → Place → Layout → Props → 递归子节点。
    /// 被 S2 场景预览与 S3 BuildPipeline 共用。
    /// place 规则：
    ///   普通父级下未写 place = 铺满父节点（与 YIUI ResetToFullScreen 习惯一致）；
    ///   布局容器(LayoutGroup)下未写 place = 居中 + 类型默认尺寸 + 自动挂 LayoutElement(preferred)，
    ///   让 LayoutGroup 取到正确 preferred 尺寸——否则子节点会被压成接近 0 的尺寸。
    /// </summary>
    public static class NodeTreeBuilder
    {
        public static void Build(Transform parent, List<NodeSpec> nodes, bool parentHasLayout = false)
        {
            foreach (NodeSpec node in nodes)
            {
                BuildNode(parent, node, parentHasLayout);
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

        private static void BuildNode(Transform parent, NodeSpec node, bool parentHasLayout)
        {
            GameObject go = ControlFactory.Create(node);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                GetDefaultSize(node.Type, out float defaultWidth, out float defaultHeight);

                if (parentHasLayout)
                {
                    float width = node.Place != null && node.Place.Width >= 0 ? node.Place.Width : defaultWidth;
                    float height = node.Place != null && node.Place.Height >= 0 ? node.Place.Height : defaultHeight;

                    if (node.Place != null)
                    {
                        // 显式 place 照常应用（SpecLoader 已警告会被 LayoutGroup 覆盖）
                        LayoutApplier.ApplyPlace(rect, node.Place, defaultWidth, defaultHeight);
                    }
                    else
                    {
                        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                        rect.pivot = new Vector2(0.5f, 0.5f);
                        rect.sizeDelta = new Vector2(width, height);
                        rect.anchoredPosition = Vector2.zero;
                    }

                    if (go.GetComponent<LayoutElement>() == null)
                    {
                        LayoutElement element = go.AddComponent<LayoutElement>();
                        element.preferredWidth = width;
                        element.preferredHeight = height;
                    }
                }
                else
                {
                    PlaceSpec place = node.Place ?? new PlaceSpec { Anchor = "stretch" };
                    LayoutApplier.ApplyPlace(rect, place, defaultWidth, defaultHeight);
                }
            }

            if (node.Layout != null)
                LayoutApplier.ApplyLayout(go, node.Layout);

            PropConfigurator.Configure(go, node);

            Build(go.transform, node.Children, node.Layout != null);
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
