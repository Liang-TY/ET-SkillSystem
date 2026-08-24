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
    [EntitySystemOf(typeof(RoleSelectPanelComponent))]
    public static partial class RoleSelectPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RoleSelectPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this RoleSelectPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this RoleSelectPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComBtnRole = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnRole");
            self.u_ComBtnEnter = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnEnter");
            self.u_EventSelectRole = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSelectRole");
            self.u_EventSelectRoleHandle = self.u_EventSelectRole.Add(self,RoleSelectPanelComponent.OnEventSelectRoleInvoke);
            self.u_EventEnterTown = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventEnterTown");
            self.u_EventEnterTownHandle = self.u_EventEnterTown.Add(self,RoleSelectPanelComponent.OnEventEnterTownInvoke);

        }
    }
}
