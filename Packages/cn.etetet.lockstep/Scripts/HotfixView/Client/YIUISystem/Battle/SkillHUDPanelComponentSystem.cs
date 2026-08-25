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
    [FriendOf(typeof(SkillHUDPanelComponent))]
    public static partial class SkillHUDPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this SkillHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this SkillHUDPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SkillHUDPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(SkillHUDPanelComponent.OnEventSkill4Invoke)]
        private static async ETTask OnEventSkill4Invoke(this SkillHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 技能按钮触发（接技能系统待后续）");
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(SkillHUDPanelComponent.OnEventSkill3Invoke)]
        private static async ETTask OnEventSkill3Invoke(this SkillHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 技能按钮触发（接技能系统待后续）");
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(SkillHUDPanelComponent.OnEventSkill2Invoke)]
        private static async ETTask OnEventSkill2Invoke(this SkillHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 技能按钮触发（接技能系统待后续）");
            await ETTask.CompletedTask;
        }
        
        [YIUIInvoke(SkillHUDPanelComponent.OnEventSkill1Invoke)]
        private static async ETTask OnEventSkill1Invoke(this SkillHUDPanelComponent self)
        {
            Log.Info("[DemoUI] 技能按钮触发（接技能系统待后续）");
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
