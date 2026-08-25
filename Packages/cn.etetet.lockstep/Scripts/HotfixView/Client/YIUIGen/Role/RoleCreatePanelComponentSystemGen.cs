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
    [EntitySystemOf(typeof(RoleCreatePanelComponent))]
    public static partial class RoleCreatePanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RoleCreatePanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this RoleCreatePanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this RoleCreatePanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComImgBg = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComImgBg");
            self.u_ComInputName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.InputField>("u_ComInputName");
            self.u_ComBtnCreate = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnCreate");
            self.u_EventCreate = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventCreate");
            self.u_EventCreateHandle = self.u_EventCreate.Add(self,RoleCreatePanelComponent.OnEventCreateInvoke);

        }
    }
}
