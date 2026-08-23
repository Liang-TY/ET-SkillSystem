using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(TownOperaComponent))]
    [FriendOf(typeof(TownOperaComponent))]
    [FriendOf(typeof(TownPlayerComponent))]
    [FriendOf(typeof(TownPlayerViewComponent))]
    [FriendOf(typeof(TownCollisionComponent))]   // 读 CellSize/CellSizeZ 算格比（ET0002）
    public static partial class TownOperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TownOperaComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this TownOperaComponent self)
        {
            Room room = self.GetParent<Room>();

            // ---- WASD 客户端权威移动（非锁步，render 帧直算；速度/格比与战斗同手感）----
            TownPlayerComponent player = room.GetComponent<TownPlayerComponent>();
            if (player != null)
            {
                TownCollisionComponent collision = room.GetComponent<TownCollisionComponent>();
                TSVector2 v = new();
                if (Input.GetKey(KeyCode.D)) v.x += 1;
                if (Input.GetKey(KeyCode.A)) v.x -= 1;
                if (Input.GetKey(KeyCode.W)) v.y += 1;
                if (Input.GetKey(KeyCode.S)) v.y -= 1;

                bool moving = v.LengthSquared() > FP.Zero;
                if (moving)
                {
                    v = v.normalized;
                    FP dt = (FP)(int)(Time.deltaTime * 1000) / 1000;
                    // 格子等速：z 分量乘格比（城镇格子同款非正方形，03 文档 §4.4）
                    FP zRatio = collision != null && collision.CellSize > FP.Zero && collision.CellSizeZ > FP.Zero
                        ? collision.CellSizeZ / collision.CellSize : FP.One;
                    TSVector delta = new(v.x * 6 * dt, FP.Zero, v.y * 6 * dt * zRatio);
                    if (collision != null)
                    {
                        collision.TryMove(player, delta);   // 网格阻挡+贴墙滑动
                    }
                    else
                    {
                        player.Position += delta;
                    }

                    if (v.x > FP.Zero) player.Forward = new TSVector(1, 0, 0);
                    else if (v.x < FP.Zero) player.Forward = new TSVector(-1, 0, 0);
                }

                // Idle↔Walk 切换
                TownPlayerViewComponent view = room.GetComponent<TownPlayerViewComponent>();
                if (view != null)
                {
                    view.AnimId = moving ? AnimId.SwordmanWalk : AnimId.SwordmanIdle;
                }
            }

            // ---- N：匹配进战斗（先记住城镇位置，回城恢复用，03 文档 §1.2）----
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (player != null) TownMemory.LastTownPosition = player.Position;
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
