using TrueSync;

namespace ET
{
    /// <summary>眩晕 Buff；参数位于 SkillParams/buffs/stun.json。</summary>
    [BuffId(BuffIds.Stun)]
    public class StunBuff : ParametricBuffDefinition
    {
        public override int ConfiguredBuffId => BuffIds.Stun;
    }
}
