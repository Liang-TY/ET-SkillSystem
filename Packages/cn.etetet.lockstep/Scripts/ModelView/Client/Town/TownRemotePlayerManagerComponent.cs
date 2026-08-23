using System.Collections.Generic;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇多人组件（阶段D，03 文档 §2.1/§2.2）：本地位置上报 + 远端角色渲染，一体。
    /// 同步开关在 System 的 const（默认关——单人 demo 零发包零开销；打开后多人互见）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownRemotePlayerManagerComponent: Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>本地：上次已上报位置/朝向（位置没变跳过）</summary>
        public TSVector LastSentPos;

        public TSVector LastSentForward;

        /// <summary>本地：下次上报时刻（ClientNow；移动中 200ms / 静止 1000ms）</summary>
        public long NextSendTime;

        /// <summary>本地：上一帧位置（判移动中）</summary>
        public TSVector LastFramePos;

        /// <summary>本地：上一帧是否在报"移动中"（停止瞬间发终包）</summary>
        public bool LastMoving;
    }

    /// <summary>单个远端角色视图（child of manager）：Unit2D 3 层渲染 + 自推帧 + 插值</summary>
    [ChildOf(typeof (TownRemotePlayerManagerComponent))]
    public class TownRemotePlayerView: Entity, IAwake, IDestroy
    {
        public long PlayerId;

        public GameObject Root;

        public UnitRenderConfig RenderConfig;

        public int AnimId;

        public int FrameIndex;

        public float Timer;

        public bool FaceRight = true;

        public int LastAnimId = -1;

        public int LastFrameIndex = -1;

        // ---- 插值状态（03 文档 §2.2：收包后 ~200ms 平滑走完；IsMoving 沿 Forward 外推）----
        public TSVector TargetPos;

        public TSVector TargetForward;

        public bool IsMoving;

        /// <summary>当前显示位置（Unity 世界坐标，float 插值）</summary>
        public Vector3 DisplayPos;
    }
}
