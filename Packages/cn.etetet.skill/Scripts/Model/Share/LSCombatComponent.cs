using MemoryPack;

namespace ET
{
    /// <summary>
    /// 战斗状态组件：受击硬直计时（顿帧字段保留但暂不启用——DNF 实证攻方不停帧，
    /// 打击感靠受击僵直 + 每帧判定框 + 受击动画，见 04b-阶段3.5 §4）。
    /// Route B：LastHitstunTimer 是上帧值，视图层轮询 diff（0→>0 刚被击中，>0→0 硬直结束）。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSCombatComponent : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
    {
        // 受击硬直剩余 ms（>0 不能移动/起手攻击）
        [MemoryPackOrder(0)]
        public int HitstunTimer;

        // 顿帧剩余 ms（暂不启用；命中时也只写 HitstunTimer）
        [MemoryPackOrder(1)]
        public int HitstopTimer;

        // 上帧硬直值（视图 Route B diff 用）
        [MemoryPackOrder(2)]
        public int LastHitstunTimer;

        // 硬直/攻击结束后回的默认动画（玩家 Idle / 怪物 Walk）
        [MemoryPackOrder(3)]
        public int DefaultAnimId;

        // 受击动画（每角色自带，DNF sq_GetDamageAni 同构；0=未配置→红闪兜底）
        [MemoryPackOrder(4)]
        public int HurtAnimId;
    }
}
