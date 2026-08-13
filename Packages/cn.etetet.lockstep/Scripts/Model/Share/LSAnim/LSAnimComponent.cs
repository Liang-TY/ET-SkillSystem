using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 逻辑层动画状态：决定"当前是哪个动作的第几帧"。
    /// 必须在逻辑层（确定性 + 快照回滚），因为帧决定伤害盒 → 命中 → 胜负。
    /// 视图层 LSSpriteAnimViewComponent 只读这里，负责换 sprite。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSAnimComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public int AnimId { get; set; }       // AnimId.Idle=1, Walk=2, ...
        public int FrameIndex { get; set; }   // 当前帧序号
        public FP FrameTick { get; set; }     // 累积毫秒（FP 定点数，确定性）
        public FP Speed { get; set; }         // 播放倍率
        public bool IsLoop { get; set; }
        public bool IsFinished { get; set; }  // 非循环动画是否播完
    }
}
