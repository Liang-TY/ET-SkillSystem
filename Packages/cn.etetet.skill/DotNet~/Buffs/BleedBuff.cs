using TrueSync;

namespace ET
{
    /// <summary>出血 Buff；参数位于 SkillParams/buffs/bleed.json。</summary>
    [BuffId(BuffIds.Bleed)]
    public class BleedBuff : ParametricBuffDefinition
    {
        public override int ConfiguredBuffId => BuffIds.Bleed;
    }
}
