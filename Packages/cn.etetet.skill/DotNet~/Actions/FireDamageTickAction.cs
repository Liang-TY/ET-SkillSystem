namespace ET
{
    /// <summary>燃烧 Tick 伤害；value 来自 SkillParams/actions/.</summary>
    [ActionId(ActionIds.FireDamageTick)]
    public sealed class FireDamageTickAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.FireDamageTick;

        public override void Run(LSActionContext ctx)
        {
            ActionParam param = GetConfiguredParam();
            int damage = param == null ? 10 : (int)param.Value;
            ctx.DamageOwner(damage);
            Log.Info($"[Buff] 帧{ctx.FrameNo} unit{ctx.GetOwnerId()} 燃烧伤害{damage}，HP={ctx.GetOwnerHp()}");
        }
    }
}
