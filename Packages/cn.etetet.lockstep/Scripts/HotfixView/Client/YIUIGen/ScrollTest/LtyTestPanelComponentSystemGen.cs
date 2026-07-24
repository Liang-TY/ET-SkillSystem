using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [FriendOf(typeof(YIUIWindowComponent))]
    [FriendOf(typeof(YIUIPanelComponent))]
    [EntitySystemOf(typeof(LtyTestPanelComponent))]
    public static partial class LtyTestPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LtyTestPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this LtyTestPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this LtyTestPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10;

            self.u_ComLoopScrollHorizontal = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComLoopScrollHorizontal");

        }
    }
}
