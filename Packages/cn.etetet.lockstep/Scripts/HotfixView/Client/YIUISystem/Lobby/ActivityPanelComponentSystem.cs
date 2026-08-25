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
    [FriendOf(typeof(ActivityPanelComponent))]
    public static partial class ActivityPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ActivityPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ActivityPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ActivityPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ActivityPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this ActivityPanelComponent self)
        {
            await self.Root().YIUIMgr().ClosePanelAsync<ActivityPanelComponent>();
        }
        #endregion YIUIEvent结束
    }
}
