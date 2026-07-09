using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2026.7.8
    /// Desc
    /// </summary>
    [FriendOf(typeof(LoginPanelComponent))]
    public static partial class LoginPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoginPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(LoginPanelComponent.OnEventClick1Invoke)]
        private static async ETTask OnEventClick1Invoke(this LoginPanelComponent self)
        {
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();

            string account = self.u_ComInput.text;
            if (string.IsNullOrEmpty(account))
            {
                account = "TestPlayer";
            }

            LoginHelper.Login(
                self.Root(),
                globalComponent.GlobalConfig.Address,
                account,
                "123456").NoContext();

            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
