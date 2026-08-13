using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 调试镜像：运行时选中 unit 物体，Inspector 实时看逻辑层动画状态。
    /// 仅编辑器编译。挂 unit prefab 上。
    /// 放 ET.Loader 程序集（ET 的 Model/Hotfix/ModelView/HotfixView 都禁止普通 MonoBehaviour）。
    /// ET 实体不是 MonoBehaviour，所以由 LSSpriteAnimViewComponentSystem.Update（ET.HotfixView，引用了 ET.Loader）
    /// 把状态推到这里的 public 字段，Inspector 才能显示。
    /// </summary>
    public class LSUnitViewDebug : MonoBehaviour
    {
        [Header("逻辑层动画状态（只读镜像）")]
        public int AnimId;
        public int FrameIndex;
        public float FrameTick;
        public float Speed;
        public bool IsLoop;
        public bool IsFinished;

        [Header("渲染状态")]
        public bool FaceRight;
        public string SpriteName;
    }
}
