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
    public partial class MapSelectPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Battle";
        public const string ResName = "MapSelectPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComMapList;
        public UnityEngine.UI.Button u_ComBtnMap1;
        public UnityEngine.UI.Button u_ComBtnMap2;
        public UnityEngine.UI.Button u_ComBtnClose;
        public UITaskEventP0 u_EventMap1;
        public UITaskEventHandleP0 u_EventMap1Handle;
        public const string OnEventMap1Invoke = "MapSelectPanelComponent.OnEventMap1Invoke";
        public UITaskEventP0 u_EventMap2;
        public UITaskEventHandleP0 u_EventMap2Handle;
        public const string OnEventMap2Invoke = "MapSelectPanelComponent.OnEventMap2Invoke";
        public UITaskEventP0 u_EventClose;
        public UITaskEventHandleP0 u_EventCloseHandle;
        public const string OnEventCloseInvoke = "MapSelectPanelComponent.OnEventCloseInvoke";

    }
}