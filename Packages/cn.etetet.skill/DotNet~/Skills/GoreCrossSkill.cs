using TrueSync;

namespace ET
{
    /// <summary>十字斩；过程和参数位于 SkillParams/skills/gorecross.json。</summary>
    [SkillId(SkillIds.GoreCross)]
    public sealed class GoreCrossSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.GoreCross;
    }
}
