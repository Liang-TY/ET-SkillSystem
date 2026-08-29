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
        public const int BloodBoom = 5;      // 浴血之怒（U 键，自耗 HP 的自身中心血爆）

        // 怪物技能段（班图女战士；无按键映射——由 AI/轮播驱动施放）
        public const int MonsterLowKick = 7;    // 下段踢（AI 距离 115px，普通硬直）
        public const int MonsterKneeKick = 8;   // 膝踢（贴身 30px，击倒）
        public const int MonsterHighKick = 9;   // 高踢（击倒 + 20% 出血）
        public const int MonsterIceBreath = 10; // 冰息（远程，帧 3 发冰雾弹，10% 冰冻）
        public const int HardAttack = 11;       // 鬼斩（G 键，暗属性击倒）
        public const int UpperSlash = 12;       // 上挑（Z 键，浮空）
        public const int TripleSlash = 13;      // 三段斩（D 键，连段+前冲）
        public const int DashAttackMultiHit = 14; // 连突刺（T 键，突刺+剑气穿透弹）
        public const int AshenFork = 15;        // 银光落刃（F 键，空中施放+落地冲击波）
        public const int HopSmash = 16;         // 崩山击（E 键，前跃多段下砸+冲击波）
        public const int VaneSlash = 17;        // 裂波斩（V 键，上斩+波轮多段+终结）
        public const int GoreCross = 18;        // 十字斩（Q 键，两刀+血之十字+追击）
        public const int WeaponCombo = 19;      // 里·鬼剑术（A 键，太刀4段连击）

        /// <summary>按键值 → 技能槽位映射（16=起跳，非技能，不进本表）</summary>
        public static bool ButtonToSkill(int button, out int skillId)
        {
            switch (button)
            {
                case 1: skillId = NormalAttack; return true;
                case 2: skillId = TestCooldown; return true;
                case 3: skillId = WaveSword; return true;
                case 5: skillId = BloodBoom; return true;
                case 11: skillId = HardAttack; return true;
                case 12: skillId = UpperSlash; return true;
                case 13: skillId = TripleSlash; return true;
                case 14: skillId = DashAttackMultiHit; return true;
                case 15: skillId = AshenFork; return true;
                case 16: skillId = HopSmash; return true;
                case 17: skillId = VaneSlash; return true;
                case 18: skillId = GoreCross; return true;
                case 19: skillId = WeaponCombo; return true;
                default: skillId = 0; return false;
            }
        }
    }
}
