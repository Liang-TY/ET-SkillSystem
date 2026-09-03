using TrueSync;

namespace ET
{
    /// <summary>拔刀斩视觉波；参数位于 SkillParams/areas/momentaryslashwave.json。</summary>
    [AreaId(AreaIds.MomentarySlashWave)]
    public class MomentarySlashWaveArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.MomentarySlashWave;
    }
}
