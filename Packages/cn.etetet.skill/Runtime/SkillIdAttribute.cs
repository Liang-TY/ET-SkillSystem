namespace ET
{
    /// <summary>技能逻辑类标记（值=技能ID）。SkillLoader.RegisterAssembly 反射扫描注册。</summary>
    public class SkillIdAttribute : BaseAttribute
    {
        public int Id;

        public SkillIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>技能 ID 常量 + 按键槽位映射。</summary>
    public static class SkillIds
    {
        public const int NormalAttack = 1;   // 普攻（膝踢，AnimId.Attack1）
        public const int TestCooldown = 2;   // CD 机制验证用空技能（K 键）

        /// <summary>按键值 → 技能槽位映射（阶段4：1=普攻 J/左键，2=CD测试 K）</summary>
        public static bool ButtonToSkill(int button, out int skillId)
        {
            switch (button)
            {
                case 1: skillId = NormalAttack; return true;
                case 2: skillId = TestCooldown; return true;
                default: skillId = 0; return false;
            }
        }
    }
}
