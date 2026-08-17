using MemoryPack;

namespace ET
{
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSInputComponent: LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public LSInput LSInput { get; set; }

        // 上帧按键值（按下沿检测：Button 从 0→1 才算一次输入，按住不连发）
        public int LastButton { get; set; }
    }
}