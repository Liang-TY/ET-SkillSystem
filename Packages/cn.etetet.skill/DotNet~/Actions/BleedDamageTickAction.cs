namespace ET
{
    /// <summary>出血 Tick 伤害（BleedBuff 每 1 秒触发；与燃烧同构的另一种 DOT）。</summary>
    [ActionId(ActionIds.BleedDamageTick)]
    public class BleedDamageTickAction : LSAction
    {
        private const int Damage = 15;

        public override void Run(LSActionContext ctx)
        {
            ctx.DamageOwner(Damage);
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} 出血伤害{Damage}，HP={ctx.GetOwnerHp()}");
        }
    }
}
