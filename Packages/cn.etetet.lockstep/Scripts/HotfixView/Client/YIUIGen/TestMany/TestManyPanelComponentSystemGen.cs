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
    [EntitySystemOf(typeof(TestManyPanelComponent))]
    public static partial class TestManyPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TestManyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this TestManyPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this TestManyPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Popup;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 5;

            self.u_ComBtnRow = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComBtnRow");
            self.u_ComBtnA = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnA");
            self.u_ComBtnB = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnB");
            self.u_ComBtnC = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnC");
            self.u_ComGrid = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComGrid");
            self.u_ComItem1 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComItem1");
            self.u_ComItem2 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComItem2");
            self.u_ComItem3 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComItem3");
            self.u_ComItem4 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComItem4");
            self.u_ComInputName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.InputField>("u_ComInputName");
            self.u_ComToggleAgree = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Toggle>("u_ComToggleAgree");
            self.u_ComBtnClose = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnClose");
            self.u_EventClose = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClose");
            self.u_EventCloseHandle = self.u_EventClose.Add(self,TestManyPanelComponent.OnEventCloseInvoke);
            self.u_EventSubmit = self.UIBase.EventTable.FindEvent<UITaskEventP1<string>>("u_EventSubmit");
            self.u_EventSubmitHandle = self.u_EventSubmit.Add(self,TestManyPanelComponent.OnEventSubmitInvoke);

        }
    }
}
