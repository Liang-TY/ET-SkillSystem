using TrueSync;

namespace ET
{
    /// <summary>
    /// 阿甘左训练场（15089training.map）：demo 第一张战斗地图（03 文档 §3.4）。
    /// 瓦片 4 张水平拼（BH004/BH002/BH001/BH005）；怪物 1 只班图女战士。
    /// 出生坐标按真实贴图校准（地图宽 8.96、网格 56 列，旧 3/5 单位是 44.8 宽度时代遗产，x=5 已出网格）。
    /// </summary>
    [MapId(MapIds.TrainingRoom)]
    public class TrainingRoom : MapDefinition
    {
        // 属性名 MonsterAiIds 与静态类 MonsterAiIds 同名冲突（CS0236），用字面值 + 注释标注
        private static readonly int[] MonsterAiIdsArr = { 1 };   // MonsterAiIds.BantuAmazones
        private static readonly TSVector[] MonsterSpawnsArr = { new((FP)5 / 2, 0, 0) };   // x=2.5 → col43 row15 绿带内
        private static readonly TSVector[] MonsterForwardsArr = { new(-1, 0, 0) };

        public override int[] MonsterAiIds => MonsterAiIdsArr;

        public override TSVector[] MonsterSpawns => MonsterSpawnsArr;

        public override TSVector[] MonsterForwards => MonsterForwardsArr;

        public override TSVector PlayerSpawn => new(0, 0, 0);

        /// <summary>瓦片布局（翻译工具 til 子命令产物，Bundles/MapRes/aganzo_training/）</summary>
        public override string TileLayoutPath => "Packages/cn.etetet.lockstep/Bundles/MapRes/aganzo_training/aganzo_tile_layout.json";
    }
}
