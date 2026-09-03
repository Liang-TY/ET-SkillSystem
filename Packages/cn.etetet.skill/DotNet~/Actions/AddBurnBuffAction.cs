namespace ET
{
    /// <summary>命中挂燃烧；buffId 和显示名称来自 SkillParams/actions/.</summary>
    [ActionId(ActionIds.AddBurnBuff)]
    public sealed class AddBurnBuffAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.AddBurnBuff;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            ctx.AddBuffToOwner(param?.BuffId ?? BuffIds.Burn);
        }
    }
}
