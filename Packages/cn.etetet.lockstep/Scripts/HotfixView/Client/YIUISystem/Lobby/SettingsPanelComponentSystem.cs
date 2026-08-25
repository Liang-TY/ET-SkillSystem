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

        
        
        [YIUIInvoke(SettingsPanelComponent.OnEventWindowDragInvoke)]
        private static void OnEventWindowDragInvoke(this SettingsPanelComponent self, object p1)
        {
            // 拖动窗口（按下标题区）：WindowDragComponent 逐帧移动，松手结束
            WindowDragHelper.Begin(self.Root(), self.u_ComWindow);
        }
        #endregion YIUIEvent结束
    }
}
