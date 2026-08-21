namespace ET
{
    /// <summary>命中挂出血（BloodBoomArea.EnterActions 用；source=攻击方）。</summary>
    [ActionId(ActionIds.AddBleedBuff)]
    public class AddBleedBuffAction : LSAction
    {
        public override void Run(LSActionContext ctx) => ctx.AddBuffToOwner(BuffIds.Bleed);
    }
}
