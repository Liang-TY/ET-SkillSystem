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
    public partial class SkillPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Skill";
        public const string ResName = "SkillPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComBtnRoot;
        public UnityEngine.UI.Button u_ComBtnSkill1;
        public UnityEngine.UI.Button u_ComBtnSkill2;
        public UnityEngine.UI.Button u_ComBtnSkill3;
        public UnityEngine.UI.LoopVerticalScrollRect u_ComLoopSkillList;
        public UnityEngine.UI.Button u_ComBtnClose;
        public UITaskEventP0 u_EventClose;
        public UITaskEventHandleP0 u_EventCloseHandle;
        public const string OnEventCloseInvoke = "SkillPanelComponent.OnEventCloseInvoke";

    }
}