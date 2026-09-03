using TrueSync;

namespace ET
{
    /// <summary>怒气爆发；过程和参数位于 SkillParams/skills/bloodblast.json。</summary>
    [SkillId(SkillIds.BloodBlast)]
    public sealed class BloodBlastSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.BloodBlast;
    }
}
