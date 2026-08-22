using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 地图瓦片视图系统：按 Room.MapId 读 MapDefinition.TileLayoutPath 懒加载（03 文档 §3.2/§4.1）——
    /// 1) tile_layout.json → MapTileLayoutCache（逻辑层 room.Init 建碰撞矩阵——本组件必须在 room.Init 前完成，
    ///    由 LSSceneChangeStart_AddComponent 的 await PublishAsync 时序保证）；
    /// 2) 瓦片 img 逐帧 Blit 到一张大 Texture2D → SpriteRenderer 铺地面（不打包图集，同 LSAnimRes 解析方式）。
    /// </summary>
    [EntitySystemOf(typeof(LSMapViewComponent))]
    [FriendOf(typeof(LSMapViewComponent))]
    public static partial class LSMapViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSMapViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSMapViewComponent self)
        {
            if (self.Ground != null)
            {
                UnityEngine.Object.Destroy(self.Ground);   // 全限定：避开 ET.Object
                self.Ground = null;
            }
        }

        /// <summary>按当前 Room.MapId 加载地图（SkillContentLoader 之后调——MapLoader 那时才注册完）</summary>
        public static async ETTask InitAsync(this LSMapViewComponent self)
        {
            Room room = self.GetParent<Room>();
            MapDefinition mapDef = MapLoader.Get(room.MapId);
            if (mapDef?.TileLayoutPath == null) return;   // 空地（mapId=0 或未配瓦片）——无地面无碰撞

            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSMapView] ResourcesLoaderComponent 不存在，跳过地图加载");
                return;
            }

            TileLayoutData layout = await LoadTileLayout(resLoader, mapDef.TileLayoutPath);
            if (layout == null) return;

            // 先进缓存再渲染：room.Init（碰撞矩阵）只依赖缓存，渲染失败不影响逻辑
            MapTileLayoutCache.Set(mapDef.TileLayoutPath, layout);
            await BuildTileTexture(self, resLoader, mapDef.TileLayoutPath, layout);
        }

        /// <summary>读 tile_layout.json（瓦片布局 + 碰撞矩阵；翻译工具 til 子命令产物）</summary>
        private static async ETTask<TileLayoutData> LoadTileLayout(ResourcesLoaderComponent resLoader, string jsonPath)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(jsonPath);
            if (asset == null)
            {
                Log.Warning($"[LSMapView] tile_layout.json 不存在：{jsonPath}（翻译工具 til 子命令产物）——空地无碰撞");
                return null;
            }

            TileLayoutData layout = JsonUtility.FromJson<TileLayoutData>(asset.text);
            if (layout == null || layout.gridWidth <= 0 || layout.gridHeight <= 0
                || layout.passTypes == null || layout.passTypes.Length < layout.gridWidth * layout.gridHeight
                || layout.tiles == null || layout.tiles.Length == 0)
            {
                Log.Error($"[LSMapView] tile_layout.json 数据不完整：{jsonPath}——空地无碰撞");
                return null;
            }
            return layout;
        }

        /// <summary>
        /// 瓦片帧 Blit 到一张大 Texture2D → SpriteRenderer 铺地面。
        /// 瓦片图集按需逐个解析（NpkImgParser，同 LSAnimResComponentSystem.BuildAtlas 的取帧方式），
        /// 不打包图集——每帧直接画到大图对应位置。
        /// </summary>
        private static async ETTask BuildTileTexture(
            LSMapViewComponent self, ResourcesLoaderComponent resLoader, string layoutPath, TileLayoutData layout)
        {
            string dir = Path.GetDirectoryName(layoutPath)?.Replace('\\', '/');

            // 1) 解析去重后的瓦片图集（多瓦片常共用一张 img，如 4 张 .til 全引 aganzo.img 不同帧）
            Dictionary<string, NpkSprite[]> atlases = new(System.StringComparer.OrdinalIgnoreCase);
            foreach (TileLayoutTile tile in layout.tiles)
            {
                if (tile == null || tile.imgName == null || atlases.ContainsKey(tile.imgName)) continue;
                TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>($"{dir}/{tile.imgName}.img.bytes");
                if (imgAsset == null)
                {
                    Log.Warning($"[LSMapView] 瓦片图集不存在：{tile.imgName}.img.bytes（同目录）——地面缺帧");
                    atlases[tile.imgName] = null;
                    continue;
                }
                atlases[tile.imgName] = NpkImgParser.Parse(imgAsset.bytes);
            }

            // 2) 大图尺寸 = 全部瓦片帧的包围盒（大图坐标 = 瓦片 Blit 位置的像素空间）
            int width = 0, height = 0;
            foreach (TileLayoutTile tile in layout.tiles)
            {
                if (tile == null || !atlases.TryGetValue(tile.imgName, out NpkSprite[] frames)
                    || frames == null || tile.frame < 0 || tile.frame >= frames.Length) continue;
                NpkSprite s = frames[tile.frame];
                if (s.ArgbData == null) continue;   // 引用帧无数据（翻译工具应引实体帧）
                width = System.Math.Max(width, tile.x + s.Width);
                height = System.Math.Max(height, tile.y + s.Height);
            }
            if (width <= 0 || height <= 0)
            {
                Log.Warning("[LSMapView] 瓦片帧全空——不铺地面（碰撞矩阵不受影响）");
                return;
            }

            // 3) 逐瓦片像素 Blit（ARGB int → RGBA 字节，Y 翻转：大图 top-left ↔ Unity bottom-left）
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;       // 像素图防糊
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] buf = new Color32[width * height];
            foreach (TileLayoutTile tile in layout.tiles)
            {
                if (tile == null || !atlases.TryGetValue(tile.imgName, out NpkSprite[] frames) || frames == null) continue;
                if (tile.frame < 0 || tile.frame >= frames.Length)
                {
                    Log.Warning($"[LSMapView] {tile.imgName}[{tile.frame}] 越界（{frames.Length} 帧）——该瓦片空白");
                    continue;
                }
                NpkSprite s = frames[tile.frame];
                if (s.ArgbData == null)
                {
                    Log.Warning($"[LSMapView] {tile.imgName}[{tile.frame}] 无像素数据（引用帧）——该瓦片空白");
                    continue;
                }
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    if (tile.x + x >= width || tile.y + y >= height) continue;   // 帧越界部分裁掉
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = height - 1 - tile.y - y;
                    buf[dstY * width + tile.x + x] = new Color32(
                        (byte)((argb >> 16) & 0xFF),   // R
                        (byte)((argb >>  8) & 0xFF),   // G
                        (byte)((argb        ) & 0xFF), // B
                        (byte)((argb >> 24) & 0xFF));  // A
                }
            }
            texture.SetPixels32(buf);
            texture.Apply(false, makeNoLongerReadable: true);   // 上传 GPU 后释放 CPU 副本

            // 4) 地面 SpriteRenderer：单位间深度排序占 [-1000,0]（LSUnitViewSystem: order=-z*100，
            //    行走带 z<10）——地面取 -1000 垫底（方案草案的 -10 会被深处单位盖住，弃用）
            GameObject ground = new("MapGround");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            ground.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -1000;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);

            // 5) 摆位：大图像素列 px ↔ 世界 x = px/100（无压缩，中线对齐）；
            //    行 py ↔ 深度 z = py/100，屏幕纵向有 0.6 纵深压缩（LSUnitViewSystem depthRatio）——
            //    只在可行走带中线严格对齐（带内上下缘各漂 0.4×带宽/2，demo 精度）
            int anchorPx = WalkBandCenterPx(layout);
            ground.transform.localPosition = new Vector3(
                width / 200f,
                (1.6f * anchorPx - height * 0.5f) / 100f,
                0f);
            self.Ground = ground;
            Log.Info($"[LSMapView] 地面就绪：{width}x{height}px，{layout.tiles.Length} 瓦片，碰撞 {layout.gridWidth}x{layout.gridHeight} 格");
        }

        /// <summary>可行走带的垂直中线（像素行）：碰撞矩阵里含可走格的行范围中点——地面与单位对齐的锚</summary>
        private static int WalkBandCenterPx(TileLayoutData layout)
        {
            int minRow = -1, maxRow = -1;
            for (int row = 0; row < layout.gridHeight; row++)
            {
                for (int col = 0; col < layout.gridWidth; col++)
                {
                    if (layout.passTypes[row * layout.gridWidth + col] != '2') continue;
                    if (minRow < 0) minRow = row;
                    maxRow = row;
                    break;
                }
            }
            if (minRow < 0) return layout.gridHeight * layout.cellSizePx / 2;   // 全阻挡：退化为图中线
            return (minRow + maxRow + 1) * layout.cellSizePx / 2;
        }
    }
}
