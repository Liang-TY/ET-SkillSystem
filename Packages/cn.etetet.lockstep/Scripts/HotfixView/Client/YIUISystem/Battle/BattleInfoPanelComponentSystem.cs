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
    [FriendOf(typeof(BattleInfoPanelComponent))]
    public static partial class BattleInfoPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BattleInfoPanelComponent self)
        {
            self.HideMonster();
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

        /// <summary>显示怪物信息（名字+血条容器）</summary>
        public static void ShowMonster(this BattleInfoPanelComponent self, string name)
        {
            self.u_ComTextMonsterName.text = name;
            self.u_ComTextMonsterName.gameObject.SetActive(true);
            self.u_ComMonsterHpBar.gameObject.SetActive(true);
        }

        /// <summary>设置玩家血条比例（0~1，锚点拉伸法：anchorMax.x 控制填充宽度）</summary>
        public static void SetPlayerHpRatio(this BattleInfoPanelComponent self, float ratio)
        {
            RectTransform fill = self.u_ComImgPlayerHp.transform as RectTransform;
            if (fill != null)
            {
                fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), fill.anchorMax.y);
            }
        }

        /// <summary>设置怪物血条比例（0~1，锚点拉伸法）</summary>
        public static void SetMonsterHpRatio(this BattleInfoPanelComponent self, float ratio)
        {
            RectTransform fill = self.u_ComImgMonsterHp.transform as RectTransform;
            if (fill != null)
            {
                fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), fill.anchorMax.y);
            }
        }

        /// <summary>隐藏怪物血条（面板打开默认，隐藏整个容器）</summary>
        public static void HideMonster(this BattleInfoPanelComponent self)
        {
            self.u_ComTextMonsterName.gameObject.SetActive(false);
            self.u_ComMonsterHpBar.gameObject.SetActive(false);
        }

        /// <summary>飘字挂载点（面板所在 Canvas）</summary>
        public static Transform GetFloatRoot(this BattleInfoPanelComponent self)
        {
            return self.u_ComTextPlayerName.canvas.transform;
        }
    }
}
