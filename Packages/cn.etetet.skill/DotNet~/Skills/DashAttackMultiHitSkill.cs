using TrueSync;

namespace ET
{
    /// <summary>连突刺；过程和参数位于 SkillParams/skills/dashattackmultihit.json。</summary>
    [SkillId(SkillIds.DashAttackMultiHit)]
    public sealed class DashAttackMultiHitSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.DashAttackMultiHit;
    }
}
