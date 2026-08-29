using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·上挑（up_attack.ani 9 帧 550ms，F2/F3 自带攻击盒 → 帧驱动自动激活）。
    /// .skl：CD 2000；attack.nut 剑鬼版函数组实证：UpForce=col1（Lv1=350）、BackForce=100、方向 UP。
    /// 命中反应：浮空为主（LaunchY 350）+ 轻微后拉（Kb 100）。
    /// 刀光：up_attack.ani.als 官方边车（F2 挂 upperslash1 层 1000）翻译 overlay。
    /// 简化：霸体（全帧 SUPERARMOR）/等级缩放/追加段（UpperSlashAfter）延后。
    /// 参考：Notes/技能实现/鬼剑士技能解析/046-UpperSlash.md
    /// </summary>
    [SkillId(SkillIds.UpperSlash)]
    public class UpperSlashSkill : SkillLogic
    {
        // upperslash.atk + 剑鬼脚本直译：伤害 70 / 硬直 500 / 后拉 100 / 浮空 350（col1 Lv1）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 70,
            HitstunMs = 500,
            KnockbackX = 100,
            LaunchY = 350,
        };
        public override HitReaction HitReaction => Reaction;

        public override int CooldownMs => 2000;   // .skl [dungeon][cool time]
        public override int TotalTimeMs => 550;   // up_attack.ani 总时长

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.SwordmanUpAttack);
            ctx.ClearHitTargets();
            ctx.SetCasterSuperArmor(TotalTimeMs);   // 全 9 帧 SUPERARMOR（up_attack.ani 实测）——霸体框架落地后补
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();   // 帧驱动盒动画切走后清残留
            ctx.PlayDefaultAnim();
        }
    }
}
