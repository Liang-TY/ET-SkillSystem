namespace ET
{
    /// <summary>禁移动开（眩晕 Buff 的 AddActions）。</summary>
    [ActionId(ActionIds.ForbidMoveOn)]
    public class ForbidMoveOnAction : LSAction
    {
        public override void Run(LSActionContext ctx) => ctx.OwnerForbidMove(true);
    }

    /// <summary>禁移动关（眩晕 Buff 的 RemoveActions，与 On 成对）。</summary>
    [ActionId(ActionIds.ForbidMoveOff)]
    public class ForbidMoveOffAction : LSAction
    {
        public override void Run(LSActionContext ctx) => ctx.OwnerForbidMove(false);
    }
}
