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
        
        [YIUIInvoke(LoginPanelComponent.OnEventLoginInvoke)]
        private static async ETTask OnEventLoginInvoke(this LoginPanelComponent self)
        {
            // DemoUI：账号密码登录（走现有 login 流程；LoginFinish 事件负责关面板开选角）
            GlobalComponent global = self.Root().GetComponent<GlobalComponent>();
            LoginHelper.Login(
                self.Root(),
                global.GlobalConfig.Address,
                self.u_ComInputAccount.text,
                self.u_ComInputPassword.text).NoContext();
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
