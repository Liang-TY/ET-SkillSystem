using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Panel)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class RoleSelectPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Role";
        public const string ResName = "RoleSelectPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.Button u_ComBtnRole;
        public UnityEngine.UI.Button u_ComBtnEnter;
        public UITaskEventP0 u_EventSelectRole;
        public UITaskEventHandleP0 u_EventSelectRoleHandle;
        public const string OnEventSelectRoleInvoke = "RoleSelectPanelComponent.OnEventSelectRoleInvoke";
        public UITaskEventP0 u_EventEnterTown;
        public UITaskEventHandleP0 u_EventEnterTownHandle;
        public const string OnEventEnterTownInvoke = "RoleSelectPanelComponent.OnEventEnterTownInvoke";

    }
}