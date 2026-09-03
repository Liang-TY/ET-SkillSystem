namespace ET
{
    /// <summary>禁移动开；value 来自 SkillParams/actions/，正值表示开启。</summary>
    [ActionId(ActionIds.ForbidMoveOn)]
    public sealed class ForbidMoveOnAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.ForbidMoveOn;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            ctx.OwnerForbidMove(param == null || (int)param.Value > 0);
        }
    }

    /// <summary>禁移动关；value 来自 SkillParams/actions/，负值表示关闭。</summary>
    [ActionId(ActionIds.ForbidMoveOff)]
    public sealed class ForbidMoveOffAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.ForbidMoveOff;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            ctx.OwnerForbidMove(param == null || (int)param.Value < 0);
        }
    }
}
