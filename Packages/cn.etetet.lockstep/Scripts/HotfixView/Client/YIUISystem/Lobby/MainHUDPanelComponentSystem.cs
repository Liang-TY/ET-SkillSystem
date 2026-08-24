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
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventMapInvoke)]
        private static async ETTask OnEventMapInvoke(this MainHUDPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventShopInvoke)]
        private static async ETTask OnEventShopInvoke(this MainHUDPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventRoleInfoInvoke)]
        private static async ETTask OnEventRoleInfoInvoke(this MainHUDPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventBagInvoke)]
        private static async ETTask OnEventBagInvoke(this MainHUDPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MainHUDPanelComponent.OnEventSettingsInvoke)]
        private static async ETTask OnEventSettingsInvoke(this MainHUDPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
