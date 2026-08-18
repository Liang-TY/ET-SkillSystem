namespace ET
{
    /// <summary>禁移动开（眩晕 Buff 的 AddActions）。</summary>
    [ActionId(ActionIds.ForbidMoveOn)]
    public class ForbidMoveOnAction : LSAction
    {
        public override void Run(LSActionContext ctx)
        {
            ctx.OwnerForbidMove(true);
            // TODO 诊断日志（眩晕验证完删）
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} ForbidMove=ON");
        }
    }

    /// <summary>禁移动关（眩晕 Buff 的 RemoveActions，与 On 成对）。</summary>
    [ActionId(ActionIds.ForbidMoveOff)]
    public class ForbidMoveOffAction : LSAction
    {
        public override void Run(LSActionContext ctx)
        {
            ctx.OwnerForbidMove(false);
            // TODO 诊断日志（眩晕验证完删）
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} ForbidMove=OFF");
        }
    }
}
