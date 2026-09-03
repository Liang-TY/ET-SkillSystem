using TrueSync;

namespace ET
{
    /// <summary>鬼剑士地面/空中普攻；过程和参数位于 SkillParams/skills/normalattack.json。</summary>
    [SkillId(SkillIds.NormalAttack)]
    public sealed class NormalAttack : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.NormalAttack;
    }
}
