using TrueSync;

namespace ET
{
    /// <summary>崩山击冲击波区域；参数位于 SkillParams/areas/hopsmashwave.json。</summary>
    [AreaId(AreaIds.HopSmashWave)]
    public class HopSmashWaveArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.HopSmashWave;
    }
}
