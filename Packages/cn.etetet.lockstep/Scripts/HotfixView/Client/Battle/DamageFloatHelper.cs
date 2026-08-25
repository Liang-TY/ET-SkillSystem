using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 伤害飘字（DemoUI 决策：v1 逻辑层创建，Text + DOTween 上飘渐隐，0.8s 后自毁）。
    /// 挂载点 = BattleInfo 面板 Canvas（屏幕坐标与面板一致，中上区域）。
    /// </summary>
    public static class DamageFloatHelper
    {
        public static void Show(Transform parent, float damage)
        {
            if (parent == null || damage <= 0f) return;

            var go = new GameObject("DamageFloat");
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = new Vector2(UnityEngine.Random.Range(-80f, 80f), 260f);
            rect.sizeDelta = new Vector2(160f, 50f);

            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 34;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.85f, 0.3f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = ((int)damage).ToString();

            rect.DOAnchorPosY(rect.anchoredPosition.y + 90f, 0.8f).SetEase(Ease.OutCubic);
            text.DOFade(0f, 0.8f).SetDelay(0.2f).OnComplete(() => UnityEngine.Object.Destroy(go));
        }
    }
}
