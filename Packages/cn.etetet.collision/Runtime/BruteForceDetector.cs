using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 暴力双循环碰撞检测 — O(n²)
    /// 适用：对象数 < 100（格斗对战、DNF 普通战斗）
    /// </summary>
    [EnableClass]
    public static class BruteForceDetector
    {
        /// <summary>
        /// 攻击框与受击框交叉检测，结果 (attackId, hurtId) 对应 AABB.Id
        /// </summary>
        public static void Detect(AABB[] attackers, AABB[] hurts, List<(long, long)> pairs)
        {
            pairs.Clear();
            for (int i = 0; i < attackers.Length; i++)
            {
                for (int j = 0; j < hurts.Length; j++)
                {
                    if (AABBUtil.Intersects(attackers[i], hurts[j]))
                    {
                        pairs.Add((attackers[i].Id, hurts[j].Id));
                    }
                }
            }
        }
    }
}
