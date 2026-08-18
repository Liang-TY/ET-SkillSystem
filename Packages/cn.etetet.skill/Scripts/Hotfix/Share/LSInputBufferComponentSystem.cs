namespace ET
{
    [EntitySystemOf(typeof(LSInputBufferComponent))]
    [LSEntitySystemOf(typeof(LSInputBufferComponent))]
    [FriendOf(typeof(LSInputBufferComponent))]
    public static partial class LSInputBufferComponentSystem
    {
        // 输入缓冲窗口 ms（DNF 连段窗口=[cancelFrame,动画末]，取 300）
        public const int BufferWindowMs = 300;

        [EntitySystem]
        private static void Awake(this LSInputBufferComponent self)
        {
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSInputBufferComponent self)
        {
            // 缓冲超时清空（没被消费的预输入作废）
            if (self.BufferTimer <= 0) return;
            self.BufferTimer -= LSConstValue.UpdateInterval;
            if (self.BufferTimer <= 0)
            {
                self.BufferTimer = 0;
                self.BufferedButton = 0;
            }
        }
    }
}
