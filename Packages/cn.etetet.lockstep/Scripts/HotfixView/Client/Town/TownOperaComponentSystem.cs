using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(TownOperaComponent))]
    [FriendOf(typeof(TownOperaComponent))]
    public static partial class TownOperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TownOperaComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this TownOperaComponent self)
        {
            // N：匹配进战斗（记住城镇位置在阶段 B 接入移动后补）
            if (Input.GetKeyDown(KeyCode.N))
            {
                EnterMapHelper.Match(self.Root().Fiber()).NoContext();
            }
            else if (Input.GetKeyDown(KeyCode.F9))   // 大厅 UI（回放调试入口保留）
            {
                UIHelper.Create(self.Root(), UIType.UILSLobby, UILayer.Mid).NoContext();
            }
        }

        [EntitySystem]
        private static void Destroy(this TownOperaComponent self)
        {
        }
    }
}
