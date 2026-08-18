using MemoryPack;

namespace ET
{
    /// <summary>Buff 容器（LSBuff 挂这下面）。System 在 ET.Skill 的 LSBuffComponentSystem。</summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSBuffComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
    }
}
