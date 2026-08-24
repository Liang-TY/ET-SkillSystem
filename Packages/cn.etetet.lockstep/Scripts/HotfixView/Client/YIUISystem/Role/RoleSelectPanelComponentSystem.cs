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
            // DemoUI：确认选角 → 进城镇（出生点=街道中段，03 文档 §1.1）
            await self.Root().YIUIMgr().ClosePanelAsync<RoleSelectPanelComponent>();
            await TownHelper.EnterTown(self.Root(), new TSVector(0, 0, 0));
        }
        
        [YIUIInvoke(RoleSelectPanelComponent.OnEventSelectRoleInvoke)]
        private static async ETTask OnEventSelectRoleInvoke(this RoleSelectPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
