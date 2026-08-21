namespace ET
{
    /// <summary>
    /// 近战命中效果：伤害 + 受击硬直 + 受击动画。
    /// 参数从来源 HitReaction 读（SkillLogic/AreaDefinition/BulletDefinition 各自 override，
    /// DNF .atk 同构）；未 override 的来源 = 默认 50/500，与旧硬编码行为一致。
    /// </summary>
    [ActionId(ActionIds.MeleeHit)]
    public class MeleeHitAction : LSAction
    {
        public override void Run(LSActionContext ctx)
        {
            HitReaction reaction = ctx.GetSourceHitReaction();
            ctx.DamageOwner(reaction.Damage);
            ctx.SetOwnerHitstun(reaction.HitstunMs);
            int hurtAnim = ctx.GetOwnerHurtAnimId();
            if (hurtAnim > 0) ctx.PlayOwnerAnim(hurtAnim);   // 受击者自己的受击动画（DNF sq_GetDamageAni 同构）

            Log.Info($"[Combat] 帧{ctx.FrameNo} unit{ctx.GetSourceId()} 命中 unit{ctx.GetOwnerId()}，" +
                     $"伤害{reaction.Damage}，HP={ctx.GetOwnerHp()} hitstun={reaction.HitstunMs}ms");
        }
    }
}
