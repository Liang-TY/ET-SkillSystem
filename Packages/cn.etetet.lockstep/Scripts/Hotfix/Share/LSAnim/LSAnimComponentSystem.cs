using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSAnimComponent))]
    [LSEntitySystemOf(typeof(LSAnimComponent))]
    public static partial class LSAnimComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimComponent self)
        {
            // 默认播 Idle（clip 在视图层 LSAnimResComponent.InitAsync 注册；
            // 若注册晚于首个 tick，Play 的 null 守卫会先记 AnimId，等注册好再播）
            self.Play(AnimId.Idle);
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSAnimComponent self)
        {
            if (self.IsFinished) return;
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;
            self.IsLoop = clip.loop;   // 每 tick 从 clip 同步 loop 标志（clip 可能晚于 Play 注册）

            // FP 累加器，余数保留 → 跨平台无漂移；只有确定性的一次性量化抖动
            self.FrameTick += (FP)LSConstValue.UpdateInterval * self.Speed;
            int delay = clip.frames[self.FrameIndex].delay;
            if (delay <= 0) delay = LSConstValue.UpdateInterval;

            while (!self.IsFinished && self.FrameTick >= (FP)delay)
            {
                self.FrameTick -= (FP)delay;
                if (++self.FrameIndex >= clip.frames.Length)
                {
                    if (self.IsLoop) self.FrameIndex = 0;
                    else { self.FrameIndex = clip.frames.Length - 1; self.IsFinished = true; break; }
                }
                delay = clip.frames[self.FrameIndex].delay;
                if (delay <= 0) delay = LSConstValue.UpdateInterval;
            }
        }

        public static void Play(this LSAnimComponent self, int animId)
        {
            self.AnimId = animId;
            self.FrameIndex = 0;
            self.FrameTick = FP.Zero;
            self.Speed = FP.One;
            self.IsFinished = false;
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip == null) return;          // 未注册：先记 AnimId，等注册好 LSUpdate 自然开始推进
            self.IsLoop = clip.loop;
        }

        public static AnimFrameData GetCurrentFrame(this LSAnimComponent self)
        {
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return default;
            return clip.frames[self.FrameIndex];
        }
    }
}
