using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 城镇网格碰撞（阶段B）：客户端权威——非锁步、不进快照（03 文档 §4.4 坐标模型与战斗同款，
    /// 字段语义与 LSCollisionComponent 一致；IsBlocked/TryMove 数学在 TownMapViewComponentSystem）。
    /// 放 Model（ModelView 程序集不引用 TrueSync）；数据源：hendonmyre_tile_layout.json
    /// （row 12-28 全宽可走=街道，row 0-11/29 阻挡=天空建筑/底边）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownCollisionComponent: Entity, IAwake, IDestroy
    {
        public byte[] PassGrid;

        public int GridWidth;

        public int GridHeight;

        public FP CellSize;

        public FP CellSizeZ;

        public FP OriginX;

        public FP OriginZ;
    }
}
