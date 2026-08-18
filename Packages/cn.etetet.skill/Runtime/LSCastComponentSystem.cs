namespace ET
{
    [EntitySystemOf(typeof(LSCastComponent))]
    [FriendOf(typeof(LSCast))]
    public static partial class LSCastComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSCastComponent self)
        {
        }

        /// <summary>当前活动 cast（未结束的）；无则 null。单活动契约（SkillCastHelper 门禁保证）。</summary>
        public static LSCast GetActiveCast(this LSCastComponent self)
        {
            foreach (var kv in self.Children)
            {
                if (kv.Value is LSCast cast && !cast.Finished) return cast;
            }
            return null;
        }
    }
}
