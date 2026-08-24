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
    [EntitySystemOf(typeof(SkillHUDPanelComponent))]
    public static partial class SkillHUDPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this SkillHUDPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this SkillHUDPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Scene;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComSkillRoot = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComSkillRoot");
            self.u_ComBtnSkill1 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnSkill1");
            self.u_ComBtnSkill2 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnSkill2");
            self.u_ComBtnSkill3 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnSkill3");
            self.u_ComBtnSkill4 = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnSkill4");
            self.u_EventSkill1 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSkill1");
            self.u_EventSkill1Handle = self.u_EventSkill1.Add(self,SkillHUDPanelComponent.OnEventSkill1Invoke);
            self.u_EventSkill2 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSkill2");
            self.u_EventSkill2Handle = self.u_EventSkill2.Add(self,SkillHUDPanelComponent.OnEventSkill2Invoke);
            self.u_EventSkill3 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSkill3");
            self.u_EventSkill3Handle = self.u_EventSkill3.Add(self,SkillHUDPanelComponent.OnEventSkill3Invoke);
            self.u_EventSkill4 = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventSkill4");
            self.u_EventSkill4Handle = self.u_EventSkill4.Add(self,SkillHUDPanelComponent.OnEventSkill4Invoke);

        }
    }
}
