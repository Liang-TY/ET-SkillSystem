using TrueSync;

namespace ET
{
    /// <summary>上挑；过程和参数位于 SkillParams/skills/upperslash.json。</summary>
    [SkillId(SkillIds.UpperSlash)]
    public sealed class UpperSlashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.UpperSlash;
    }
}
