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

    // ---- 瓦片布局数据（tile_layout.json 的形状，与翻译工具 til 子命令输出对齐）----

    /// <summary>
    /// 地图瓦片布局（翻译工具 til 子命令产物 tile_layout.json 的形状）。
    /// tiles[] 水平拼接；gridWidth = tiles.Length × 14，gridHeight = 30。
    /// </summary>
    [Serializable]
    public class TileLayoutData
    {
        /// <summary>瓦片贴图条目（按水平拼接顺序）</summary>
        public TileLayoutTile[] tiles;

        /// <summary>总网格宽（格）= tiles.Length × 14</summary>
        public int gridWidth;

        /// <summary>总网格高（格）= 30</summary>
        public int gridHeight;

        // ---- 以下为运行时派生字段（json 里不存在，JsonUtility 反序列化后保持默认值，加载时 DeriveLayout 填充）----
        // 注意：不加 [NonSerialized]——部分 Unity 版本该属性会干扰同类数组字段的反序列化（tiles 变 null 的坑）

        /// <summary>每格像素（DNF 80，[img pos]；DeriveLayout 填充）</summary>
        public int cellSizePx;

        /// <summary>压平碰撞矩阵（DeriveLayout 填充）</summary>
        public string passTypes;
    }

    /// <summary>单个瓦片贴图引用（.til 直译：imgPath + imgFrame + 压平碰撞矩阵）</summary>
    [Serializable]
    public class TileLayoutTile
    {
        /// <summary>瓦片图集文件名（含 .img 后缀，如 "Aganzo.img"——运行时去后缀加载 .img.bytes）</summary>
        public string imgPath;

        /// <summary>img 内帧号</summary>
        public int imgFrame;

        /// <summary>该瓦片的碰撞矩阵压平（行优先 30 行×14 列=420 个 int；DNF 原值 0=阻挡 2=可走）</summary>
        public int[] passTypes;
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
