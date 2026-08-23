using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇地图视图（阶段B，03 文档 §2.3）：读 hendonmyre_town 瓦片布局铺地面 + 建 TownCollisionComponent（客户端权威）
    /// + 碰撞调试叠图（绿=可走 红=阻挡——初版可视化检查用）。
    /// 与战斗 LSMapViewComponent 同管线但独立实现（战斗走 MapLoader/快照缓存，城镇无 MapDefinition；
    /// Blit/碰撞数学与战斗侧重复属预期，跑通后统一抽取）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownMapViewComponent: Entity, IAwake, IDestroy
    {
        public GameObject Ground;

        public GameObject CollisionDebugOverlay;

        public Texture2D CollisionDebugTexture;
    }
}
