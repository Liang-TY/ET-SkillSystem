namespace ET
{
    /// <summary>命中挂出血；buffId 和显示名称来自 SkillParams/actions/.</summary>
    [ActionId(ActionIds.AddBleedBuff)]
    public sealed class AddBleedBuffAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.AddBleedBuff;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            ctx.AddBuffToOwner(param?.BuffId ?? BuffIds.Bleed);
        }
    }
}
