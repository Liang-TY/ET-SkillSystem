using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 技能/战斗表现消费（Route B 轮询——标记只在逻辑帧内存活，读到 true 立刻表现）：
    /// - LSCast.JustStarted → 技能起手表现
    /// - LSCast.JustHit     → 命中火花/音效
    /// - LSCast.JustFinished→ 技能结束清理
    /// - LSBuff.JustAdded   → Buff 图标出现
    /// - LSBuff.JustRemoved → Buff 图标消失
    /// - HP diff            → 伤害数字（LastHp 缓存差值）
    /// 本阶段全部 Log 标记"UI 接入点"——血条/伤害数字/Buff 图标在这些位置写实现。
    /// </summary>
    [EntitySystemOf(typeof(LSCastViewComponent))]
    [FriendOf(typeof(LSCastViewComponent))]
    [ET.FriendOf(typeof(ET.LSCast))]
    [ET.FriendOf(typeof(ET.LSBuff))]
    [ET.FriendOf(typeof(ET.LSCombatComponent))]
    public static partial class LSCastViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSCastViewComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this LSCastViewComponent self)
        {
            Room room = self.GetParent<Room>();
            LSWorld world = room.LSWorld;
            LSUnitComponent unitComponent = world?.GetComponent<LSUnitComponent>();
            if (unitComponent == null) return;

            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is not LSUnit unit) continue;

                // ---- Cast 标记（技能起手/命中/结束）—— UI 接入点 ----
                LSCastComponent castComp = unit.GetComponent<LSCastComponent>();
                if (castComp != null)
                {
                    foreach (var ckv in castComp.Children)
                    {
                        if (ckv.Value is not LSCast cast || cast.Removing) continue;
                        if (cast.JustStarted)
                            Log.Info($"[SkillView] UI接入点·起手特效 unit={unit.Id} skill={cast.SkillId}");
                        if (cast.JustHit)
                            Log.Info($"[SkillView] UI接入点·命中火花 unit={unit.Id} skill={cast.SkillId} targets={cast.TargetIds.Count}");
                        if (cast.JustFinished)
                            Log.Info($"[SkillView] UI接入点·结束清理 unit={unit.Id} skill={cast.SkillId}");
                    }
                }

                // ---- Buff 标记（图标出现/消失）—— UI 接入点 ----
                LSBuffComponent buffComp = unit.GetComponent<LSBuffComponent>();
                if (buffComp != null)
                {
                    foreach (var bkv in buffComp.Children)
                    {
                        if (bkv.Value is not LSBuff buff || buff.Removing) continue;
                        if (buff.JustAdded)
                            Log.Info($"[SkillView] UI接入点·Buff图标出现 unit={unit.Id} buff={buff.ConfigId} stack={buff.Stack}");
                        if (buff.JustRemoved)
                            Log.Info($"[SkillView] UI接入点·Buff图标消失 unit={unit.Id} buff={buff.ConfigId}");
                    }
                }

                // ---- HP diff（伤害数字）—— UI 接入点 ----
                var numeric = unit.GetComponent<LSNumericComponent>();
                if (numeric != null)
                {
                    int hp = (int)numeric.Get(NumericType.Hp);
                    if (self.LastHp.TryGetValue(unit.Id, out int lastHp))
                    {
                        if (hp < lastHp)
                            Log.Info($"[SkillView] UI接入点·伤害数字 unit={unit.Id} -{lastHp - hp} HP={hp}");
                    }
                    self.LastHp[unit.Id] = hp;
                }
            }
        }
    }
}
