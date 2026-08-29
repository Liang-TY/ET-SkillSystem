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
            ctx.DamageOwner(reaction.Damage);                // 霸体也扣血（DNF super armor 同构）
            bool superArmor = ctx.IsOwnerSuperArmor();       // 霸体：免硬直/受击动画/击退浮空，只掉血
            if (!superArmor)
            {
                ctx.SetOwnerHitstun(reaction.HitstunMs);
                int hurtAnim = ctx.GetOwnerHurtAnimId();
                if (hurtAnim > 0) ctx.PlayOwnerAnim(hurtAnim);   // 受击者自己的受击动画（DNF sq_GetDamageAni 同构）

                // 击退/浮空（DNF .atk push aside / lift up；未配置 = 0 保持旧行为）
                if (reaction.KnockbackX != 0 || reaction.LaunchY != 0)
                    ctx.LaunchOwner(reaction.KnockbackX, reaction.LaunchY);
            }

            // 概率附加状态（DNF .atk [active status]：出血/冰冻…；LSRng 确定性种子判定）
            if (reaction.ProcBuffId != 0 && LSRng.Roll(ctx.FrameNo, ctx.GetOwnerId(), LSRng.PurposeProcStatus) < reaction.ProcChance)
            {
                ctx.AddBuffToOwner(reaction.ProcBuffId);
                Log.Info($"[Combat] unit{ctx.GetOwnerId()} 触发附加状态 buff{reaction.ProcBuffId}（{reaction.ProcChance}%）");
            }

            Log.Info($"[Combat] 帧{ctx.FrameNo} unit{ctx.GetSourceId()} 命中 unit{ctx.GetOwnerId()}，" +
                     $"伤害{reaction.Damage}，HP={ctx.GetOwnerHp()} hitstun={reaction.HitstunMs}ms" +
                     (reaction.KnockbackX != 0 || reaction.LaunchY != 0
                         ? $" 击退{reaction.KnockbackX}/浮空{reaction.LaunchY}" : ""));
        }
    }
}
