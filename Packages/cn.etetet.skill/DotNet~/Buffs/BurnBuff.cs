using TrueSync;

namespace ET
{
    /// <summary>燃烧 Buff；参数位于 SkillParams/buffs/burn.json。</summary>
    [BuffId(BuffIds.Burn)]
    public class BurnBuff : ParametricBuffDefinition
    {
        public override int ConfiguredBuffId => BuffIds.Burn;
    }
}
