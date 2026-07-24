using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [EntitySystemOf(typeof(TestScrollItemComponent))]
    public static partial class TestScrollItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TestScrollItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this TestScrollItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this TestScrollItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_ComU_DataIndex = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Text>("u_ComU_DataIndex");
            self.u_ComU_DataName = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Text>("u_ComU_DataName");
            self.u_ComU_DataSelect = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComU_DataSelect");
            self.u_ComBtnAction = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComBtnAction");
            self.u_EventSelect = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventSelect");
            self.u_EventSelectHandle = self.u_EventSelect.Add(self,TestScrollItemComponent.OnEventSelectInvoke);
            self.u_EventClick = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventClick");
            self.u_EventClickHandle = self.u_EventClick.Add(self,TestScrollItemComponent.OnEventClickInvoke);

        }
    }
}
