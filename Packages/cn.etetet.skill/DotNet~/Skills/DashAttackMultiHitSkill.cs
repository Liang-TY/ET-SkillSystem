using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·连突刺（dashattackmultihit.ani 8 帧 500ms，F2-F6 自带攻击盒 → 帧驱动自动激活）。
    /// DNF：前冲攻击中按 X 追加突刺 + 前方激光剑气 PO（穿透延伸，独立结算）；无 nut 参照，数据两方重建。
    /// 简化：前置"前冲攻击中"无状态机 → 独立瞬发键（CD 1000 本就近连打节奏）；
    /// 命中率加成（col1）/反向击退标记/cut+blood 表现忽略；"MultiHit"按单次结算（HitTargets 去重）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/011-DashAttackMultiHit.md
    /// </summary>
    [SkillId(SkillIds.DashAttackMultiHit)]
    public class DashAttackMultiHitSkill : SkillLogic
    {
        // dashattackmultihit.atk 直译：damage/push 250/lift 200/hit horizon（水平击退为主，Ly 取小防意外浮空）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 60,
            HitstunMs = 400,
            KnockbackX = 250,
            LaunchY = 80,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 1000;   // .skl [dungeon][cool time]
        public override int TotalTimeMs => 500;   // dashattackmultihit.ani 总时长

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 剑气弹发射时点（F2 = 100+100+... f0 100 + f1 100 = 200ms）
        private const int BeamAtMs = 200;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.SwordmanDashAttackMultiHit);
            ctx.ClearHitTargets();
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // F2 时点发剑气弹（SubState 守卫一次性；帧驱动攻击盒自动激活不用管）
            if (ctx.GetSubState() == 0 && ctx.GetElapsedMs() >= BeamAtMs)
            {
                ctx.SetSubState(1);
                ctx.CreateBullet(BulletIds.ThrustBeam);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
