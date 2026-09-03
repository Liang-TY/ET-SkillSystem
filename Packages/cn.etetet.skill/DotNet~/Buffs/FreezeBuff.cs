using TrueSync;

namespace ET
{
    /// <summary>冰冻 Buff；参数位于 SkillParams/buffs/freeze.json。</summary>
    [BuffId(BuffIds.Freeze)]
    public class FreezeBuff : ParametricBuffDefinition
    {
        public override int ConfiguredBuffId => BuffIds.Freeze;
    }
}
