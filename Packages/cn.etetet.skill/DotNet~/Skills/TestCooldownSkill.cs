using TrueSync;

namespace ET
{
    /// <summary>CD/Buff 链路验证技能；过程和参数位于 SkillParams/skills/testcooldown.json。</summary>
    [SkillId(SkillIds.TestCooldown)]
    public sealed class TestCooldownSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.TestCooldown;
    }
}
