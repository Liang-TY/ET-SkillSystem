using System;
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
    [FriendOf(typeof(MainHUDPanelComponent))]
    public static partial class MainHUDPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MainHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MainHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainHUDPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventActivityInvoke)]
        private static async ETTask OnEventActivityInvoke(this MainHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 活动：功能开发中（批次③）");
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventMapInvoke)]
        private static async ETTask OnEventMapInvoke(this MainHUDPanelComponent self)
        {
            await self.Root().YIUIRoot().OpenPanelAsync<MapSelectPanelComponent>();
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventShopInvoke)]
        private static async ETTask OnEventShopInvoke(this MainHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 商城：功能开发中（批次③）");
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventRoleInfoInvoke)]
        private static async ETTask OnEventRoleInfoInvoke(this MainHUDPanelComponent self)
        {
            await self.Root().YIUIRoot().OpenPanelAsync<RoleInfoPanelComponent>();
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventBagInvoke)]
        private static async ETTask OnEventBagInvoke(this MainHUDPanelComponent self)
        {
            await self.Root().YIUIRoot().OpenPanelAsync<BagPanelComponent>();
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventSettingsInvoke)]
        private static async ETTask OnEventSettingsInvoke(this MainHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 设置：功能开发中（批次③）");
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
