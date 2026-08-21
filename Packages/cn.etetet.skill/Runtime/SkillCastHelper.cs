using TrueSync;

namespace ET
{
    /// <summary>
    /// 施放门禁 + 创建（单一实现，两处消费）：
    /// ① Hotfix 侧 LSSkillComponentSystem.TryCast（缓冲消费路径）
    /// ② SkillContext.RestartCurrentSkill（连段取消路径——SkillContext 在 ET.Skill，不能调 Hotfix 的扩展方法否则循环依赖）。
    /// 放 ET.Skill 的普通静态类（无 [EntitySystemOf]——LSSkillComponent 的 System 生成锚点在 Hotfix 侧，避免生成器冲突）。
    /// </summary>
    public static class SkillCastHelper
    {
        /// <summary>三重门禁（硬直/在技/冷却）→ 通过则创建 LSCast（内部调 OnCast）</summary>
        public static bool TryCast(LSUnit unit, int skillId)
        {
            SkillLogic logic = SkillLoader.Get(skillId);
            if (logic == null) return false;

            // 受击硬直中不能施放（缓冲持有，硬直结束立刻放）
            LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();
            if (combat != null && combat.HitstunTimer > 0) return false;

            // 在技中不能施放（连段取消走 SkillContext.RestartCurrentSkill）
            LSCastComponent castComp = unit.GetComponent<LSCastComponent>();
            if (castComp == null) return false;
            if (castComp.GetActiveCast() != null) return false;

            // 施放门槛：最低自身 HP 百分比（DNF checkExecutableSkill 同构；不满足不进 CD 不建 cast）
            if (logic.MinCastHpPct > FP.Zero)
            {
                LSNumericComponent num = unit.GetComponent<LSNumericComponent>();
                FP hp = num?.Get(NumericType.Hp) ?? FP.Zero;
                FP maxHp = num?.Get(NumericType.MaxHp) ?? FP.Zero;
                if (hp * 100 < maxHp * logic.MinCastHpPct)
                {
                    Log.Info($"[Skill] unit{unit.Id} 技能{skillId} HP 不足（{hp}/{maxHp} < {logic.MinCastHpPct}%），拒绝施放");
                    return false;
                }
            }

            LSSkillComponent skill = unit.GetComponent<LSSkillComponent>();
            if (skill != null && skill.Cooldowns.TryGetValue(skillId, out int remain) && remain > 0)
            {
                Log.Info($"[Skill] unit{unit.Id} 技能{skillId} 冷却中，剩余{remain}ms");
                return false;
            }

            // CD 双机制（DNF 实证）：默认 TryCast 即 CD；ManualCooldown 多段技能 OnEnd 才起
            if (skill != null && logic.CooldownMs > 0 && !logic.ManualCooldown)
            {
                skill.Cooldowns[skillId] = logic.CooldownMs;
            }

            castComp.Create(unit, skillId);
            return true;
        }
    }
}
