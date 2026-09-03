using TrueSync;

namespace ET
{
    /// <summary>三段斩终段区域；参数位于 SkillParams/areas/tripleslashfinish.json。</summary>
    [AreaId(AreaIds.TripleSlashFinish)]
    public class TripleSlashFinishArea : ParametricAreaDefinition
    {
        public override int ConfiguredAreaId => AreaIds.TripleSlashFinish;
    }
}
