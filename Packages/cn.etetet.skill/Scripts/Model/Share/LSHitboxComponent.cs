using MemoryPack;

namespace ET
{
    /// <summary>
    /// 命中盒组件：受击盒（每帧从当前动画帧 damageBox 采样）+ 攻击盒（技能/攻击动作设置）。
    /// 采样逻辑在 LSHitboxComponentSystem.LSUpdate（确定性，回滚安全）。
    /// AABB 构造一律走 new AABB{Min=,Max=} / AABBUtil.FromMinMax / UpdateCenter —— Id 恒 0，
    /// 不碰静态计数器（见 AABB.cs 注释），帧同步快照 hash 一致。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSHitboxComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        // 受击盒（世界坐标，每帧从动画帧 damageBox 动态采样，缓存当前结果）
        [MemoryPackOrder(0)]
        public AABB CurrentHurtBox;

        // 攻击盒（世界坐标，技能/攻击动作设置；阶段2先由 LSUpdate 临时驱动，阶段4+ 换成 Cast/帧事件驱动）
        [MemoryPackOrder(1)]
        public AABB CurrentAttackBox;

        [MemoryPackOrder(2)]
        public bool AttackEnabled;
    }
}
