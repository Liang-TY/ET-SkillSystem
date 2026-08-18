using System.Collections.Generic;
using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 投射物（纯逻辑实体：位置/飞行/碰撞/寿命；**动画在视图层自推**——弹的表现帧不影响逻辑，
    /// 逻辑只管 Position 和命中，视图 LSBulletViewComponent 用渲染时间播帧）。
    /// 配置在 BulletDefinition（[BulletId] 反射注册）；命中效果走 HitActions 节点。
    /// </summary>
    [ChildOf(typeof(LSBulletComponent))]
    [MemoryPackable]
    public partial class LSBullet : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public int ConfigId;

        [MemoryPackOrder(1)]
        public long CasterId;

        [MemoryPackOrder(2)]
        public TSVector Position;

        [MemoryPackOrder(3)]
        public TSVector Direction;

        // 剩余寿命 ms（<=0 销毁；穿透型靠寿命自然结束）
        [MemoryPackOrder(4)]
        public int RemainingMs;

        // 已命中单位（防同一目标反复结算；穿透弹跨帧去重）
        [MemoryPackOrder(5)]
        public HashSet<long> HitTargets = new();
    }
}
