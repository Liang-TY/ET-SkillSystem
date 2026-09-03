namespace ET
{
    /// <summary>出血 Tick 伤害；value 来自 SkillParams/actions/.</summary>
    [ActionId(ActionIds.BleedDamageTick)]
    public sealed class BleedDamageTickAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.BleedDamageTick;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            int damage = param == null ? 15 : (int)param.Value;
            ctx.DamageOwner(damage);
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} 出血伤害{damage}，HP={ctx.GetOwnerHp()}");
        }
    }
}
