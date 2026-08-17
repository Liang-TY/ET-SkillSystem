using TrueSync;

namespace ET
{
    [EnableClass]
    public struct AABB
    {
        // 身份标识：需要时由调用方显式赋值（实体 Id / 碰撞框类型等）。
        // 不做自动分配——静态计数器是进程级状态，帧同步回滚重放后各端不一致，
        // AABB 存进 LSEntity 快照后 hash 会假报 desync。
        public long Id;
        public TSVector Min;
        public TSVector Max;

        public AABB(TSVector min, TSVector max)
        {
            Id = 0;
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
