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
    [FriendOf(typeof(TownPlayerComponent))]
    [FriendOf(typeof(LoadingPanelComponent))]
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
            await self.Root().YIUIMgr().ClosePanelAsync<MapSelectPanelComponent>();
        }
        
        [YIUIInvoke(MapSelectPanelComponent.OnEventMap2Invoke)]
        private static async ETTask OnEventMap2Invoke(this MapSelectPanelComponent self)
        {
            await EnterBattleAsync(self);
        }
        
        [YIUIInvoke(MapSelectPanelComponent.OnEventMap1Invoke)]
        private static async ETTask OnEventMap1Invoke(this MapSelectPanelComponent self)
        {
            await EnterBattleAsync(self);
        }
        #endregion YIUIEvent结束

        /// <summary>
        /// DemoUI：选图进战斗（demo 唯一 TrainingRoom，两按钮占位同图）。
        /// 记住城镇位置（回城恢复）→ 关弹窗 → 开 Loading → 匹配（场景就绪后 LSSceneInitFinish 关 Loading）。
        /// </summary>
        private static async ETTask EnterBattleAsync(this MapSelectPanelComponent self)
        {
            TownPlayerComponent player = self.Root().GetComponent<Room>()?.GetComponent<TownPlayerComponent>();
            if (player != null) TownMemory.LastTownPosition = player.Position;

            await self.Root().YIUIMgr().ClosePanelAsync<MapSelectPanelComponent>();

            LoadingPanelComponent loading = await self.Root().YIUIRoot().OpenPanelAsync<LoadingPanelComponent>();
            if (loading != null) loading.u_ComTextSub.text = "正在进入战斗…";

            EnterMapHelper.Match(self.Root().Fiber()).NoContext();
        }
    }
}
