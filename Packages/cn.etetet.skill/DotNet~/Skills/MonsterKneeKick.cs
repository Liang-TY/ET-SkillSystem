using TrueSync;

namespace ET
{
    /// <summary>班图女战士膝踢；过程和参数位于 SkillParams/skills/monsterkneekick.json。</summary>
    [SkillId(SkillIds.MonsterKneeKick)]
    public sealed class MonsterKneeKick : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.MonsterKneeKick;
    }
}
