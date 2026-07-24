using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Common)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class TestScrollItemComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "ScrollTest";
        public const string ResName = "TestScrollItem";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public UnityEngine.UI.Text u_ComU_DataIndex;
        public UnityEngine.UI.Text u_ComU_DataName;
        public UnityEngine.UI.Image u_ComU_DataSelect;
        public UnityEngine.UI.Button u_ComBtnAction;
        public UIEventP0 u_EventSelect;
        public UIEventHandleP0 u_EventSelectHandle;
        public const string OnEventSelectInvoke = "TestScrollItemComponent.OnEventSelectInvoke";
        public UIEventP0 u_EventClick;
        public UIEventHandleP0 u_EventClickHandle;
        public const string OnEventClickInvoke = "TestScrollItemComponent.OnEventClickInvoke";

    }
}