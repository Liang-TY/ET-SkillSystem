using TrueSync;

namespace ET
{
    /// <summary>裂波斩终结区域；参数位于 SkillParams/areas/vaneslashfinal.json。</summary>
    [AreaId(AreaIds.VaneSlashFinal)]
    public class VaneSlashFinalArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.VaneSlashFinal;
    }
}
