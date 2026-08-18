using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    /// <summary>
    /// 技能组件：冷却计时（数据；TryCast/冷却递减在 LSSkillComponentSystem）。
    /// 按键→技能槽位映射见 SkillIds.ButtonToSkill。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSSkillComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        // 冷却剩余 ms（skillId → 剩余；无条目 = 无 CD）
        [MemoryPackOrder(0)]
        public Dictionary<int, int> Cooldowns = new();
    }
}
