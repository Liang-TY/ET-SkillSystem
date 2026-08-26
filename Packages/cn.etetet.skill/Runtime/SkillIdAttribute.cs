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
        public const int NormalAttack = 1;   // 普攻（鬼剑士 swordman_attack1）
        public const int TestCooldown = 2;   // CD/眩晕验证技能（K 键：起 CD + 给自己挂 Stun）
        public const int WaveSword = 3;      // 地裂·波动剑（I 键，投射物）
        public const int FireCircle = 4;     // 火圈（O 键，区域效果）
        public const int BloodBoom = 5;      // 浴血之怒（U 键，自耗 HP 的自身中心血爆）

        // 怪物技能段（班图女战士；无按键映射——由 AI/轮播驱动施放）
        public const int MonsterLowKick = 7;    // 下段踢（AI 距离 115px，普通硬直）
        public const int MonsterKneeKick = 8;   // 膝踢（贴身 30px，击倒）
        public const int MonsterHighKick = 9;   // 高踢（击倒 + 20% 出血）
        public const int MonsterIceBreath = 10; // 冰息（远程，帧 3 发冰雾弹，10% 冰冻）

        /// <summary>按键值 → 技能槽位映射</summary>
        public static bool ButtonToSkill(int button, out int skillId)
        {
            switch (button)
            {
                case 1: skillId = NormalAttack; return true;
                case 2: skillId = TestCooldown; return true;
                case 3: skillId = WaveSword; return true;
                case 4: skillId = FireCircle; return true;
                case 5: skillId = BloodBoom; return true;
                default: skillId = 0; return false;
            }
        }
    }
}
