using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇本地玩家视图（阶段B）：Unit2D prefab + 分层渲染（鬼剑士 3 层同战斗）。
    /// 城镇无逻辑实体——AnimId/FrameIndex 视图层自推（子弹视图同款模式），移动由 TownOpera 写 TownPlayerComponent。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownPlayerViewComponent: Entity, IAwake, IUpdate, ILateUpdate, IDestroy
    {
        public GameObject Root;

        /// <summary>跟随相机（MainCamera，LateUpdate 跟随本地玩家并 snap 到像素格）</summary>
        public Camera Camera;

        /// <summary>Unit2D 预制体缓存（InitAsync 载入；远端玩家视图复用）</summary>
        public GameObject UnitPrefab;

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
