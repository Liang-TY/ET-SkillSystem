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
    [EntitySystemOf(typeof(BagPanelComponent))]
    public static partial class BagPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BagPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this BagPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this BagPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Popup;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComWindow = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComWindow");
            self.u_ComGridRoot = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComGridRoot");
            self.u_ComBtnClose = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnClose");
            self.u_EventClose = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClose");
            self.u_EventCloseHandle = self.u_EventClose.Add(self,BagPanelComponent.OnEventCloseInvoke);
            self.u_EventWindowDrag = self.UIBase.EventTable.FindEvent<UIEventP1<object>>("u_EventWindowDrag");
            self.u_EventWindowDragHandle = self.u_EventWindowDrag.Add(self,BagPanelComponent.OnEventWindowDragInvoke);

        }
    }
}
