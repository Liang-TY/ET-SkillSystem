using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视图层 .als 特效叠加（bloodboom.ani.als 同构，DNF 引擎零代码声明式叠加的客户端实现）：
    /// 监听逻辑层 AnimId 变化 → 有 AnimOverlayConfig 的动画动态建子 GO 叠加渲染；
    /// 父动画切走/组件销毁 → 全部销毁。帧推进用渲染 deltaTime（同弹/区域视图，逻辑层零参与）；
    /// startFrame 门控读逻辑层父动画 FrameIndex。
    /// 子 GO 挂单位根下：自动跟随位置 + 朝向镜像（LSUnitViewSystem 翻根 GO）。
    /// </summary>
    [ComponentOf(typeof(LSUnitView))]
    public class LSAnimOverlayViewComponent : Entity, IAwake, IUpdate, IDestroy, ILSRollback
    {
        /// <summary>当前父动画缓存（变化 = 重建叠加组）</summary>
        public int LastParentAnimId = -1;

        /// <summary>当前父动画的叠加层运行时状态（父动画切换时全清重建）</summary>
        public readonly List<OverlayViewInfo> Overlays = new();
    }

    /// <summary>单条叠加层的视图运行时状态</summary>
    [EnableClass]
    public class OverlayViewInfo
    {
        public GameObject Go;
        public SpriteRenderer Renderer;
        public Material OriginalMaterial;
        public AnimOverlayEntry Config;   // startFrame / z / effectAnimId
        public int FrameIndex;
        public float Timer;
    }
}
