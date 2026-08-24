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
    [FriendOf(typeof(MapSelectPanelComponent))]
    public static partial class MapSelectPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MapSelectPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MapSelectPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MapSelectPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(MapSelectPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this MapSelectPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MapSelectPanelComponent.OnEventMap2Invoke)]
        private static async ETTask OnEventMap2Invoke(this MapSelectPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(MapSelectPanelComponent.OnEventMap1Invoke)]
        private static async ETTask OnEventMap1Invoke(this MapSelectPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
