using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 伤害飘字（DemoUI 决策：v1 逻辑层创建，手动每帧驱动上飘渐隐）。
    /// 不用 DOTween——ET.HotfixView 程序集未引用它（0018 编译实证）。
    /// Tick 由 BattleHudUnitComponentSystem.Update 驱动；Clear 在战斗 HUD 销毁时清场。
    /// </summary>
    public static class DamageFloatHelper
    {
        private class FloatItem
        {
            public Text Text;
            public RectTransform Rect;
            public float Elapsed;
        }

        [StaticField]
        private static readonly List<FloatItem> Items = new List<FloatItem>();

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

            Items.Add(new FloatItem { Text = text, Rect = rect, Elapsed = 0f });
        }

        /// <summary>每帧驱动：上飘 120px/s，0.2s 后渐隐，0.8s 销毁</summary>
        public static void Tick()
        {
            if (Items.Count == 0) return;
            float dt = Time.deltaTime;

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                FloatItem item = Items[i];
                if (item.Text == null)   // 面板关闭时随之销毁
                {
                    Items.RemoveAt(i);
                    continue;
                }

                item.Elapsed += dt;
                Vector2 pos = item.Rect.anchoredPosition;
                pos.y += 120f * dt;
                item.Rect.anchoredPosition = pos;

                if (item.Elapsed > 0.2f)
                {
                    float alpha = Mathf.Clamp01(1f - (item.Elapsed - 0.2f) / 0.6f);
                    Color c = item.Text.color;
                    c.a = alpha;
                    item.Text.color = c;
                }

                if (item.Elapsed >= 0.8f)
                {
                    UnityEngine.Object.Destroy(item.Text.gameObject);
                    Items.RemoveAt(i);
                }
            }
        }

        /// <summary>战斗 HUD 销毁时清场（残留文本随面板 Canvas 销毁，只清登记）</summary>
        public static void Clear()
        {
            Items.Clear();
        }
    }
}
