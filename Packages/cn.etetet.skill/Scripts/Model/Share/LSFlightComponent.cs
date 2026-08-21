using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 击退/浮空飞行组件（DNF .atk push aside / lift up 同构）：
    /// 命中时写入初速度（LSActionContext.LaunchOwner），之后每逻辑帧重力积分 + 位移。
    /// - 空中（lift &gt; 0）：抛物线，落地（y≤0 且向下）动量清零趴住（击倒手感，起身靠硬直计时）；
    /// - 贴地（纯水平击退）：y 钳 0 滑行，水平速度摩擦衰减到阈值后停止。
    /// 进快照回滚安全；受击硬直期间输入移动已被门禁（LSInputComponentSystem），互不冲突。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSFlightComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        // 飞行中（false 时组件静默，零开销）
        [MemoryPackOrder(0)]
        public bool Active;

        // 当前速度（单位/秒；y 分量受重力）
        [MemoryPackOrder(1)]
        public TSVector Velocity;

        // ---- 物理参数（Awake 给默认值；按单位可调——以后重型单位可加大重力/摩擦表抗击飞）----
        // 重力（单位/s²）：lift 400px（初速 8）→ 空中 ~0.4s、最高 0.8 单位
        [MemoryPackOrder(2)]
        public FP Gravity;

        // 贴地摩擦（1/s）：纯水平击退的滑行衰减
        [MemoryPackOrder(3)]
        public FP GroundFriction;

        // 停滑阈值（单位/s）：水平速度低于此值结束滑行
        [MemoryPackOrder(4)]
        public FP MinSlideSpeed;
    }
}
