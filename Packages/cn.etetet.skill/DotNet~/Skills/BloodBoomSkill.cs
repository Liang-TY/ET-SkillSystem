using TrueSync;

namespace ET
{
    /// <summary>浴血之怒；过程和参数位于 SkillParams/skills/bloodboom.json。</summary>
    [SkillId(SkillIds.BloodBoom)]
    public sealed class BloodBoomSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.BloodBoom;
    }
}
