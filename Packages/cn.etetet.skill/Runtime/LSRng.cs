namespace ET
{
    /// <summary>
    /// 确定性概率（帧同步安全）：种子来自 skillconfig.json（SkillSystemConfig.RngSeed，两端同配置），
    /// 逐次结果由 (种子, 帧号, 单位Id, 用途) 整数混合派生——无状态、不进快照、回滚重放天然一致。
    /// 注意：同帧同单位同用途的多次 Roll 结果相同（命中去重后一帧内同用途基本只 roll 一次，够用）。
    /// </summary>
    public static class LSRng
    {
        /// <summary>用途常量（不同用途的 roll 互不干扰）</summary>
        public const int PurposeProcStatus = 1;   // HitReaction.ProcBuffId 概率判定
        public const int PurposeAiSelect = 2;     // AI 选招（阶段2）
        public const int PurposeCounter = 3;      // 受击反击概率（阶段2）

        /// <summary>返回 [0,100) 的确定性伪随机数。低于概率值 = 触发。</summary>
        public static int Roll(int frameNo, long unitId, int purpose)
        {
            // splitmix32 混合：足够打散、纯整数运算（FP 都不用，无精度歧义）
            uint x = (uint)(SkillSystemConfig.RngSeed ^ (uint)frameNo * 0x9E3779B1u ^ (uint)unitId * 0x85EBCA6Bu ^ (uint)purpose * 0xC2B2AE35u);
            x ^= x >> 16; x *= 0x7FEB352Du;
            x ^= x >> 15; x *= 0x846CA68Bu;
            x ^= x >> 16;
            return (int)(x % 100u);
        }
    }
}
