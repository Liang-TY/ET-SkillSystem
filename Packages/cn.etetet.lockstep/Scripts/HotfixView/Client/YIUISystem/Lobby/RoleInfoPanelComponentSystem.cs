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
    [FriendOf(typeof(RoleInfoPanelComponent))]
    public static partial class RoleInfoPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RoleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RoleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RoleInfoPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(RoleInfoPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this RoleInfoPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
