using TrueSync;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 战斗 HUD：怪物血条轮询（平滑缓动）+ 伤害飘字（怪物世界坐标投影）。
    /// 飘字状态在 BattleHudUnitComponent 实体上（Hotfix 无状态红线）。
    /// </summary>
    [EntitySystemOf(typeof(BattleHudUnitComponent))]
    [FriendOf(typeof(BattleHudUnitComponent))]
    public static partial class BattleHudUnitComponentSystem
    {
        private const float FloatDuration = 0.8f;
        private const float FloatFadeStart = 0.2f;
        private const float FloatSpeed = 120f;
        private const float HpLerpSpeed = 10f;

        [EntitySystem]
        private static void Awake(this BattleHudUnitComponent self, BattleInfoPanelComponent panel)
        {
            self.m_Panel = panel;
        }

        [EntitySystem]
        private static void Destroy(this BattleHudUnitComponent self)
        {
            self.FloatTexts.Clear();
            self.FloatRects.Clear();
            self.FloatElapsed.Clear();

            // 只隐藏血条元素，不关面板（用户决策：面板由 TownSceneInitFinish 统一收口）
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

            LSUnit monster = unitComponent != null ? FindFirstMonster(unitComponent) : null;
            LSNumericComponent numeric = monster?.GetComponent<LSNumericComponent>();
            float hpFloat = numeric != null ? numeric.Get(NumericType.Hp).AsFloat() : 0f;
            float maxFloat = numeric != null ? numeric.Get(NumericType.MaxHp).AsFloat() : 0f;

            if (monster != null && numeric != null)
            {
                self.TargetHpRatio = maxFloat > 0f ? Mathf.Clamp01(hpFloat / maxFloat) : 0f;
                self.LastMonsterWorldPos = new Vector3(
                    monster.Position.x.AsFloat(),
                    monster.Position.y.AsFloat(),
                    monster.Position.z.AsFloat());

                if (monster.Id != self.LastMonsterId)
                {
                    self.LastMonsterId = monster.Id;
                    self.LastMonsterHp = hpFloat;
                    self.MonsterShown = false;
                    self.DisplayHpRatio = self.TargetHpRatio;
                    panel.ShowMonster("怪物");
                }
            }
            else if (self.MonsterShown)
            {
                self.TargetHpRatio = 0f;
            }

            if (monster != null && hpFloat < self.LastMonsterHp)
            {
                SpawnFloatAtMonster(self, panel, self.LastMonsterHp - hpFloat);
                self.MonsterShown = true;
            }
            self.LastMonsterHp = hpFloat;

            self.DisplayHpRatio = Mathf.Lerp(
                self.DisplayHpRatio, self.TargetHpRatio,
                Mathf.Clamp01(Time.deltaTime * HpLerpSpeed));
            if (Mathf.Abs(self.DisplayHpRatio - self.TargetHpRatio) < 0.005f)
                self.DisplayHpRatio = self.TargetHpRatio;

            if (self.MonsterShown)
            {
                panel.SetMonsterHpRatio(self.DisplayHpRatio);

                if (self.DisplayHpRatio <= 0.01f && monster == null)
                {
                    panel.HideMonster();
                    self.MonsterShown = false;
                }
            }
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

        private static Vector2 MonsterToCanvasPos(this BattleHudUnitComponent self, Canvas canvas)
        {
            Camera cam = Camera.main;
            if (cam == null || canvas == null) return new Vector2(0f, 260f);

            Vector3 screenPos = cam.WorldToScreenPoint(self.LastMonsterWorldPos);
            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null) return new Vector2(0f, 260f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvas.worldCamera, out Vector2 localPos);
            return localPos;
        }

        private static void SpawnFloatAtMonster(this BattleHudUnitComponent self, BattleInfoPanelComponent panel, float damage)
        {
            Transform parent = panel.GetFloatRoot();
            if (parent == null || damage <= 0f) return;

            Canvas canvas = parent.GetComponentInParent<Canvas>();
            Vector2 spawnPos = self.MonsterToCanvasPos(canvas);

            var go = new GameObject("DamageFloat");
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = new Vector2(spawnPos.x + Random.Range(-30f, 30f), spawnPos.y + 40f);
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
                if (text == null)
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
