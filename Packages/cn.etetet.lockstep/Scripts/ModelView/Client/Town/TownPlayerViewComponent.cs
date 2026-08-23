using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇本地玩家视图（阶段B）：Unit2D prefab + 分层渲染（鬼剑士 3 层同战斗）。
    /// 城镇无逻辑实体——AnimId/FrameIndex 视图层自推（子弹视图同款模式），移动由 TownOpera 写 TownPlayerComponent。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownPlayerViewComponent: Entity, IAwake, IDestroy
    {
        public GameObject Root;

        public UnitRenderConfig RenderConfig;

        /// <summary>当前动画（AnimId.SwordmanIdle / SwordmanWalk，移动方切换）</summary>
        public int AnimId;

        public int FrameIndex;

        public float Timer;

        public bool FaceRight = true;

        public int LastAnimId = -1;

        public int LastFrameIndex = -1;
    }
}
