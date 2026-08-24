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
    [EntitySystemOf(typeof(MainHUDPanelComponent))]
    public static partial class MainHUDPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MainHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this MainHUDPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this MainHUDPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Scene;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComTextRoleName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Text>("u_ComTextRoleName");
            self.u_ComBtnRoot = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComBtnRoot");
            self.u_ComBtnSettings = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnSettings");
            self.u_ComBtnBag = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnBag");
            self.u_ComBtnRoleInfo = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnRoleInfo");
            self.u_ComBtnShop = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnShop");
            self.u_ComBtnMap = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnMap");
            self.u_ComBtnActivity = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnActivity");
            self.u_EventSettings = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSettings");
            self.u_EventSettingsHandle = self.u_EventSettings.Add(self,MainHUDPanelComponent.OnEventSettingsInvoke);
            self.u_EventBag = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventBag");
            self.u_EventBagHandle = self.u_EventBag.Add(self,MainHUDPanelComponent.OnEventBagInvoke);
            self.u_EventRoleInfo = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventRoleInfo");
            self.u_EventRoleInfoHandle = self.u_EventRoleInfo.Add(self,MainHUDPanelComponent.OnEventRoleInfoInvoke);
            self.u_EventShop = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventShop");
            self.u_EventShopHandle = self.u_EventShop.Add(self,MainHUDPanelComponent.OnEventShopInvoke);
            self.u_EventMap = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventMap");
            self.u_EventMapHandle = self.u_EventMap.Add(self,MainHUDPanelComponent.OnEventMapInvoke);
            self.u_EventActivity = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventActivity");
            self.u_EventActivityHandle = self.u_EventActivity.Add(self,MainHUDPanelComponent.OnEventActivityInvoke);

        }
    }
}
