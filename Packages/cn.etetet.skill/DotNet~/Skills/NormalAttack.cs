namespace ET
{
    /// <summary>
    /// 普攻（膝踢）。判定框走 kneekick.json 帧数据（多盒，帧 1-3=判定帧）——不用固定盒 API。
    /// 数值（伤害50/硬直500ms）暂硬编码在 LSHitboxComponentSystem.ApplyHit，Actions 层接入后配置化。
    /// </summary>
    [SkillId(SkillIds.NormalAttack)]
    public class NormalAttack : SkillLogic
    {
        // kneekick（5帧）取消窗口从帧3起（收招）；attack 配置表接入后从 cancelFrame 读
        private const int CancelFrame = 3;

        // DNF 普攻无真 CD（动画 + 取消窗口门禁）
        public override int CooldownMs => 0;

        // kneekick 总时长，到时自动 OnEnd
        public override int TotalTimeMs => 360;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.Attack1);
            ctx.ClearHitTargets();   // 新一轮攻击重置多重命中
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 取消窗口：收招帧起，缓冲有攻击 → 重新起手（连段）
            if (ctx.CurrentFrameIndex() >= CancelFrame && ctx.PeekBufferedButton() == 1)
            {
                ctx.RestartCurrentSkill();
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }
    }
}
