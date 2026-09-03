using TrueSync;

namespace ET
{
    /// <summary>裂波斩波轮区域；参数位于 SkillParams/areas/vaneslashwheel.json。</summary>
    [AreaId(AreaIds.VaneSlashWheel)]
    public class VaneSlashWheelArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.VaneSlashWheel;
    }
}
