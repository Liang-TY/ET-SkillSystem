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
    [FriendOf(typeof(BattleInfoPanelComponent))]
    public static partial class BattleInfoPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BattleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BattleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BattleInfoPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
