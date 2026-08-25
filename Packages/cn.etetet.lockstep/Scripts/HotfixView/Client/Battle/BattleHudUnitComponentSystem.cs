using TrueSync;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 战斗 HUD：怪物血条轮询 + 伤害飘字（手动帧驱动，无 DOTween 依赖）。
    /// 飘字状态在 BattleHudUnitComponent 实体上（Hotfix 无状态红线）。
    /// </summary>
    [EntitySystemOf(typeof(BattleHudUnitComponent))]
    [FriendOf(typeof(BattleHudUnitComponent))]
    public static partial class BattleHudUnitComponentSystem
    {
        private const float FloatDuration = 0.8f;
        private const float FloatFadeStart = 0.2f;
        private const float FloatSpeed = 120f;

        [EntitySystem]
        private static void Awake(this BattleHudUnitComponent self, BattleInfoPanelComponent panel)
        {
            self.m_Panel = panel;
        }

        [EntitySystem]
        private static void Destroy(this BattleHudUnitComponent self)
        {
            // 残留文本随面板 Canvas 销毁，只清登记
            self.FloatTexts.Clear();
            self.FloatRects.Clear();
            self.FloatElapsed.Clear();

            // 只隐藏血条元素，不关面板（用户决策：面板由 TownSceneInitFinish 统一收口，
            // Room 销毁时只做元素级清场——竞态安全兜底）
            self.Panel?.HideMonster();
        }

        [EntitySystem]
        private static void Update(this BattleHudUnitComponent self)
        {
            TickFloats(self);

            BattleInfoPanelComponent panel = self.Panel;
            if (panel == null) return;

            Room room = self.GetParent<Room>();
            LSUnitComponent unitComponent = room.LSWorld?.GetComponent<LSUnitComponent>();
            if (unitComponent == null) return;

            LSUnit monster = FindFirstMonster(unitComponent);
            if (monster == null) return;   // 全灭后不再更新，血条留最后状态

            LSNumericComponent numeric = monster.GetComponent<LSNumericComponent>();
            if (numeric == null) return;
            FP hp = numeric.Get(NumericType.Hp);
            FP maxHp = numeric.Get(NumericType.MaxHp);
            float hpFloat = hp.AsFloat();

            if (monster.Id != self.LastMonsterId)
            {
                self.LastMonsterId = monster.Id;
                self.LastMonsterHp = hpFloat;
                self.MonsterShown = false;
            }

            if (hpFloat < self.LastMonsterHp)
            {
                SpawnFloat(self, panel.GetFloatRoot(), self.LastMonsterHp - hpFloat);
                self.MonsterShown = true;   // 受过伤才显示（不打怪就隐藏——用户决策）
            }
            self.LastMonsterHp = hpFloat;

            if (self.MonsterShown)
                panel.UpdateMonster("怪物", hp, maxHp);
        }

        private static LSUnit FindFirstMonster(LSUnitComponent unitComponent)
        {
            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is LSUnit unit && unit.GetComponent<LSMonsterAIComponent>() != null)
                    return unit;
            }

            return null;
        }

        /// <summary>生成飘字（上飘 120px/s，0.2s 起渐隐，0.8s 自毁）</summary>
        private static void SpawnFloat(this BattleHudUnitComponent self, Transform parent, float damage)
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

            self.FloatTexts.Add(text);
            self.FloatRects.Add(rect);
            self.FloatElapsed.Add(0f);
        }

        private static void TickFloats(this BattleHudUnitComponent self)
        {
            if (self.FloatTexts.Count == 0) return;
            float dt = Time.deltaTime;

            for (int i = self.FloatTexts.Count - 1; i >= 0; i--)
            {
                Text text = self.FloatTexts[i];
                if (text == null)   // 面板关闭时随之销毁
                {
                    RemoveAt(self, i);
                    continue;
                }

                self.FloatElapsed[i] += dt;
                Vector2 pos = self.FloatRects[i].anchoredPosition;
                pos.y += FloatSpeed * dt;
                self.FloatRects[i].anchoredPosition = pos;

                float elapsed = self.FloatElapsed[i];
                if (elapsed > FloatFadeStart)
                {
                    Color c = text.color;
                    c.a = Mathf.Clamp01(1f - (elapsed - FloatFadeStart) / (FloatDuration - FloatFadeStart));
                    text.color = c;
                }

                if (elapsed >= FloatDuration)
                {
                    UnityEngine.Object.Destroy(text.gameObject);
                    RemoveAt(self, i);
                }
            }
        }

        private static void RemoveAt(this BattleHudUnitComponent self, int index)
        {
            self.FloatTexts.RemoveAt(index);
            self.FloatRects.RemoveAt(index);
            self.FloatElapsed.RemoveAt(index);
        }
    }
}
