using TrueSync;

namespace ET
{
    /// <summary>裂波斩；过程和参数位于 SkillParams/skills/vaneslash.json。</summary>
    [SkillId(SkillIds.VaneSlash)]
    public sealed class VaneSlashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.VaneSlash;
    }
}
