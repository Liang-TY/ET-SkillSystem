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
    [EntitySystemOf(typeof(BattleTipPanelComponent))]
    public static partial class BattleTipPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleTipPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this BattleTipPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this BattleTipPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Tips;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComTextTip = self.UIBase.ComponentTable.FindComponent<TMPro.TextMeshProUGUI>("u_ComTextTip");

        }
    }
}
