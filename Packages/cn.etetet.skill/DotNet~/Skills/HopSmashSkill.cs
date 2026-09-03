using TrueSync;

namespace ET
{
    /// <summary>崩山击；过程和参数位于 SkillParams/skills/hopsmash.json。</summary>
    [SkillId(SkillIds.HopSmash)]
    public sealed class HopSmashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.HopSmash;
    }
}
