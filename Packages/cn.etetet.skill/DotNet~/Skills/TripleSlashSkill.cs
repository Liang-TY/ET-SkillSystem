using TrueSync;

namespace ET
{
    /// <summary>三段斩；过程和参数位于 SkillParams/skills/tripleslash.json。</summary>
    [SkillId(SkillIds.TripleSlash)]
    public sealed class TripleSlashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.TripleSlash;
    }
}
