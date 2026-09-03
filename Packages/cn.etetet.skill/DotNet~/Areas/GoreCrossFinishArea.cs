using TrueSync;

namespace ET
{
    /// <summary>十字斩追击区域；参数位于 SkillParams/areas/gorecrossfinish.json。</summary>
    [AreaId(AreaIds.GoreCrossFinish)]
    public class GoreCrossFinishArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.GoreCrossFinish;
    }
}
