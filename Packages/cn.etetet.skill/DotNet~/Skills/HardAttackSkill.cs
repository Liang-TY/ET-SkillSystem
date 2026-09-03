using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·鬼斩。过程已迁到 SkillParams/skills/hardattack.json，
    /// 这里仅保留 [SkillId] 身份桥接；逻辑实例不保存任何运行时状态。
    /// </summary>
    [SkillId(SkillIds.HardAttack)]
    public class HardAttackSkill : ParametricSkillLogic
    {
        public override int ConfiguredSkillId => SkillIds.HardAttack;
    }
}
