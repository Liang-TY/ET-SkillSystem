using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 资源依赖收集器：沿配置链收集 IMG 文件名集合（方案文档 §5）。
    /// 纯内存查找，不碰 IO——数据来自已加载的 AnimConfigRegistry + 视图配置。
    ///
    /// 三条收集链：
    ///   动画链：AnimConfigRegistry.GetAll() → 每个 AnimClipData.frames[].image.path
    ///   角色链：视图层 RenderConfig.Layers[].AtlasName（身体/武器，不走 JSON）
    ///   地图链：TileLayoutData.tiles[].imgPath + decorations[].imgPath
    /// </summary>
    public static class ResourceDependencyCollector
    {
        /// <summary>
        /// 收集所有已注册动画引用的 IMG 文件名 + 玩家角色常驻 IMG。
        /// 在 LSAnimResComponent.InitAsync 中调用（替代硬编码列表）。
        /// </summary>
        public static HashSet<string> CollectForAnimRes()
        {
            var imgs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // ① 动画链：遍历所有已注册 AnimClipData，提取 image.path
            foreach (var (_, clip) in AnimConfigRegistry.GetAll())
            {
                if (clip?.frames == null) continue;
                foreach (var frame in clip.frames)
                {
                    string path = frame.image?.path;
                    if (!string.IsNullOrEmpty(path))
                        imgs.Add(path);
                }
            }

            // ② 角色链：玩家身体+武器不走 JSON，由视图层直接引用——当前鬼剑士固定三件
            // TODO: 后续从角色配置驱动（classId → 身体/武器/时装 IMG 路径）
            imgs.Add("sm_body0000.img");
            imgs.Add("katana9200b.img");
            imgs.Add("katana9200c.img");

            return imgs;
        }

        /// <summary>
        /// 收集城镇瓦片引用的 IMG 文件名（从 TileLayoutData 提取）。
        /// 在 TownMapViewComponent.BuildTileTexture 中调用。
        /// </summary>
        public static HashSet<string> CollectFromTileLayout(TileLayoutData layout)
        {
            var imgs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            if (layout == null) return imgs;

            if (layout.tiles != null)
                foreach (var tile in layout.tiles)
                    if (!string.IsNullOrEmpty(tile?.imgPath))
                        imgs.Add(NormalizeImgName(tile.imgPath));

            if (layout.extendedTiles != null)
                foreach (var tile in layout.extendedTiles)
                    if (!string.IsNullOrEmpty(tile?.imgPath))
                        imgs.Add(NormalizeImgName(tile.imgPath));

            if (layout.decorations != null)
                foreach (var deco in layout.decorations)
                    if (!string.IsNullOrEmpty(deco?.imgPath))
                        imgs.Add(NormalizeImgName(deco.imgPath));

            return imgs;
        }

        /// <summary>"Aganzo.img" → "aganzo.img"（去后缀再统一格式 + 小写）</summary>
        private static string NormalizeImgName(string imgPath)
        {
            string name = imgPath;
            if (name.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase))
                name = name[..^4];
            return name.ToLowerInvariant() + ".img";
        }
    }
}
