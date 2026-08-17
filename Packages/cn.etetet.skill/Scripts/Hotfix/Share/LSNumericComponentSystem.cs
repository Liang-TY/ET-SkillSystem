using System.Collections.Generic;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSNumericComponent))]
    [FriendOf(typeof(LSNumericComponent))]
    public static partial class LSNumericComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSNumericComponent self)
        {
            self.NumericDic ??= new Dictionary<int, FP>();
        }

        public static FP Get(this LSNumericComponent self, int key)
        {
            self.NumericDic.TryGetValue(key, out FP value);
            return value;
        }

        public static void Set(this LSNumericComponent self, int key, FP value)
        {
            self.NumericDic[key] = value;
            // key % 10 != 0 表示是子属性（base/add/pct/finalAdd/finalPct），需要重算 final
            if (key % 10 != 0) self.UpdateFinal(key / 10 * 10);
        }

        public static void Add(this LSNumericComponent self, int key, FP value)
            => self.Set(key, self.Get(key) + value);

        // 五层公式：final = (((base + add) * (1 + pct) + finalAdd) * (1 + finalPct)
        private static void UpdateFinal(this LSNumericComponent self, int finalKey)
        {
            FP baseVal  = self.Get(finalKey + 1);
            FP add      = self.Get(finalKey + 2);
            FP pct      = self.Get(finalKey + 3);
            FP finalAdd = self.Get(finalKey + 4);
            FP finalPct = self.Get(finalKey + 5);
            self.NumericDic[finalKey] = ((baseVal + add) * (FP.One + pct) + finalAdd) * (FP.One + finalPct);
        }
    }
}
