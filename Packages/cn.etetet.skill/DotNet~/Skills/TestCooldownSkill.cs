namespace ET
{
    /// <summary>
    /// CD/眩晕双验证技能（K 键）：施放起 2 秒 CD + 给自己挂 Stun（1 秒 WASD 失效），
    /// 一个键验证 CD 机制和 Buff 的 Add/Remove Actions 链路。
    /// </summary>
    [SkillId(SkillIds.TestCooldown)]
    public class TestCooldownSkill : SkillLogic
    {
        public override int CooldownMs => 2000;   // 2 秒冷却
        public override int TotalTimeMs => 300;   // 300ms 后自动 OnEnd

        public override void OnCast(SkillContext ctx)
        {
            ctx.AddBuffToSelf(BuffIds.Stun);   // 眩晕验证：1 秒 WASD 失效（ForbidMove 开/关）
            Log.Info($"[Skill] unit{ctx.GetCasterId()} TestCooldown 施放（起 2000ms CD + 自挂 Stun）");
        }

        public override void OnEnd(SkillContext ctx)
        {
            Log.Info($"[Skill] unit{ctx.GetCasterId()} TestCooldown OnEnd（Elapsed={ctx.GetElapsedMs()}ms）");
        }
    }
}
