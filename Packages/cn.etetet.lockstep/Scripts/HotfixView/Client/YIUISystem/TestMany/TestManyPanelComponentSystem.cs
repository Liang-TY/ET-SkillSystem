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
    [FriendOf(typeof(TestManyPanelComponent))]
    public static partial class TestManyPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TestManyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TestManyPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TestManyPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(TestManyPanelComponent.OnEventSubmitInvoke)]
        private static async ETTask OnEventSubmitInvoke(this TestManyPanelComponent self, string p1)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(TestManyPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this TestManyPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
