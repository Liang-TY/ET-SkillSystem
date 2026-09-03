using TrueSync;

namespace ET
{
    /// <summary>银光落刃；过程和参数位于 SkillParams/skills/ashenfork.json。</summary>
    [SkillId(SkillIds.AshenFork)]
    public sealed class AshenForkSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.AshenFork;
    }
}
