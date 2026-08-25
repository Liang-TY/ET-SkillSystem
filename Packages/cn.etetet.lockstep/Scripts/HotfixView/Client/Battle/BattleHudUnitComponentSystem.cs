using TrueSync;

namespace ET.Client
{
    [EntitySystemOf(typeof(BattleHudUnitComponent))]
    [FriendOf(typeof(BattleHudUnitComponent))]
    public static partial class BattleHudUnitComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleHudUnitComponent self, BattleInfoPanelComponent panel)
        {
            self.m_Panel = panel;
        }

        [EntitySystem]
        private static void Destroy(this BattleHudUnitComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this BattleHudUnitComponent self)
        {
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
                DamageFloatHelper.Show(panel.GetFloatRoot(), self.LastMonsterHp - hpFloat);
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
    }
}
