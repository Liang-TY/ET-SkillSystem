namespace ET
{
    /// <summary>
    /// CD 机制验证技能（K 键）：无动画无伤害，打日志验证生命周期 + 冷却拦截 + ManualCooldown 路径。
    /// 验证完可删或留作技能模板。
    /// </summary>
    [SkillId(SkillIds.TestCooldown)]
    public class TestCooldownSkill : SkillLogic
    {
        public override int CooldownMs => 2000;   // 2 秒冷却
        public override int TotalTimeMs => 300;   // 300ms 后自动 OnEnd

        public override void OnCast(SkillContext ctx)
        {
            Log.Info($"[Skill] unit{ctx.GetCasterId()} TestCooldown 施放（起 2000ms CD），TotalTime=300ms");
        }

        public override void OnEnd(SkillContext ctx)
        {
            Log.Info($"[Skill] unit{ctx.GetCasterId()} TestCooldown OnEnd（Elapsed={ctx.GetElapsedMs()}ms）");
        }
    }
}
