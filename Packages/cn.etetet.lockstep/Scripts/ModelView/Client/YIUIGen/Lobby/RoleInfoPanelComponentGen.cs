using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Popup)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class RoleInfoPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Lobby";
        public const string ResName = "RoleInfoPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComWindow;
        public UnityEngine.UI.Image u_ComImgBg;
        public UnityEngine.UI.Text u_ComTextInfo;
        public UnityEngine.UI.Button u_ComBtnClose;
        public UITaskEventP0 u_EventClose;
        public UITaskEventHandleP0 u_EventCloseHandle;
        public const string OnEventCloseInvoke = "RoleInfoPanelComponent.OnEventCloseInvoke";
        public UIEventP0 u_EventWindowDrag;
        public UIEventHandleP0 u_EventWindowDragHandle;
        public const string OnEventWindowDragInvoke = "RoleInfoPanelComponent.OnEventWindowDragInvoke";

    }
}