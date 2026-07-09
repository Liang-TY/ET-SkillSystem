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
    [EntitySystemOf(typeof(LoginPanelComponent))]
    public static partial class LoginPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this LoginPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this LoginPanelComponent self)
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

            self.u_ComInput = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.InputField>("u_ComInput");
            self.u_EventClick1 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClick1");
            self.u_EventClick1Handle = self.u_EventClick1.Add(self,LoginPanelComponent.OnEventClick1Invoke);

        }
    }
}
