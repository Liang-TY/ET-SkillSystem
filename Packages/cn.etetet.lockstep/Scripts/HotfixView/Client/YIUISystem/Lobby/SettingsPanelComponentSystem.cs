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
    [FriendOf(typeof(SettingsPanelComponent))]
    public static partial class SettingsPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this SettingsPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this SettingsPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SettingsPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(SettingsPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this SettingsPanelComponent self)
        {
            await self.Root().YIUIMgr().ClosePanelAsync<SettingsPanelComponent>();
        }

        #endregion YIUIEvent结束
    }
}
