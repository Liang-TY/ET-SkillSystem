using TrueSync;

namespace ET
{
    public static class LSConstValue
    {
        public const int MatchCount = 1;
        public const int UpdateInterval = 50;

        /// <summary>玩家移动速度（单位/秒）。FP 定点数，可非整数（如 1.5）。战斗（锁步）+城镇共用，改这里同步生效。</summary>
        [StaticField]
        public static readonly FP PlayerMoveSpeed = (FP)3 / 2;   // 1.5
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
        
        public const string ExcelPackagePath = "./Packages/cn.etetet.excel";
    }
}