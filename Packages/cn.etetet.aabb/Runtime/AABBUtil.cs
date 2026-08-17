using TrueSync;

namespace ET
{
    [EnableClass]
    public static class AABBUtil
    {
        // === 构造（Id 默认 0；需要身份时调用方显式赋 box.Id） ===

        public static AABB FromMinMax(TSVector min, TSVector max)
        {
            return new AABB(min, max);
        }

        public static AABB FromCenter(TSVector center, TSVector halfExtents)
        {
            return new AABB(center - halfExtents, center + halfExtents);
        }

        // === 更新（复用 AABB，Id 不变） ===

        public static void UpdateMinMax(ref AABB box, TSVector min, TSVector max)
        {
            box.Min = min;
            box.Max = max;
        }

        public static void UpdateCenter(ref AABB box, TSVector center, TSVector halfExtents)
        {
            box.Min = center - halfExtents;
            box.Max = center + halfExtents;
        }

        // === 检测 ===

        public static bool Intersects(AABB a, AABB b)
        {
            return a.Min.x <= b.Max.x && a.Max.x >= b.Min.x
                && a.Min.y <= b.Max.y && a.Max.y >= b.Min.y
                && a.Min.z <= b.Max.z && a.Max.z >= b.Min.z;
        }

        public static bool Contains(AABB outer, AABB inner)
        {
            return outer.Min.x <= inner.Min.x && outer.Max.x >= inner.Max.x
                && outer.Min.y <= inner.Min.y && outer.Max.y >= inner.Max.y
                && outer.Min.z <= inner.Min.z && outer.Max.x >= inner.Max.z;
        }

        // === 变换 ===

        public static AABB Merge(AABB a, AABB b)
        {
            return new AABB
            {
                Min = TSVector.Min(a.Min, b.Min),
                Max = TSVector.Max(a.Max, b.Max)
            };
        }

        public static AABB Expand(AABB box, TSVector expansion)
        {
            return new AABB(box.Min - expansion, box.Max + expansion);
        }
    }
}
