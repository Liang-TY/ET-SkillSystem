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
    [FriendOf(typeof(BattleTipPanelComponent))]
    public static partial class BattleTipPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BattleTipPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BattleTipPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BattleTipPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
