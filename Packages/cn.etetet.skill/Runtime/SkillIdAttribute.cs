namespace ET
{
    /// <summary>技能逻辑类标记（值=技能ID）。SkillLoader/RegisterAssembly 反射扫描注册。</summary>
    public class SkillIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public SkillIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>技能 ID 常量 + 按键槽位映射。</summary>
    public static class SkillIds
    {
        public const int NormalAttack = 1;   // 普攻（膝踢，AnimId.Attack1）
        public const int TestCooldown = 2;   // CD/眩晕验证技能（K 键：起 CD + 给自己挂 Stun）
        public const int WaveSword = 3;      // 地裂·波动剑（I 键，投射物）
        public const int FireCircle = 4;     // 火圈（O 键，区域效果）

        /// <summary>按键值 → 技能槽位映射</summary>
        public static bool ButtonToSkill(int button, out int skillId)
        {
            switch (button)
            {
                case 1: skillId = NormalAttack; return true;
                case 2: skillId = TestCooldown; return true;
                case 3: skillId = WaveSword; return true;
                case 4: skillId = FireCircle; return true;
                default: skillId = 0; return false;
            }
        }
    }
}
