using TrueSync;

namespace ET
{
    [EnableClass]
    public struct AABB
    {
        private static long nextId = 1;

        public long Id;
        public TSVector Min;
        public TSVector Max;

        public AABB(TSVector min, TSVector max)
        {
            Id = nextId++;
            Min = min;
            Max = max;
        }

        public TSVector Center => (Min + Max) * FP.Half;
        public TSVector Size => Max - Min;
        public TSVector HalfExtents => (Max - Min) * FP.Half;

        public readonly bool Contains(TSVector point)
        {
            return point.x >= Min.x && point.x <= Max.x
                && point.y >= Min.y && point.y <= Max.y
                && point.z >= Min.z && point.z <= Max.z;
        }
    }
}
