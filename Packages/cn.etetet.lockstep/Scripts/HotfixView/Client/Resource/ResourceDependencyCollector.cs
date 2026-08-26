using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 资源依赖收集器：沿配置链收集 IMG 文件名集合（方案文档 §5）。
    /// 纯内存查找，不碰 IO——数据来自已加载的 AnimConfigRegistry + 视图配置。
    /// </summary>
    public static class ResourceDependencyCollector
    {
        // ---- 玩家角色常驻 IMG（不走 JSON，由视图层直接引用）----
        // TODO: 后续从角色配置驱动（classId → 身体/武器/时装 IMG 路径）
        private static readonly string[] PlayerPermanent = new string[]
        {
            "sm_body0000.img",
            "katana9200b.img",
            "katana9200c.img",
        };

        /// <summary>收集玩家角色常驻 IMG（body + weapon）</summary>
        public static HashSet<string> CollectCharacter()
        {
            var imgs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (string s in PlayerPermanent)
                imgs.Add(s);
            return imgs;
        }

        /// <summary>
        /// 收集城镇场景所需的全部 IMG：角色常驻 + 城镇瓦片。
        /// 不含怪物/副本特效。
        /// </summary>
        public static HashSet<string> CollectForTown(TileLayoutData tileLayout = null)
        {
            var imgs = CollectCharacter();

            if (tileLayout != null)
                imgs.UnionWith(CollectFromTileLayout(tileLayout));

            return imgs;
        }

        /// <summary>
        /// 收集副本场景所需的全部 IMG：角色常驻 + 角色全部技能特效 + 怪物动画。
        /// 不含城镇瓦片。
        /// </summary>
        public static HashSet<string> CollectForDungeon()
        {
            var imgs = CollectCharacter();

            // 城镇不需要的动画：怪物段（MonsterLowKick=42 到 IceBreathBullet2=48）
            // + 玩家技能特效（从 AnimClipData 收集）
            // 当前简单做法：收集所有已注册动画的 IMG（除了城镇瓦片）
            // 后续精确化：只收集 MapDefinition.MonsterAiIds 对应的怪物 + 该职业的技能
            foreach (var (_, clip) in AnimConfigRegistry.GetAll())
            {
                if (clip?.frames == null) continue;
                foreach (var frame in clip.frames)
                {
                    string path = frame.image.path;
                    if (!string.IsNullOrEmpty(path))
                        imgs.Add(path);
                }
            }

            return imgs;
        }

        /// <summary>从瓦片布局收集 IMG 文件名</summary>
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

        /// <summary>"Aganzo.img" → "aganzo.img"</summary>
        private static string NormalizeImgName(string imgPath)
        {
            string name = imgPath;
            if (name.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase))
                name = name[..^4];
            return name.ToLowerInvariant() + ".img";
        }
    }
}
