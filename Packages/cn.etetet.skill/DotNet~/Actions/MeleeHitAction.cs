namespace ET
{
    /// <summary>
    /// 近战命中效果：伤害 + 受击硬直 + 受击动画（原 LSHitboxComponentSystem.ApplyHit 的硬编码搬家）。
    /// 数值暂 const（attack 表 luban 化时进表）。
    /// </summary>
    [ActionId(ActionIds.MeleeHit)]
    public class MeleeHitAction : LSAction
    {
        private const int Damage = 50;      // 伤害
        private const int HitstunMs = 500;  // 受击硬直

        public override void Run(LSActionContext ctx)
        {
            ctx.DamageOwner(Damage);
            ctx.SetOwnerHitstun(HitstunMs);
            int hurtAnim = ctx.GetOwnerHurtAnimId();
            if (hurtAnim > 0) ctx.PlayOwnerAnim(hurtAnim);   // 受击者自己的受击动画（DNF sq_GetDamageAni 同构）

            Log.Info($"[Combat] 帧{ctx.FrameNo} unit{ctx.GetSourceId()} 命中 unit{ctx.GetOwnerId()}，" +
                     $"伤害{Damage}，HP={ctx.GetOwnerHp()} hitstun={HitstunMs}ms");
        }
    }
}
