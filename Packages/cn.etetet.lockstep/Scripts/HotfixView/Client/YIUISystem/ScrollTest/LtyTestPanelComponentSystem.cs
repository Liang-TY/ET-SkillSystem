using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    [FriendOf(typeof(LtyTestPanelComponent))]
    [FriendOf(typeof(TestScrollItemComponent))]
    public static partial class LtyTestPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LtyTestPanelComponent self)
        {
            var scrollRect = self.u_ComLoopScrollHorizontal.GetComponent<LoopHorizontalScrollRect>() as LoopScrollRect;
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(
                scrollRect,
                typeof(TestScrollItemComponent),
                "u_EventSelect");
        }

        [EntitySystem]
        private static void Destroy(this LtyTestPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LtyTestPanelComponent self)
        {
            await ETTask.CompletedTask;
            var list = new List<int>();
            for (int i = 0; i < 100; i++) list.Add(i);
            self.Loop.ClearSelect();
            self.Loop.SetDataRefresh(list, 0).NoContext();
            return true;
        }

        [EntitySystem]
        private static void YIUILoopRenderer(this LtyTestPanelComponent self, TestScrollItemComponent item, int data, int index, bool select)
        {
            item.u_ComU_DataIndex.text = index.ToString();
            item.u_ComU_DataName.text = $"Item_{index}";
        }

        [EntitySystem]
        private static void YIUILoopOnClick(this LtyTestPanelComponent self, TestScrollItemComponent item, int data, int index, bool select)
        {
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
