using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ChildOf(typeof(LSUnitViewComponent))]
    public class LSUnitView: Entity, IAwake<GameObject>, IUpdate, IDestroy, ILSRollback
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
        public EntityRef<LSUnit> Unit;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool FaceRight = true;
        public SpriteRenderer SpriteRenderer;   // 单层渲染用（怪物）
        public UnitRenderConfig RenderConfig;   // 分层渲染配置（玩家多层；null=单层走 SpriteRenderer）
        public float totalTime;
        public float t;
    }

    /// <summary>单位分层渲染配置（DNF 换装：每图层一张图集，帧号一一对应）。
    /// 代码按 SortingOrder 匹配 prefab 里对应 sortingOrder 的 SpriteRenderer。</summary>
    [EnableClass]
    public class UnitRenderConfig
    {
        public List<RenderLayer> Layers = new();
    }

    /// <summary>单个渲染层。SortingOrder = prefab 里子 GO 的 sortingOrder（匹配 renderer）。</summary>
    [EnableClass]
    public class RenderLayer
    {
        public int SortingOrder;        // 对应 prefab 子 GO 的 sortingOrder
        public string AtlasName;        // 图集名（= img 文件名）
        public SpriteRenderer Renderer; // 运行时从 prefab 匹配到的 renderer
        public Material OriginalMaterial;
    }
}