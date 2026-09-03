namespace ET
{
    /// <summary>
    /// 近战命中效果：伤害 + 受击硬直 + 受击动画。
    /// 数值来自命中来源的 HitReaction；ActionParams/MeleeHit 保留可编辑的节点身份。
    /// </summary>
    [ActionId(ActionIds.MeleeHit)]
    public sealed class MeleeHitAction : LSAction
    {
        public override int ConfiguredActionId => ActionIds.MeleeHit;

        public override void Run(LSActionContext ctx)
        {
            HitReaction reaction = ctx.GetSourceHitReaction();
            ctx.DamageOwner(reaction.Damage);
            bool superArmor = ctx.IsOwnerSuperArmor();
            if (!superArmor)
            {
                ctx.SetOwnerHitstun(reaction.HitstunMs);
                int hurtAnim = ctx.GetOwnerHurtAnimId();
                if (hurtAnim > 0) ctx.PlayOwnerAnim(hurtAnim);
                if (reaction.KnockbackX != 0 || reaction.LaunchY != 0)
                    ctx.LaunchOwner(reaction.KnockbackX, reaction.LaunchY);
            }

            if (reaction.ProcBuffId != 0
                && LSRng.Roll(ctx.FrameNo, ctx.GetOwnerId(), LSRng.PurposeProcStatus) < reaction.ProcChance)
            {
                ctx.AddBuffToOwner(reaction.ProcBuffId);
                Log.Info($"[Combat] unit{ctx.GetOwnerId()} 触发附加状态 buff{reaction.ProcBuffId}（{reaction.ProcChance}%）");
            }

            Log.Info($"[Combat] 帧{ctx.FrameNo} unit{ctx.GetSourceId()} 命中 unit{ctx.GetOwnerId()}，"
                     + $"伤害{reaction.Damage}，HP={ctx.GetOwnerHp()} hitstun={reaction.HitstunMs}ms"
                     + (reaction.KnockbackX != 0 || reaction.LaunchY != 0
                         ? $" 击退{reaction.KnockbackX}/浮空{reaction.LaunchY}" : string.Empty));
        }
    }
}
