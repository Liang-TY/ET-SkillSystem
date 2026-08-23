using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 地图瓦片视图（Room 级，按地图懒加载——与 AnimRes 全量加载的区别，03 文档 §3.2）：
    /// 进图时按 MapDefinition.TileLayoutPath 读 tile_layout.json——
    /// 解析结果进 MapTileLayoutCache（逻辑层 room.Init 建碰撞矩阵用）+ 瓦片 Blit 大贴图铺地面。
    /// 装饰物渲染（decoration_*.json）demo 阶段跳过（03 文档 §4.1）。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class LSMapViewComponent : Entity, IAwake, IDestroy
    {
        /// <summary>地面渲染 GO（Destroy 时销毁——Room 销毁连根拔，03 文档 §1.7）</summary>
        public GameObject Ground;

        /// <summary>碰撞调试叠图 GO（03 文档 §9 调试工具，Destroy 时销毁）</summary>
        public GameObject CollisionDebugOverlay;

        /// <summary>碰撞调试叠图纹理（运行时创建须显式销毁——GO 销毁不带走纹理）</summary>
        public Texture2D CollisionDebugTexture;
    }
}
