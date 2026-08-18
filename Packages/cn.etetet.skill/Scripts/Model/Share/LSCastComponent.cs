using MemoryPack;

namespace ET
{
    /// <summary>技能施放容器（施放中的 LSCast 挂这下面；一个单位同时只允许一个活动 cast）。</summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSCastComponent : LSEntity, IAwake, ISerializeToEntity
    {
    }
}
