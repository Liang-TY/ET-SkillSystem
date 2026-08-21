using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>区域效果视图容器（Room 级）：轮询逻辑 LSAreaComponent 差分——新区域建 GO（循环火焰），消失销毁。</summary>
    [ComponentOf(typeof(Room))]
    public class LSAreaViewComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>Unit2D 预制体（区域视图复用单位的渲染层级）</summary>
        public GameObject Prefab;

        public readonly Dictionary<long, AreaViewInfo> Areas = new();
    }

    /// <summary>区域的视图运行时状态</summary>
    [EnableClass]
    public class AreaViewInfo
    {
        public GameObject Go;
        public SpriteRenderer Renderer;            // 主层（正面/循环）
        public Material OriginalMaterial;
        public SpriteRenderer BackRenderer;        // 背面层（ViewBackAnimId；null = 单层）
        public Material BackOriginalMaterial;
        public int AnimId;
        public int EndAnimId;
        public int FrameIndex;
        public float Timer;
        public int BackAnimId;
        public int BackFrameIndex;
        public float BackTimer;
        public bool Ending;   // 到时后播收尾动画（不循环），播完销毁
    }
}
