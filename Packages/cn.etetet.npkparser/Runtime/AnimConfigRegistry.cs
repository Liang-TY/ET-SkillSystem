using System.Collections.Generic;

namespace ET
{
    public static class AnimConfigRegistry
    {
        [StaticField]
        private static readonly Dictionary<int, AnimClipData> configs = new();

        public static void Register(int animId, AnimClipData data)
        {
            configs[animId] = data;
        }

        public static AnimClipData Get(int animId)
        {
            configs.TryGetValue(animId, out AnimClipData data);
            return data;
        }
    }

    public static class AnimId
    {
        public const int None = 0;
        public const int Idle = 1;
        public const int Walk = 2;
        public const int Attack1 = 3;   // 普攻第一段（暂用班图膝踢 kneekick.json，判定帧 1-3 有攻击盒）
        public const int Hurt = 4;      // 受击僵直（damage.json；末帧长 delay 停帧，靠硬直计时切走）
        public const int NormalWave = 5; // 地裂波动剑投射物（normalwave.json + NormalWave1.img，视图层自推帧）
    }
}
