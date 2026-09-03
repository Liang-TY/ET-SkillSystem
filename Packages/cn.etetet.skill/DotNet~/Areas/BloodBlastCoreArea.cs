using TrueSync;

namespace ET
{
    /// <summary>怒气爆发内圈；参数位于 SkillParams/areas/bloodblastcore.json。</summary>
    [AreaId(AreaIds.BloodBlastCore)]
    public class BloodBlastCoreArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.BloodBlastCore;
    }
}
