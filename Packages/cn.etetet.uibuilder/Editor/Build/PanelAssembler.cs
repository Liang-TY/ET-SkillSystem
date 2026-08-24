using System;
using UnityEngine;
using YIUIFramework;

namespace ET.UIBuilder
{
    /// <summary>
    /// 面板骨架（逻辑迁移自 ubridge 的 UBridgeYIUICreatePanelHandler）：
    /// 根节点(RectTransform 全屏 + CanvasRenderer + UIBindCDETable) + UIBlockBG 子节点。
    /// 层/优先级/缓存等选项写入 UIBindCDETable，供 S3 代码生成读取。
    /// 只在内存创建，不落盘。
    /// </summary>
    public static class PanelAssembler
    {
        public static GameObject CreateSkeleton(UISpec spec)
        {
            PanelSpec panel = spec.Panel;

            var rootGo = new GameObject(panel.Name);
            RectTransform root = rootGo.AddComponent<RectTransform>();
            root.ResetToFullScreen();
            rootGo.AddComponent<CanvasRenderer>();

            var cde = rootGo.AddComponent<UIBindCDETable>();
            cde.UICodeType = EUICodeType.Panel;
            cde.PanelLayer = ParseEnum(panel.Layer, EPanelLayer.Panel);
            cde.PanelStackOption = ParseEnum(panel.StackOption, EPanelStackOption.VisibleTween);
            cde.Priority = panel.Priority;
            if (panel.CacheSeconds > 0)
            {
                cde.PanelOption |= EPanelOption.TimeCache;
                cde.CachePanelTime = panel.CacheSeconds;
            }

            if (panel.BlockBg)
            {
                var bgGo = new GameObject("UIBlockBG");
                RectTransform bgRect = bgGo.AddComponent<RectTransform>();
                bgGo.AddComponent<CanvasRenderer>();
                bgGo.AddComponent<UIBlock>();
                bgRect.SetParent(root, false);
                bgRect.ResetToFullScreen();
            }

            rootGo.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            return rootGo;
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct, Enum
        {
            return Enum.TryParse(value, out T result) ? result : fallback;
        }
    }
}
