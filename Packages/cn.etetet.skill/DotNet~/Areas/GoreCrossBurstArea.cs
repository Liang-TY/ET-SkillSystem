using TrueSync;

namespace ET
{
    /// <summary>十字斩三联爆发区域；参数位于 SkillParams/areas/gorecrossburst.json。</summary>
    [AreaId(AreaIds.GoreCrossBurst)]
    public class GoreCrossBurstArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.GoreCrossBurst;
    }
}
