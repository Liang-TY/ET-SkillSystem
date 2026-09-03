using TrueSync;

namespace ET
{
    /// <summary>拔刀斩；过程和参数位于 SkillParams/skills/momentaryslash.json。</summary>
    [SkillId(SkillIds.MomentarySlash)]
    public sealed class MomentarySlashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.MomentarySlash;
    }
}
