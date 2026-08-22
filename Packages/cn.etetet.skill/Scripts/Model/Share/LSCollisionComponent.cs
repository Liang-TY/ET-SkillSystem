using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 网格碰撞（挂 LSWorld，进快照）：DNF .til [pass type] 直译的压平矩阵（03 文档 §4.2）。
    /// 世界坐标→格子坐标只看 (x=横向, z=纵深)——网格是地面俯视 2D，高度轴 y 不参与。
    /// 数据来源：tile_layout.json（视图层 LSMapViewComponent 解析进 MapTileLayoutCache，
    /// room.Init 拷进本组件——两端同进程同 json，矩阵一致）。
    /// </summary>
    [ComponentOf(typeof(LSWorld))]
    [MemoryPackable]
    public partial class LSCollisionComponent : LSEntity, IAwake, ISerializeToEntity
    {
        /// <summary>压平碰撞矩阵（gridWidth*gridHeight）：0=阻挡 1=可走；行优先自上而下（行 0 = 世界 z=0）</summary>
        [MemoryPackOrder(0)]
        public byte[] PassGrid;

        [MemoryPackOrder(1)]
        public int GridWidth;

        [MemoryPackOrder(2)]
        public int GridHeight;

        /// <summary>每格大小（单位）。DNF 80px/格 → 0.8（1 单位=100px）</summary>
        [MemoryPackOrder(3)]
        public FP CellSize;

        /// <summary>网格 col=0 的世界 x（瓦片贴图左边缘 = -gridWidth×CellSize/2）</summary>
        [MemoryPackOrder(4)]
        public FP OriginX;

        /// <summary>网格 row=0 的世界 z（对齐可行走带中线：z=0 ≈ 行走带中间行）</summary>
        [MemoryPackOrder(5)]
        public FP OriginZ;
    }
}
