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

        /// <summary>受击闪白剩余时间（秒；>0 时 sprite 白色高亮。SkillSystemConfig.HitFlashEnabled 控制）</summary>
        public float FlashTimer;

        /// <summary>原始材质缓存（Awake 时抓取；加法混合帧切走后要切回来，设 null 会丢 shader→粉红）</summary>
        public Material OriginalMaterial;
    }
}
