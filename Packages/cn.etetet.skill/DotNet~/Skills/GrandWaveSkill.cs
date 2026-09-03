using TrueSync;

namespace ET
{
    /// <summary>邪光斩；过程和参数位于 SkillParams/skills/grandwave.json。</summary>
    [SkillId(SkillIds.GrandWave)]
    public sealed class GrandWaveSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.GrandWave;
    }
}
