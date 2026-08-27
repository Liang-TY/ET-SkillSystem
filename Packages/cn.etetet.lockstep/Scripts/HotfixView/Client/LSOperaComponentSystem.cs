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
            if (Input.GetKey(KeyCode.J)) button = 1;    // 普攻
            else if (Input.GetKey(KeyCode.K)) button = 2;   // CD/眩晕测试
            else if (Input.GetKey(KeyCode.I)) button = 3;   // 波动剑（投射物）
            else if (Input.GetKey(KeyCode.U)) button = 5;   // 浴血之怒（自耗 HP 血爆）
            else if (Input.GetKey(KeyCode.R)) button = 7;   // 调试：回出生点（位移出界救援，方案3）

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
            lsClientUpdater.Input.VY = vy;
            lsClientUpdater.Input.Button = button;
        }

    }
}