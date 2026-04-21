using UnityEngine;

namespace ET
{
    [ChildOf(typeof(LSUnitViewComponent))]
    public class LSUnitView: Entity, IAwake<GameObject>, IUpdate, ILSRollback
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
        public EntityRef<LSUnit> Unit;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool FaceRight = true;       // 新增：朝向
        public SpriteRenderer SpriteRenderer; // 新增：用于排序和翻转
        public float totalTime;
        public float t;
    }
}