using TrueSync;

namespace ET
{
    /// <summary>怒气爆发外圈；参数位于 SkillParams/areas/bloodblastouter.json。</summary>
    [AreaId(AreaIds.BloodBlastOuter)]
    public class BloodBlastArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.BloodBlastOuter;
    }
}
