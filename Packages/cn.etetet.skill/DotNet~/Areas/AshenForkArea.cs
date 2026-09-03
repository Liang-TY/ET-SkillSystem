using TrueSync;

namespace ET
{
    /// <summary>银光落刃落地冲击波；参数位于 SkillParams/areas/ashenfork.json。</summary>
    [AreaId(AreaIds.AshenFork)]
    public class AshenForkArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.AshenFork;
    }
}
