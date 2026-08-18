using MemoryPack;

namespace ET
{
    /// <summary>
    /// Buff 实例（纯数据）。配置在 BuffDefinition（[BuffId] 反射注册），效果在 LSAction 节点。
    /// Route B：Just\* 标记由 LSBuffComponentSystem 清（挂载顺序在 Hitbox 前）；Removing 存活一帧服务标记后回收。
    /// </summary>
    [ChildOf(typeof(LSBuffComponent))]
    [MemoryPackable]
    public partial class LSBuff : LSEntity, IAwake<int>, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public int ConfigId;

        [MemoryPackOrder(1)]
        public long SourceId;

        // 剩余 ms（TotalTimeMs=0 的永久 buff 不倒计时）
        [MemoryPackOrder(2)]
        public int RemainingMs;

        // Tick 累积 ms
        [MemoryPackOrder(3)]
        public int TickTimer;

        // 层数（叠层简版：同 buff 再挂 = Stack+1 + 刷新时长）
        [MemoryPackOrder(4)]
        public int Stack;

        // 待回收标记（移除流程已跑，等 Route B 标记被视图读一帧）
        [MemoryPackOrder(5)]
        public bool Removing;

        // --- Route B 状态标记（视图层轮询 diff 用）---
        [MemoryPackOrder(6)]
        public bool JustAdded;

        [MemoryPackOrder(7)]
        public bool JustRemoved;
    }
}
