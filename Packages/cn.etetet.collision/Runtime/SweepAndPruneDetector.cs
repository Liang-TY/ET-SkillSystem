using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 扫掠裁剪（Sweep and Prune）碰撞检测 — O(n·k)
    /// 适用：横版/2D，角色主要沿 X 轴移动
    /// </summary>
    [EnableClass]
    public static class SweepAndPruneDetector
    {
        /// <summary>
        /// 宽相：按 X 轴排序扫掠，X 轴重叠即算候选对。结果可能含假阳性（Y/Z 轴未验证）
        /// </summary>
        public static void Detect(AABB[] attackers, AABB[] hurts, List<(long, long)> pairs)
        {
            pairs.Clear();
            int an = attackers.Length;
            int hn = hurts.Length;
            if (an == 0 || hn == 0) return;

            // 按 minX 排序受击框
            int[] sortedHurts = new int[hn];
            for (int i = 0; i < hn; i++) sortedHurts[i] = i;
            System.Array.Sort(sortedHurts, (a, b) =>
            {
                int cmp = hurts[a].Min.x.CompareTo(hurts[b].Min.x);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });

            for (int i = 0; i < an; i++)
            {
                AABB atk = attackers[i];
                for (int j = 0; j < hn; j++)
                {
                    AABB hurt = hurts[sortedHurts[j]];
                    if (atk.Max.x < hurt.Min.x) break;
                    if (atk.Min.x > hurt.Max.x) continue;
                    pairs.Add((atk.Id, hurt.Id));
                }
            }
        }

        /// <summary>
        /// 窄相：在宽相基础上用 AABBUtil.Intersects 精确验证，去除假阳性
        /// </summary>
        public static void DetectExact(AABB[] attackers, AABB[] hurts, List<(long, long)> pairs)
        {
            pairs.Clear();
            int an = attackers.Length;
            int hn = hurts.Length;
            if (an == 0 || hn == 0) return;

            int[] sortedHurts = new int[hn];
            for (int i = 0; i < hn; i++) sortedHurts[i] = i;
            System.Array.Sort(sortedHurts, (a, b) =>
            {
                int cmp = hurts[a].Min.x.CompareTo(hurts[b].Min.x);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });

            for (int i = 0; i < an; i++)
            {
                AABB atk = attackers[i];
                for (int j = 0; j < hn; j++)
                {
                    AABB hurt = hurts[sortedHurts[j]];
                    if (atk.Max.x < hurt.Min.x) break;
                    if (atk.Min.x > hurt.Max.x) continue;
                    if (AABBUtil.Intersects(atk, hurt))
                    {
                        pairs.Add((atk.Id, hurt.Id));
                    }
                }
            }
        }
    }
}
