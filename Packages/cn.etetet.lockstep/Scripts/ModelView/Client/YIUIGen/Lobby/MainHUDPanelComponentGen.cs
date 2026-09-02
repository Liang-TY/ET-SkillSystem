using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Scene)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class MainHUDPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Lobby";
        public const string ResName = "MainHUDPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public TMPro.TextMeshProUGUI u_ComTextRoleName;
        public UnityEngine.RectTransform u_ComBtnRoot;
        public UnityEngine.UI.Button u_ComBtnSettings;
        public UnityEngine.UI.Button u_ComBtnBag;
        public UnityEngine.UI.Button u_ComBtnRoleInfo;
        public UnityEngine.UI.Button u_ComBtnShop;
        public UnityEngine.UI.Button u_ComBtnMap;
        public UnityEngine.UI.Button u_ComBtnActivity;
        public UITaskEventP0 u_EventSettings;
        public UITaskEventHandleP0 u_EventSettingsHandle;
        public const string OnEventSettingsInvoke = "MainHUDPanelComponent.OnEventSettingsInvoke";
        public UITaskEventP0 u_EventBag;
        public UITaskEventHandleP0 u_EventBagHandle;
        public const string OnEventBagInvoke = "MainHUDPanelComponent.OnEventBagInvoke";
        public UITaskEventP0 u_EventRoleInfo;
        public UITaskEventHandleP0 u_EventRoleInfoHandle;
        public const string OnEventRoleInfoInvoke = "MainHUDPanelComponent.OnEventRoleInfoInvoke";
        public UITaskEventP0 u_EventShop;
        public UITaskEventHandleP0 u_EventShopHandle;
        public const string OnEventShopInvoke = "MainHUDPanelComponent.OnEventShopInvoke";
        public UITaskEventP0 u_EventMap;
        public UITaskEventHandleP0 u_EventMapHandle;
        public const string OnEventMapInvoke = "MainHUDPanelComponent.OnEventMapInvoke";
        public UITaskEventP0 u_EventActivity;
        public UITaskEventHandleP0 u_EventActivityHandle;
        public const string OnEventActivityInvoke = "MainHUDPanelComponent.OnEventActivityInvoke";

    }
}