using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 注意：本 System 在 ET.Skill 程序集（不在 ET.Hotfix）——SkillContext（同程序集）要调 Get/Set/Add
    /// 扩展方法，而 ET.Skill 不能引用 Hotfix（循环依赖）。Hotfix 侧调用方经 ET.Skill 引用照常可用；
    /// 实体 LSNumericComponent 仍在 ET.Model。
    /// </summary>
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
            // 子属性（5位数，final*10+1~5，见 NumericType）→ 重算对应 final（4位数）。
            // final key 直接写入不重算：当前 HP 扣减、ForbidMove 标记都走这条路。
            if (key >= 10000) self.UpdateFinal(key / 10);
        }

        public static void Add(this LSNumericComponent self, int key, FP value)
            => self.Set(key, self.Get(key) + value);

        // 五层公式：final = (((base + add) * (1 + pct) + finalAdd) * (1 + finalPct)
        // 子属性 key = finalKey * 10 + 1~5（如 HpBase=10011 → finalKey=1001=Hp）
        private static void UpdateFinal(this LSNumericComponent self, int finalKey)
        {
            FP baseVal  = self.Get(finalKey * 10 + 1);
            FP add      = self.Get(finalKey * 10 + 2);
            FP pct      = self.Get(finalKey * 10 + 3);
            FP finalAdd = self.Get(finalKey * 10 + 4);
            FP finalPct = self.Get(finalKey * 10 + 5);
            self.NumericDic[finalKey] = ((baseVal + add) * (FP.One + pct) + finalAdd) * (FP.One + finalPct);
        }
    }
}
