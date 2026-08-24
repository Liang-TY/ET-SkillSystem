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
    [FriendOf(typeof(RoleCreatePanelComponent))]
    public static partial class RoleCreatePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RoleCreatePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RoleCreatePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RoleCreatePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(RoleCreatePanelComponent.OnEventCreateInvoke)]
        private static async ETTask OnEventCreateInvoke(this RoleCreatePanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
