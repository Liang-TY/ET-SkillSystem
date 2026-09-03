using TrueSync;

namespace ET
{
    /// <summary>破军升龙击；过程和参数位于 SkillParams/skills/chargecrash.json。</summary>
    [SkillId(SkillIds.ChargeCrash)]
    public sealed class ChargeCrashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.ChargeCrash;
    }
}
