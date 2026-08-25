using System;
using TrueSync;
using UnityEngine;
using UnityEngine.UI;
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

            // 保险：确保两条血条 Image 为 Filled 类型（fillAmount 只在 Filled 下有视觉效果）
            self.u_ComImgPlayerHp.type = Image.Type.Filled;
            self.u_ComImgPlayerHp.fillMethod = Image.FillMethod.Horizontal;
            self.u_ComImgMonsterHp.type = Image.Type.Filled;
            self.u_ComImgMonsterHp.fillMethod = Image.FillMethod.Horizontal;
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

        /// <summary>显示怪物信息（名字+血条框架）</summary>
        public static void ShowMonster(this BattleInfoPanelComponent self, string name)
        {
            self.u_ComTextMonsterName.text = name;
            self.u_ComTextMonsterName.gameObject.SetActive(true);
            self.u_ComImgMonsterHp.gameObject.SetActive(true);
        }

        /// <summary>设置玩家血条比例（0~1）</summary>
        public static void SetPlayerHpRatio(this BattleInfoPanelComponent self, float ratio)
        {
            self.u_ComImgPlayerHp.fillAmount = Mathf.Clamp01(ratio);
        }

        /// <summary>设置怪物血条比例（0~1，供平滑缓动）</summary
        public static void SetMonsterHpRatio(this BattleInfoPanelComponent self, float ratio)
        {
            self.u_ComImgMonsterHp.fillAmount = Mathf.Clamp01(ratio);
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
