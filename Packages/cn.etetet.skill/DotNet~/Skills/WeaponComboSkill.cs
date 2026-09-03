using TrueSync;

namespace ET
{
    /// <summary>里·鬼剑术；过程和参数位于 SkillParams/skills/weaponcombo.json。</summary>
    [SkillId(SkillIds.WeaponCombo)]
    public sealed class WeaponComboSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.WeaponCombo;
    }
}
