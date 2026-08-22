using System;
using TrueSync;

namespace ET
{
    /// <summary>地图配置类标记（值=MapId）。MapLoader/RegisterAssembly 反射扫描注册。</summary>
    public class MapIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public MapIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>地图 ID 常量。</summary>
    public static class MapIds
    {
        /// <summary>阿甘左训练场（15089training.map，4 瓦片水平拼：BH004/BH002/BH001/BH005，见 03 文档 §3.4）</summary>
        public const int TrainingRoom = 1;
    }

    /// <summary>
    /// 地图配置基类（第七类内容，无状态属性类——同 SkillLogic/MonsterAiDefinition）。
    /// 数值出处 DNF .map/.til 直译；RoomSystem.Init 只读配置零常量（按配置建怪物+碰撞矩阵）。
    /// 配置类不准存运行状态（守门员强制）。瓦片布局是外部 json（TileLayoutPath）：
    /// 视图层 LSMapViewComponent 按 path 懒加载（渲染+进 MapTileLayoutCache），
    /// 逻辑层 room.Init 从缓存取碰撞矩阵建 LSCollisionComponent。
    /// </summary>
    public abstract class MapDefinition
    {
        /// <summary>怪物 AI 配置 Id 列表（与 MonsterSpawns/MonsterForwards 一一对应；null=无怪）</summary>
        public virtual int[] MonsterAiIds => null;

        /// <summary>怪物出生点（DNF 像素/100；与 MonsterAiIds 一一对应）</summary>
        public virtual TSVector[] MonsterSpawns => null;

        /// <summary>怪物出生朝向（与 MonsterAiIds 一一对应——攻击盒采样/发弹方向都吃朝向）</summary>
        public virtual TSVector[] MonsterForwards => null;

        /// <summary>玩家出生点（服务器建 unitInfo 用；默认原点）</summary>
        public virtual TSVector PlayerSpawn => TSVector.zero;

        /// <summary>瓦片布局 json 路径（null=空地无碰撞；翻译工具 til 子命令产物）</summary>
        public virtual string TileLayoutPath => null;
    }

    /// <summary>Map 注册表薄壳。</summary>
    public static class MapLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<MapIdAttribute, MapDefinition>.RegisterAssembly(assembly);

        public static MapDefinition Get(int mapId)
            => ContentLoader<MapIdAttribute, MapDefinition>.Get(mapId);
    }

    // ---- 瓦片布局数据（tile_layout.json 的形状，视图层 JsonUtility 反序列化）----

    /// <summary>
    /// 地图瓦片布局（翻译工具 til 子命令产物 tile_layout.json 的形状，03 文档 §3.2/§4.1）。
    /// 像素坐标 = 大图（多瓦片水平拼）坐标，网格原点 (0,0) = 大图左上角；
    /// 1px 深度 = 0.01 单位（DNF y 纵深 ↔ 我们 z，行 0 = 世界 z=0 自上而下）。
    /// </summary>
    [Serializable]
    public class TileLayoutData
    {
        /// <summary>碰撞矩阵宽（格）</summary>
        public int gridWidth;

        /// <summary>碰撞矩阵高（格）</summary>
        public int gridHeight;

        /// <summary>每格像素（DNF 80，[img pos]）</summary>
        public int cellSizePx;

        /// <summary>压平 pass type 串（gridWidth*gridHeight 个字符，行优先自上而下；DNF 原值：'2'=可走 '0'=阻挡）</summary>
        public string passTypes;

        /// <summary>瓦片贴图条目（全部 Blit 到一张大 Texture2D 铺地面）</summary>
        public TileLayoutTile[] tiles;
    }

    /// <summary>单个瓦片贴图引用（.til [IMAGE] 段直译 + 大图 Blit 位置）</summary>
    [Serializable]
    public class TileLayoutTile
    {
        /// <summary>瓦片图集文件名（不含 .img.bytes 后缀——与 tile_layout.json 同目录）</summary>
        public string imgName;

        /// <summary>img 内帧号（[IMAGE] 段第二个数：tile00=0、tile01=1 依序）</summary>
        public int frame;

        /// <summary>Blit 到大图的左上角 X（px）</summary>
        public int x;

        /// <summary>Blit 到大图的左上角 Y（px）</summary>
        public int y;
    }

    /// <summary>
    /// 瓦片布局进程级缓存：视图层解析 json 后 Set，逻辑层 room.Init 读 Get（建 LSCollisionComponent）。
    /// 两端口径一致前提 = 同进程同一份 json（同 SkillSystemConfig.RngSeed 模式——服务器与客户端同 Unity 进程跑）。
    /// </summary>
    public static class MapTileLayoutCache
    {
        [StaticField]
        private static string cachedPath;

        [StaticField]
        private static TileLayoutData cached;

        public static void Set(string path, TileLayoutData data)
        {
            cachedPath = path;
            cached = data;
        }

        /// <summary>按 path 取（path 不匹配=未加载，返回 null——调用方按空地处理）</summary>
        public static TileLayoutData Get(string path)
        {
            return path == cachedPath ? cached : null;
        }
    }
}
