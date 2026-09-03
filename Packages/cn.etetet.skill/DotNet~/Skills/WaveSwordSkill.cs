using TrueSync;

namespace ET
{
    /// <summary>地裂·波动剑；过程和参数位于 SkillParams/skills/wavesword.json。</summary>
    [SkillId(SkillIds.WaveSword)]
    public sealed class WaveSwordSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.WaveSword;
    }
}
