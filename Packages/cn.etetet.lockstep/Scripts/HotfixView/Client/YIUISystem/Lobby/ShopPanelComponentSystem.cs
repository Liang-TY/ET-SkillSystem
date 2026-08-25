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
    [FriendOf(typeof(ShopPanelComponent))]
    public static partial class ShopPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ShopPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ShopPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ShopPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ShopPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this ShopPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
