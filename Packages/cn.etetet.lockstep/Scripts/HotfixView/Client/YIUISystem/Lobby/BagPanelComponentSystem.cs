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
    [FriendOf(typeof(BagPanelComponent))]
    public static partial class BagPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BagPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BagPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BagPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BagPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this BagPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
