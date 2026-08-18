namespace ET
{
    /// <summary>命中挂燃烧（NormalAttack.HitActions 用；source=攻击方）。</summary>
    [ActionId(ActionIds.AddBurnBuff)]
    public class AddBurnBuffAction : LSAction
    {
        public override void Run(LSActionContext ctx) => ctx.AddBuffToOwner(BuffIds.Burn);
    }
}
