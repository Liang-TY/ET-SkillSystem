using TrueSync;

namespace ET
{
    /// <summary>怒气爆发前段；参数位于 SkillParams/areas/bloodblastpre.json。</summary>
    [AreaId(AreaIds.BloodBlastPre)]
    public class BloodBlastPreArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.BloodBlastPre;
    }
}
