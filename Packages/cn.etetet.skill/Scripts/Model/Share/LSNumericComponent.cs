using System.Collections.Generic;
using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 数值组件：key-value 存储 + 五层公式。
    /// HP/攻击/防御/速度/禁止移动 等战斗数值全走这里。
    /// Buff 修改属性 = Add(key, value)；回滚时 [MemoryPackable] 快照恢复。
    /// 方法（Get/Set/Add）在 LSNumericComponentSystem 里（ET 铁律：Entity 只放字段）。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSNumericComponent : LSEntity, IAwake, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public Dictionary<int, FP> NumericDic = new();
    }
}
