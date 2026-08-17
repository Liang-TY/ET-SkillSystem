namespace ET
{
    [EntitySystemOf(typeof(LSInputBufferComponent))]
    [LSEntitySystemOf(typeof(LSInputBufferComponent))]
    [FriendOf(typeof(LSInputBufferComponent))]
    public static partial class LSInputBufferComponentSystem
    {
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
