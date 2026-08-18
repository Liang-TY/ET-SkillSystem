using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSOperaComponent))]
    [FriendOf(typeof(LSClientUpdater))]
    public static partial class LSOperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSOperaComponent self)
        {

        }
        
        [EntitySystem]
        private static void Update(this LSOperaComponent self)
        {
            TSVector2 v = new();
            FP vy = FP.Zero;
            if (Input.GetKey(KeyCode.D)) { v.x += 1; }
            if (Input.GetKey(KeyCode.A)) { v.x -= 1; }
            if (Input.GetKey(KeyCode.W)) { v.y += 1; }  // z+
            if (Input.GetKey(KeyCode.S)) { v.y -= 1; }  // z-
            if (Input.GetKey(KeyCode.C)) { vy += 1; }    // y+
            if (Input.GetKey(KeyCode.V)) { vy -= 1; }    // y-

            int button = 0;
            if (Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)) button = 1;   // 普攻（按下沿触发，逻辑层检测）
            else if (Input.GetKey(KeyCode.K)) button = 2;                          // CD/眩晕测试技能（阶段4/5）
            else if (Input.GetKey(KeyCode.I)) button = 3;                          // 波动剑（阶段6投射物）

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
            lsClientUpdater.Input.VY = vy;
            lsClientUpdater.Input.Button = button;
        }

    }
}