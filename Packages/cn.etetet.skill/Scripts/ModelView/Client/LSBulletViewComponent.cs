using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 投射物视图容器（Room 级）：轮询逻辑 LSBulletComponent 的子弹做差分——新弹建 GO、消失销毁。
    /// 弹的动画在视图层自推（渲染时间，表现帧不影响逻辑）——逻辑 LSBullet 无动画状态。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class LSBulletViewComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>Unit2D 预制体（弹视图复用单位的渲染层级结构——摆位补偿才和单位一致）</summary>
        public GameObject Prefab;

        /// <summary>每发弹的视图状态（视图侧字典，非逻辑状态，不序列化）</summary>
        public readonly Dictionary<long, BulletViewInfo> Bullets = new();
    }

    /// <summary>弹的视图运行时状态（[EnableClass]：ModelView 允许字段的普通类）</summary>
    [EnableClass]
    public class BulletViewInfo
    {
        public GameObject Go;
        public SpriteRenderer Renderer;
        public Material OriginalMaterial;
        public int AnimId;
        public int FrameIndex;
        public float Timer;
        public bool FaceRight;
        public bool ViewGrounded = true;   // true=GO 贴地面（地波）；false=GO 用逻辑高度（空中弹）
        public Vector2 ViewOffset;         // 摆位补偿（CreateView 按朝向镜像 x；def.ViewOffset 直译）
        public System.Collections.Generic.List<OverlayViewInfo> Overlays;   // .als 叠加子层
    }
}
