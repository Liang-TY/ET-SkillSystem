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
    public partial class LSCollisionComponent : LSEntity, IAwake, ILSUpdate, ISerializeToEntity
    {
        /// <summary>压平碰撞矩阵（gridWidth*gridHeight）：0=阻挡 1=可走；行优先自上而下（行 0 = 贴图顶部 z=+OriginZ，行增大向屏幕下方）</summary>
        [MemoryPackOrder(0)]
        public byte[] PassGrid;

        [MemoryPackOrder(1)]
        public int GridWidth;

        [MemoryPackOrder(2)]
        public int GridHeight;

        /// <summary>X 轴每格宽（单位）= 贴图世界宽/GridWidth（训练场 0.16 = 16px@100PPU）</summary>
        [MemoryPackOrder(3)]
        public FP CellSize;

        /// <summary>Z 轴每格高（单位）= 贴图世界高/GridHeight（训练场 ≈0.1867 = 18.67px）。
        /// 源美术格子非正方形（16×18.67px），X/Z 拆两轴网格才能精确铺满贴图（03 文档 §9 第 6 轮）</summary>
        [MemoryPackOrder(6)]
        public FP CellSizeZ;

        /// <summary>网格 col=0 的世界 x（瓦片贴图左边缘 = -gridWidth×CellSize/2）</summary>
        [MemoryPackOrder(4)]
        public FP OriginX;

        /// <summary>网格 row=0 的世界 z = 贴图顶边（gridHeight×CellSizeZ/2，贴图中心对齐世界原点）；z=0 对应中间行</summary>
        [MemoryPackOrder(5)]
        public FP OriginZ;
    }
}
