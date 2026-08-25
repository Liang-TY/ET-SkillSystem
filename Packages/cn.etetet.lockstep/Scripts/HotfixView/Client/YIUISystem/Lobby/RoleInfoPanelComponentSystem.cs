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
            // DemoUI：v1 展示 UnitConfig 基础字段（战斗数值 LSNumeric 待接入）
            UnitConfig config = UnitConfigCategory.Instance.GetOne();
            self.u_ComTextInfo.text = config == null
                ? "（无角色配置）"
                : "名字：" + config.Name + "\n类型：" + config.Type + "\n身高：" + config.Height + "\n\n（战斗数值待接入）";

            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(RoleInfoPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this RoleInfoPanelComponent self)
        {
            await self.Root().YIUIMgr().ClosePanelAsync<RoleInfoPanelComponent>();
        }

        
        [YIUIInvoke(RoleInfoPanelComponent.OnEventWindowDragInvoke)]
        private static async ETTask OnEventWindowDragInvoke(this RoleInfoPanelComponent self)
        {
            // 拖动窗口（按下标题区）：WindowDragComponent 逐帧移动，松手结束
            WindowDragHelper.Begin(self.Root(), self.u_ComWindow);
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
