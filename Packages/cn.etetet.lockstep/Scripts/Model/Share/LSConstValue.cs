namespace ET
{
    public static class LSConstValue
    {
        public const int MatchCount = 1;
        public const int UpdateInterval = 50;

        /// <summary>玩家移动速度（单位/秒）。战斗（锁步）与城镇（客户端权威）共用，改这里两处同步生效。</summary>
        public const int PlayerMoveSpeed = 4;
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
        
        public const string ExcelPackagePath = "./Packages/cn.etetet.excel";
    }
}