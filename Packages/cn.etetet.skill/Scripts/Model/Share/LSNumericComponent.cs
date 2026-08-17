using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 数值组件：key-value 存储 + 五层公式。
    /// HP/攻击/防御/速度/禁止移动 等战斗数值全走这里。
    /// Buff 修改属性 = Add(key, value)；回滚时 [MemoryPackable] 快照恢复。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSNumericComponent : LSEntity, IAwake, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public Dictionary<int, FP> NumericDic = new();

        public FP Get(int key)
        {
            NumericDic.TryGetValue(key, out FP value);
            return value;
        }

        public void Set(int key, FP value)
        {
            NumericDic[key] = value;
            // key % 10 != 0 表示是子属性（base/add/pct/finalAdd/finalPct），需要重算 final
            if (key % 10 != 0) UpdateFinal(key / 10 * 10);
        }

        public void Add(int key, FP value) => Set(key, Get(key) + value);

        // 五层公式：final = (((base + add) * (1 + pct) + finalAdd) * (1 + finalPct)
        private void UpdateFinal(int finalKey)
        {
            FP baseVal  = Get(finalKey + 1);
            FP add      = Get(finalKey + 2);
            FP pct      = Get(finalKey + 3);
            FP finalAdd = Get(finalKey + 4);
            FP finalPct = Get(finalKey + 5);
            NumericDic[finalKey] = ((baseVal + add) * (FP.One + pct) + finalAdd) * (FP.One + finalPct);
        }
    }
}
