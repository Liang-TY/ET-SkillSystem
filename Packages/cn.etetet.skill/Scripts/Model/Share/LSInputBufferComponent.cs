using MemoryPack;

namespace ET
{
    /// <summary>
    /// 输入缓冲：动作中按下的键暂存（300ms 窗口），到取消窗口/动作结束时消费。
    /// 写入：LSInputComponentSystem（按下沿检测，按住不重复写）。
    /// 消费：LSHitboxComponentSystem（起手判定 + 收招帧取���窗口）。
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSInputBufferComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        // 缓冲的按键（0=无, 1=攻击）
        [MemoryPackOrder(0)]
        public int BufferedButton;

        // 缓冲剩余 ms（超时清空）
        [MemoryPackOrder(1)]
        public int BufferTimer;
    }
}
