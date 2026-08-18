namespace ET
{
    [EntitySystemOf(typeof(LSBuff))]
    [FriendOf(typeof(LSBuff))]
    public static partial class LSBuffSystem
    {
        [EntitySystem]
        private static void Awake(this LSBuff self, int configId)
        {
            self.ConfigId = configId;
        }
    }
}
