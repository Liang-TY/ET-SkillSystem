using TrueSync;

namespace ET
{
    /// <summary>
    /// 班图女战士 AI 配置（.mob 直译，数据全录见 02 文档 §10.2）。
    /// 简化决策（已授权）：HighKick 权重 0 → 进近身随机池；IceBreath 权重 0 → 远程先手；
    /// close-carefully 绕圈不做（直线接近）；反击 5% 放弃；返回不做。
    /// </summary>
    [MonsterAiId(MonsterAiIds.BantuAmazones)]
    public class BantuAmazonesAi : MonsterAiDefinition
    {
        // 近战三选一（LowKick 115px / KneeKick 30px / HighKick 45px → 统一池，权重均分）
        private static readonly int[] MeleeSkills = { SkillIds.MonsterLowKick, SkillIds.MonsterKneeKick, SkillIds.MonsterHighKick };
        public override int[] MeleeSkillIds => MeleeSkills;

        private static readonly int[] Weights = { 34, 33, 33 };
        public override int[] MeleeWeights => Weights;

        // 冰息（.mob [throw attack]：远程先手，1.2~6 单位窗口内优先喷，喷完继续接近）
        public override int RangedSkillId => SkillIds.MonsterIceBreath;
    }
}
