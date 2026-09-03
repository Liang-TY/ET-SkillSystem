using TrueSync;

namespace ET
{
    /// <summary>班图女战士高踢；过程和参数位于 SkillParams/skills/monsterhighkick.json。</summary>
    [SkillId(SkillIds.MonsterHighKick)]
    public sealed class MonsterHighKick : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.MonsterHighKick;
    }
}
