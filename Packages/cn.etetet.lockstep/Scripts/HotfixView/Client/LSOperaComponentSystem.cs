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
            if (Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)) button = 1;   // 攻击（按住持续，Button: 0=无 1=攻击）

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
            lsClientUpdater.Input.VY = vy;
            lsClientUpdater.Input.Button = button;
        }

    }
}