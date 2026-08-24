using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2026.8.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(SkillPanelComponent))]
    public static partial class SkillPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this SkillPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this SkillPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SkillPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(SkillPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this SkillPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
