using MemoryPack;

namespace ET
{
    /// <summary>投射物容器（挂 LSWorld——弹不属于任何单位）。System 在 ET.Skill 的 LSBulletSystem。</summary>
    [ComponentOf(typeof(LSWorld))]
    [MemoryPackable]
    public partial class LSBulletComponent : LSEntity, IAwake, ISerializeToEntity
    {
    }
}
