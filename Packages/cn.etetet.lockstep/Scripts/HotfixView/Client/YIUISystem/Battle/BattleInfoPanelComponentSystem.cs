using System;
using TrueSync;
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
    [FriendOf(typeof(BattleInfoPanelComponent))]
    public static partial class BattleInfoPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BattleInfoPanelComponent self)
        {
            self.HideMonster();   // 决策：不打怪就隐藏
        }

        [EntitySystem]
        private static void Destroy(this BattleInfoPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BattleInfoPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束

        /// <summary>更新怪物血条（含显示）</summary>
        public static void UpdateMonster(this BattleInfoPanelComponent self, string name, FP hp, FP maxHp)
        {
            self.u_ComTextMonsterName.text = name;
            self.u_ComTextMonsterName.gameObject.SetActive(true);
            self.u_ComImgMonsterHp.gameObject.SetActive(true);
            float ratio = maxHp > FP.Zero ? (hp / maxHp).AsFloat() : 0f;
            self.u_ComImgMonsterHp.fillAmount = ratio;
        }

        /// <summary>隐藏怪物血条（面板打开默认）</summary>
        public static void HideMonster(this BattleInfoPanelComponent self)
        {
            self.u_ComTextMonsterName.gameObject.SetActive(false);
            self.u_ComImgMonsterHp.gameObject.SetActive(false);
        }

        /// <summary>飘字挂载点（面板所在 Canvas）</summary>
        public static Transform GetFloatRoot(this BattleInfoPanelComponent self)
        {
            return self.u_ComTextPlayerName.canvas.transform;
        }
    }
}
