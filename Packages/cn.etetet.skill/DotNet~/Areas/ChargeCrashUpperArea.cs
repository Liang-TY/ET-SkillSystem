using TrueSync;

namespace ET
{
    /// <summary>破军升龙击上挑区域；参数位于 SkillParams/areas/chargecrashupper.json。</summary>
    [AreaId(AreaIds.ChargeCrashUpper)]
    public class ChargeCrashUpperArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.ChargeCrashUpper;
    }
}
