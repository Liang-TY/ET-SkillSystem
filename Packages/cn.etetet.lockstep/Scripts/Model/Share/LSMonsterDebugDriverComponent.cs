using MemoryPack;

namespace ET
{
    /// <summary>
    /// 怪物技能轮播驱动（阶段1 测试用——班图女战士技能验证；阶段2 被 LSMonsterAIComponent 替换/移除）。
    /// 每 IntervalMs 依次 TryCast 技能表里的下一个（CD/硬直门禁照走，放不出来就跳过下轮再试）。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSMonsterDebugDriverComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public int SkillIndex;      // 当前轮播到第几个

        [MemoryPackOrder(1)]
        public int TimerMs;         // 距下次施放的倒计时
    }
}
