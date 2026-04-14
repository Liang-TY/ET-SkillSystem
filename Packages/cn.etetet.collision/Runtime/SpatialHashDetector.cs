using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 空间哈希碰撞检测 — O(n) 平均
    /// 适用：固定场景，对象均匀分布（帧同步 RTS、大型战场）
    /// </summary>
    [EnableClass]
    public struct SpatialHash
    {
        public int CellSize;
        public Dictionary<long, List<long>> Grid;

        private static long HashCell(int x, int y, int z)
        {
            return (long)x * 73856093 ^ (long)y * 19349663 ^ (long)z * 83492791;
        }

        public void Init(int cellSize)
        {
            CellSize = cellSize;
            Grid = new Dictionary<long, List<long>>();
        }

        public void Clear()
        {
            Grid.Clear();
        }

        public void Insert(AABB box)
        {
            int minX = FP.ToInt(FP.Floor(box.Min.x)) / CellSize;
            int maxX = FP.ToInt(FP.Floor(box.Max.x)) / CellSize;
            int minY = FP.ToInt(FP.Floor(box.Min.y)) / CellSize;
            int maxY = FP.ToInt(FP.Floor(box.Max.y)) / CellSize;
            int minZ = FP.ToInt(FP.Floor(box.Min.z)) / CellSize;
            int maxZ = FP.ToInt(FP.Floor(box.Max.z)) / CellSize;

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                long key = HashCell(x, y, z);
                if (!Grid.TryGetValue(key, out var list))
                {
                    list = new List<long>();
                    Grid[key] = list;
                }
                list.Add(box.Id);
            }
        }

        public void Query(AABB box, HashSet<long> results)
        {
            int minX = FP.ToInt(FP.Floor(box.Min.x)) / CellSize;
            int maxX = FP.ToInt(FP.Floor(box.Max.x)) / CellSize;
            int minY = FP.ToInt(FP.Floor(box.Min.y)) / CellSize;
            int maxY = FP.ToInt(FP.Floor(box.Max.y)) / CellSize;
            int minZ = FP.ToInt(FP.Floor(box.Min.z)) / CellSize;
            int maxZ = FP.ToInt(FP.Floor(box.Max.z)) / CellSize;

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                long key = HashCell(x, y, z);
                if (Grid.TryGetValue(key, out var list))
                {
                    foreach (long id in list)
                    {
                        results.Add(id);
                    }
                }
            }
        }

        /// <summary>
        /// 宽相：注册受击框，遍历攻击框查询候选对。结果可能含假阳性
        /// </summary>
        public static void Detect(SpatialHash hash, AABB[] attackers, AABB[] hurts, List<(long, long)> pairs)
        {
            pairs.Clear();
            hash.Clear();
            foreach (AABB hurt in hurts)
                hash.Insert(hurt);

            var candidates = new HashSet<long>();
            foreach (AABB atk in attackers)
            {
                candidates.Clear();
                hash.Query(atk, candidates);
                foreach (long hurtId in candidates)
                {
                    pairs.Add((atk.Id, hurtId));
                }
            }
        }

        /// <summary>
        /// 窄相：宽相基础上用 AABBUtil.Intersects 精确验证，去除假阳性
        /// 需要 hurtMap 将 Id 映射回 AABB 用于精确检测
        /// </summary>
        public static void DetectExact(SpatialHash hash, AABB[] attackers, AABB[] hurts, Dictionary<long, AABB> hurtMap, List<(long, long)> pairs)
        {
            pairs.Clear();
            hash.Clear();
            foreach (AABB hurt in hurts)
            {
                hash.Insert(hurt);
                hurtMap[hurt.Id] = hurt;
            }

            var candidates = new HashSet<long>();
            foreach (AABB atk in attackers)
            {
                candidates.Clear();
                hash.Query(atk, candidates);
                foreach (long hurtId in candidates)
                {
                    if (hurtMap.TryGetValue(hurtId, out AABB hurt) && AABBUtil.Intersects(atk, hurt))
                    {
                        pairs.Add((atk.Id, hurtId));
                    }
                }
            }
        }
    }
}
