using TrueSync;

namespace ET
{
    /// <summary>月光斩；过程和参数位于 SkillParams/skills/moonlightslash.json。</summary>
    [SkillId(SkillIds.MoonlightSlash)]
    public sealed class MoonlightSlashSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.MoonlightSlash;
    }
}
