using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    /// <summary>
    /// 命中盒组件：受击盒 + 攻击盒（世界 AABB，多盒——DNF 一帧可有多个 damageBox/attackBox）。
    /// 采样逻辑在 LSHitboxComponentSystem.LSUpdate（确定性，回滚安全）：
    /// 受击盒每帧从当前动画帧 damageBoxes 采样；攻击盒仅攻击动作的判定帧（有 attackBoxes 的帧）激活。
    /// AABB 构造走 new AABB{Min=,Max=} —— Id 恒 0，不碰静���计数器（见 AABB.cs 注释），帧同步 hash 一致。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSHitboxComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        // 受击盒列表（世界坐标，每帧重采样）
        [MemoryPackOrder(0)]
        public List<AABB> CurrentHurtBoxes = new();

        // 攻击盒列表（世界坐标，仅判定帧非空）
        [MemoryPackOrder(1)]
        public List<AABB> CurrentAttackBoxes = new();

        // 派生态：本帧有攻击盒（判定帧）——日志/调试用，逻辑以 CurrentAttackBoxes.Count 为准
        [MemoryPackOrder(2)]
        public bool AttackEnabled;

        // 本次攻击已命中目标（防多重命中：同一次攻击动作内同一目标只结算一次；起手/取消重开时清空）
        [MemoryPackOrder(3)]
        public HashSet<long> HitTargets = new();
    }
}
