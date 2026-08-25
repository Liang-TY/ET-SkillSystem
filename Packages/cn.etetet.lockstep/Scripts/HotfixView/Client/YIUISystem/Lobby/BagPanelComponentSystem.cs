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
    [FriendOf(typeof(BagPanelComponent))]
    public static partial class BagPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BagPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BagPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BagPanelComponent self)
        {
            // DemoUI：清旧格 → 20 个假 item 占位（TestScrollItem prefab，正式物品系统后续接）
            for (int i = self.u_ComGridRoot.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(self.u_ComGridRoot.GetChild(i).gameObject);

            GameObject prefab = await self.Root().GetComponent<ResourcesLoaderComponent>()
                .LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/ScrollTest/TestScrollItem.prefab");
            if (prefab == null)
            {
                Log.Error("[DemoUI] BagPanel 找不到 TestScrollItem.prefab");
                return false;
            }

            for (int i = 0; i < 20; i++)
            {
                GameObject item = UnityEngine.Object.Instantiate(prefab, self.u_ComGridRoot);
                item.name = $"Item_{i}";
            }

            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BagPanelComponent.OnEventCloseInvoke)]
        private static async ETTask OnEventCloseInvoke(this BagPanelComponent self)
        {
            await self.Root().YIUIMgr().ClosePanelAsync<BagPanelComponent>();
        }

        
        [YIUIInvoke(BagPanelComponent.OnEventWindowDragInvoke)]
        private static async ETTask OnEventWindowDragInvoke(this BagPanelComponent self)
        {
            // 拖动窗口（按下标题区）：WindowDragComponent 逐帧移动，松手结束
            WindowDragHelper.Begin(self.Root(), self.u_ComWindow);
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
