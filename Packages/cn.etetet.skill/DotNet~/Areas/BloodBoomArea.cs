using TrueSync;

namespace ET
{
    /// <summary>浴血之怒爆炸区域；参数位于 SkillParams/areas/bloodboom.json。</summary>
    [AreaId(AreaIds.BloodBoom)]
    public class BloodBoomArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.BloodBoom;
    }
}
