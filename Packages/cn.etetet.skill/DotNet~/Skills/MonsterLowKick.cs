using TrueSync;

namespace ET
{
    /// <summary>班图女战士下段踢；过程和参数位于 SkillParams/skills/monsterlowkick.json。</summary>
    [SkillId(SkillIds.MonsterLowKick)]
    public sealed class MonsterLowKick : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.MonsterLowKick;
    }
}
