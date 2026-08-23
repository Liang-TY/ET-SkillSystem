using MemoryPack;

namespace ET
{
    /// <summary>
    /// 怪物 AI 组件（行为机的锁步翻译：协程局部状态快照化，见 02 文档 §10.4）。
    /// 行为=互斥节点（Idle/ChaseAttack），条件重估切换时清状态（=cancelToken 打断）。
    /// 数值全部读 MonsterAiDefinition（第六类内容，配置驱动）；本组件只存运行状态。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSMonsterAIComponent : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
    {
        // 使用的 AI 配置（MonsterAiIds）
        [MemoryPackOrder(0)]
        public int MonsterAiId;

        // 当前行为节点（0=Idle / 1=ChaseAttack；行为机节点枚举，扩展加值）
        [MemoryPackOrder(1)]
        public int CurrentNode;

        // 当前目标（0=无）
        [MemoryPackOrder(2)]
        public long TargetId;

        // 行为重估节流（ThinkIntervalMs 倒数）
        [MemoryPackOrder(3)]
        public int ThinkTimerMs;

        // 出手间隔节流（AttackIntervalMs 倒数）
        [MemoryPackOrder(4)]
        public int AttackTimerMs;

        // 死亡倒计时（>0=正在播死亡动画，到 0 才 Dispose；BattleWatcher 按 AI 组件消失判活=自动等动画播完）
        [MemoryPackOrder(5)]
        public int DyingTimerMs;
    }
}
