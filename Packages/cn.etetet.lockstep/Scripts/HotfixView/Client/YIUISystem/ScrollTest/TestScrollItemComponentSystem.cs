using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2026.7.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(TestScrollItemComponent))]
    public static partial class TestScrollItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TestScrollItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TestScrollItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(TestScrollItemComponent.OnEventClickInvoke)]
        private static void OnEventClickInvoke(this TestScrollItemComponent self)
        {
            Log.Info($"Item button clicked, index: {self.u_ComU_DataIndex.text}");
        }
        
        [YIUIInvoke(TestScrollItemComponent.OnEventSelectInvoke)]
        private static void OnEventSelectInvoke(this TestScrollItemComponent self)
        {

        }
        #endregion YIUIEvent结束
    }
}
