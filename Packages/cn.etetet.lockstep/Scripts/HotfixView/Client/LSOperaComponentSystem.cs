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
            // 移动：方向键（←→ 横向/转向，↑↓ 纵深）。字母键全部让位技能（2026-08-29 重排）
            TSVector2 v = new();
            if (Input.GetKey(KeyCode.RightArrow)) { v.x += 1; }
            if (Input.GetKey(KeyCode.LeftArrow)) { v.x -= 1; }
            if (Input.GetKey(KeyCode.UpArrow)) { v.y += 1; }
            if (Input.GetKey(KeyCode.DownArrow)) { v.y -= 1; }

            int button = 0;
            if (Input.GetKey(KeyCode.X)) button = 1;        // 普攻（DNF 同构，三段连击）
            else if (Input.GetKey(KeyCode.K)) button = 2;   // CD/眩晕测试
            else if (Input.GetKey(KeyCode.I)) button = 3;   // 波动剑（投射物）
            else if (Input.GetKey(KeyCode.U)) button = 5;   // 浴血之怒（自耗 HP 血爆）
            else if (Input.GetKey(KeyCode.G)) button = 11;  // 鬼斩（暗属性击倒）
            else if (Input.GetKey(KeyCode.Z)) button = 12;  // 上挑（浮空）
            else if (Input.GetKey(KeyCode.D)) button = 13;  // 三段斩（连段+前冲）
            else if (Input.GetKey(KeyCode.T)) button = 14;  // 连突刺（突刺+剑气弹）
            else if (Input.GetKey(KeyCode.F)) button = 15;  // 银光落刃（空中限定+落地冲击波）
            else if (Input.GetKey(KeyCode.C)) button = 16;  // 起跳（非技能，LSInputComponentSystem 消费）
            else if (Input.GetKey(KeyCode.R)) button = 7;   // 调试：回出生点（位移出界救援，方案3）

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
            lsClientUpdater.Input.VY = FP.Zero;   // C/V 升降调试已移除（C 让位起跳）；VY 通道保留
            lsClientUpdater.Input.Button = button;
        }

    }
}