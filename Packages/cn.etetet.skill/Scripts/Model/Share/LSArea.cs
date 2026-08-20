using System.Collections.Generic;
using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 区域效果实体（纯逻辑：位置/持续/进出检测/Tick；动画在视图层自推同弹）。
    /// 配置在 AreaDefinition（[AreaId] 反射注册）；效果走 Enter/Tick/Exit Actions 节点。
    /// </summary>
    [ChildOf(typeof(LSAreaComponent))]
    [MemoryPackable]
    public partial class LSArea : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public int ConfigId;

        [MemoryPackOrder(1)]
        public long CasterId;

        [MemoryPackOrder(2)]
        public TSVector Position;

        [MemoryPackOrder(3)]
        public int RemainingMs;

        [MemoryPackOrder(4)]
        public int TickTimer;

        // 当前区域内单位（进入加/离开删；驱动 Enter/Exit Actions）
        [MemoryPackOrder(5)]
        public HashSet<long> InsideUnits = new();

        // 待回收标记（消失流程已跑，Route B 标记服务一帧后回收）
        [MemoryPackOrder(6)]
        public bool Removing;

        // --- Route B 标记 ---
        [MemoryPackOrder(7)]
        public bool JustAdded;

        [MemoryPackOrder(8)]
        public bool JustRemoved;
    }
}
