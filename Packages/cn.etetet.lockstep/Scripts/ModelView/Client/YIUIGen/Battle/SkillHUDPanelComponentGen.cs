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
    public partial class SkillHUDPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Battle";
        public const string ResName = "SkillHUDPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComSkillRoot;
        public UnityEngine.UI.Button u_ComBtnSkill1;
        public UnityEngine.UI.Button u_ComBtnSkill2;
        public UnityEngine.UI.Button u_ComBtnSkill3;
        public UnityEngine.UI.Button u_ComBtnSkill4;
        public UITaskEventP0 u_EventSkill1;
        public UITaskEventHandleP0 u_EventSkill1Handle;
        public const string OnEventSkill1Invoke = "SkillHUDPanelComponent.OnEventSkill1Invoke";
        public UITaskEventP0 u_EventSkill2;
        public UITaskEventHandleP0 u_EventSkill2Handle;
        public const string OnEventSkill2Invoke = "SkillHUDPanelComponent.OnEventSkill2Invoke";
        public UITaskEventP0 u_EventSkill3;
        public UITaskEventHandleP0 u_EventSkill3Handle;
        public const string OnEventSkill3Invoke = "SkillHUDPanelComponent.OnEventSkill3Invoke";
        public UITaskEventP0 u_EventSkill4;
        public UITaskEventHandleP0 u_EventSkill4Handle;
        public const string OnEventSkill4Invoke = "SkillHUDPanelComponent.OnEventSkill4Invoke";

    }
}