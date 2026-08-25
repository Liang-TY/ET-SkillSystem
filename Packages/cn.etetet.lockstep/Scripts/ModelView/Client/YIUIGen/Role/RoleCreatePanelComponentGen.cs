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
    public partial class RoleCreatePanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Role";
        public const string ResName = "RoleCreatePanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.Image u_ComImgBg;
        public UnityEngine.UI.InputField u_ComInputName;
        public UnityEngine.UI.Button u_ComBtnCreate;
        public UITaskEventP0 u_EventCreate;
        public UITaskEventHandleP0 u_EventCreateHandle;
        public const string OnEventCreateInvoke = "RoleCreatePanelComponent.OnEventCreateInvoke";

    }
}