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
    [EntitySystemOf(typeof(MapSelectPanelComponent))]
    public static partial class MapSelectPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MapSelectPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this MapSelectPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this MapSelectPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Popup;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComImgBg = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComImgBg");
            self.u_ComMapList = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComMapList");
            self.u_ComBtnMap1 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnMap1");
            self.u_ComBtnMap2 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnMap2");
            self.u_ComBtnClose = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnClose");
            self.u_EventMap1 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventMap1");
            self.u_EventMap1Handle = self.u_EventMap1.Add(self,MapSelectPanelComponent.OnEventMap1Invoke);
            self.u_EventMap2 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventMap2");
            self.u_EventMap2Handle = self.u_EventMap2.Add(self,MapSelectPanelComponent.OnEventMap2Invoke);
            self.u_EventClose = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClose");
            self.u_EventCloseHandle = self.u_EventClose.Add(self,MapSelectPanelComponent.OnEventCloseInvoke);

        }
    }
}
