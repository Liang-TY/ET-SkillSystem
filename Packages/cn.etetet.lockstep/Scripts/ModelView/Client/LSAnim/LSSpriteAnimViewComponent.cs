using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视图层动画：只读逻辑层 LSAnimComponent 的 AnimId/FrameIndex，diff 到变化就换 SpriteRenderer.sprite。
    /// 零模拟、不持帧状态；回滚时重置缓存强制重新同步。
    /// </summary>
    [ComponentOf(typeof(LSUnitView))]
    public class LSSpriteAnimViewComponent : Entity, IAwake, IUpdate, ILSRollback
    {
        public SpriteRenderer SpriteRenderer;
        public int LastAnimId = -1;
        public int LastFrameIndex = -1;
    }
}
