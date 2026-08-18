namespace ET
{
    /// <summary>燃烧 Tick 伤害（BurnBuff 每 1 秒触发）。</summary>
    [ActionId(ActionIds.FireDamageTick)]
    public class FireDamageTickAction : LSAction
    {
        private const int Damage = 10;

        public override void Run(LSActionContext ctx)
        {
            ctx.DamageOwner(Damage);
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} 燃烧伤害{Damage}，HP={ctx.GetOwnerHp()}");
        }
    }
}
