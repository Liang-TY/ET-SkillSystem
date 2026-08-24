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
    public partial class TestManyPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "TestMany";
        public const string ResName = "TestManyPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComBtnRow;
        public UnityEngine.UI.Button u_ComBtnA;
        public UnityEngine.UI.Button u_ComBtnB;
        public UnityEngine.UI.Button u_ComBtnC;
        public UnityEngine.RectTransform u_ComGrid;
        public UnityEngine.UI.Button u_ComItem1;
        public UnityEngine.UI.Button u_ComItem2;
        public UnityEngine.UI.Button u_ComItem3;
        public UnityEngine.UI.Button u_ComItem4;
        public UnityEngine.UI.InputField u_ComInputName;
        public UnityEngine.UI.Toggle u_ComToggleAgree;
        public UnityEngine.UI.Button u_ComBtnClose;
        public UITaskEventP0 u_EventClose;
        public UITaskEventHandleP0 u_EventCloseHandle;
        public const string OnEventCloseInvoke = "TestManyPanelComponent.OnEventCloseInvoke";
        public UITaskEventP1<string> u_EventSubmit;
        public UITaskEventHandleP1<string> u_EventSubmitHandle;
        public const string OnEventSubmitInvoke = "TestManyPanelComponent.OnEventSubmitInvoke";

    }
}