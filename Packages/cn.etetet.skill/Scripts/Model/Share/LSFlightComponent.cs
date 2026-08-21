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

        // 当前速度（单位/秒；y 分量受重力，见 LSFlightComponentSystem）
        [MemoryPackOrder(1)]
        public TSVector Velocity;
    }
}
