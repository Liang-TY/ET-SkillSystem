using System;
using TrueSync;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2026.8.25
    /// Desc
    /// </summary>
    [FriendOf(typeof(RoleSelectPanelComponent))]
    public static partial class RoleSelectPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RoleSelectPanelComponent self)
        {
            // 每次打开重置选中态：卡片还原白色，进入按钮禁用
            self.RoleSelected = false;
            self.u_ComBtnRole.image.color = Color.white;
            self.u_ComBtnEnter.interactable = false;
        }

        [EntitySystem]
        private static void Destroy(this RoleSelectPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RoleSelectPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(RoleSelectPanelComponent.OnEventEnterTownInvoke)]
        private static async ETTask OnEventEnterTownInvoke(this RoleSelectPanelComponent self)
        {
            if (!self.RoleSelected) return;   // 未选中不给进（按钮已禁用，双保险）

            // DemoUI：确认选角 → 进城镇（出生点=街道中段，03 文档 §1.1）
            await self.Root().YIUIMgr().ClosePanelAsync<RoleSelectPanelComponent>();
            await TownHelper.EnterTown(self.Root(), new TSVector(0, 0, 0));
        }
        
        [YIUIInvoke(RoleSelectPanelComponent.OnEventSelectRoleInvoke)]
        private static async ETTask OnEventSelectRoleInvoke(this RoleSelectPanelComponent self)
        {
            // 选中：卡片高亮（暖金色）+ 进入按钮解锁
            self.RoleSelected = true;
            self.u_ComBtnRole.image.color = new Color(1f, 0.85f, 0.4f);
            self.u_ComBtnEnter.interactable = true;
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
