using TrueSync;

namespace ET
{
    /// <summary>
    /// 阿甘左训练场（15089training.map）：demo 第一张战斗地图（03 文档 §3.4）。
    /// 瓦片 4 张水平拼（BH004/BH002/BH001/BH005）；怪物 2 只班图女战士。
    /// 出生坐标承接原 RoomSystem 测试桩（3/5 单位、面向玩家出生点）——像素直译待翻译工具产物到位后校准。
    /// </summary>
    [MapId(MapIds.TrainingRoom)]
    public class TrainingRoom : MapDefinition
    {
        // 属性名 MonsterAiIds 与静态类 MonsterAiIds 同名冲突（CS0236），用字面值 + 注释标注
        private static readonly int[] MonsterAiIdsArr = { 1, 1 };   // MonsterAiIds.BantuAmazones
        private static readonly TSVector[] MonsterSpawnsArr = { new(3, 0, 0), new(5, 0, 0) };
        private static readonly TSVector[] MonsterForwardsArr = { new(-1, 0, 0), new(-1, 0, 0) };

        public override int[] MonsterAiIds => MonsterAiIdsArr;

        public override TSVector[] MonsterSpawns => MonsterSpawnsArr;

        public override TSVector[] MonsterForwards => MonsterForwardsArr;

        public override TSVector PlayerSpawn => new(0, 0, 0);

        /// <summary>瓦片布局（翻译工具 til 子命令产物，Bundles/MapRes/aganzo_training/）</summary>
        public override string TileLayoutPath => "Packages/cn.etetet.lockstep/Bundles/MapRes/aganzo_training/aganzo_tile_layout.json";
    }
}
