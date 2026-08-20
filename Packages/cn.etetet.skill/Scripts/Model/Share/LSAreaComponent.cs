using MemoryPack;

namespace ET
{
    /// <summary>区域效果容器（挂 LSWorld）。System 在 ET.Skill 的 LSAreaComponentSystem。</summary>
    [ComponentOf(typeof(LSWorld))]
    [MemoryPackable]
    public partial class LSAreaComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
    }
}
