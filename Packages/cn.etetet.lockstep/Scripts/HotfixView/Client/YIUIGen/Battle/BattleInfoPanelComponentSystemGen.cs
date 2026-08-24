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
    [EntitySystemOf(typeof(BattleInfoPanelComponent))]
    public static partial class BattleInfoPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this BattleInfoPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this BattleInfoPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Scene;
            self.UIPanel.PanelOption = EPanelOption.None;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;

            self.u_ComTextPlayerName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Text>("u_ComTextPlayerName");
            self.u_ComImgPlayerHp = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComImgPlayerHp");
            self.u_ComTextMonsterName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Text>("u_ComTextMonsterName");
            self.u_ComImgMonsterHp = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComImgMonsterHp");

        }
    }
}
