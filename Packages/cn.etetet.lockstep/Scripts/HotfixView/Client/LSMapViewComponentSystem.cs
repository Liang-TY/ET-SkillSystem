using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 地图瓦片视图系统：按 Room.MapId 读 MapDefinition.TileLayoutPath 懒加载（03 文档 §3.2/§4.1）。
    /// tile_layout.json 的 tiles[] 水平拼接 → 合并大 Texture2D 铺地面 + 合成碰撞矩阵进缓存。
    /// </summary>
    [EntitySystemOf(typeof(LSMapViewComponent))]
    [FriendOf(typeof(LSMapViewComponent))]
    public static partial class LSMapViewComponentSystem
    {
        private const int TileColumns = 14;   // DNF .til 固定 14 列
        private const int TileRows = 30;      // DNF .til 固定 30 行
        private const int CellSizePx = 80;    // DNF [img pos] 每格像素

        [EntitySystem]
        private static void Awake(this LSMapViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSMapViewComponent self)
        {
            if (self.Ground != null)
            {
                UnityEngine.Object.Destroy(self.Ground);
                self.Ground = null;
            }
        }

        public static async ETTask InitAsync(this LSMapViewComponent self)
        {
            Room room = self.GetParent<Room>();
            MapDefinition mapDef = MapLoader.Get(room.MapId);
            if (mapDef?.TileLayoutPath == null) return;

            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSMapView] ResourcesLoaderComponent 不存在，跳过地图加载");
                return;
            }

            TileLayoutData layout = await LoadTileLayout(resLoader, mapDef.TileLayoutPath);
            if (layout == null) return;

            // 合成派生字段（碰撞矩阵 + cellSize）
            DeriveLayout(layout);

            // 先进缓存再渲染（逻辑层 room.Init 只读缓存）
            MapTileLayoutCache.Set(mapDef.TileLayoutPath, layout);
            await BuildTileTexture(self, resLoader, mapDef.TileLayoutPath, layout);
        }

        /// <summary>读 tile_layout.json 并校验</summary>
        private static async ETTask<TileLayoutData> LoadTileLayout(ResourcesLoaderComponent resLoader, string jsonPath)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(jsonPath);
            if (asset == null)
            {
                Log.Warning($"[LSMapView] tile_layout.json 不存在：{jsonPath}——空地无碰撞");
                return null;
            }

            TileLayoutData layout = Newtonsoft.Json.JsonConvert.DeserializeObject<TileLayoutData>(asset.text);
            if (layout == null || layout.tiles == null || layout.tiles.Length == 0
                || layout.gridWidth <= 0 || layout.gridHeight <= 0)
            {
                Log.Error($"[LSMapView] tile_layout.json 数据不完整：{jsonPath}——空地无碰撞\n" +
                          $"  tiles={layout?.tiles?.Length ?? -1} gridW={layout?.gridWidth ?? -1} gridH={layout?.gridHeight ?? -1}");
                return null;
            }
            return layout;
        }

        /// <summary>从各瓦片压平 passTypes 合成全图碰撞矩阵 + 填 cellSizePx</summary>
        private static void DeriveLayout(TileLayoutData layout)
        {
            layout.cellSizePx = CellSizePx;
            char[] flat = new char[layout.gridWidth * layout.gridHeight];
            for (int i = 0; i < flat.Length; i++) flat[i] = '0';

            for (int t = 0; t < layout.tiles.Length; t++)
            {
                TileLayoutTile tile = layout.tiles[t];
                if (tile?.passTypes == null) continue;
                int baseCol = t * TileColumns;
                for (int row = 0; row < TileRows; row++)
                {
                    for (int col = 0; col < TileColumns; col++)
                    {
                        int srcIdx = row * TileColumns + col;   // 压平索引
                        if (srcIdx >= tile.passTypes.Length) break;
                        int gridCol = baseCol + col;
                        if (gridCol >= layout.gridWidth) break;
                        flat[row * layout.gridWidth + gridCol] = tile.passTypes[srcIdx] == 2 ? '2' : '0';
                    }
                }
            }
            layout.passTypes = new string(flat);
        }

        /// <summary>瓦片帧 Blit 到一张大 Texture2D → SpriteRenderer 铺地面</summary>
        private static async ETTask BuildTileTexture(
            LSMapViewComponent self, ResourcesLoaderComponent resLoader, string layoutPath, TileLayoutData layout)
        {
            string dir = Path.GetDirectoryName(layoutPath)?.Replace('\\', '/');

            // 1) 解析去重后的瓦片图集
            Dictionary<string, NpkSprite[]> atlases = new(System.StringComparer.OrdinalIgnoreCase);
            foreach (TileLayoutTile tile in layout.tiles)
            {
                if (tile?.imgPath == null) continue;
                string imgName = (tile.imgPath.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase)
                    ? tile.imgPath[..^4] : tile.imgPath).ToLowerInvariant();   // 统一小写（YooAsset 路径大小写敏感）
                if (atlases.ContainsKey(imgName)) continue;
                TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>($"{dir}/{imgName}.img.bytes");
                atlases[imgName] = imgAsset != null ? NpkImgParser.Parse(imgAsset.bytes) : null;
                if (imgAsset == null) Log.Warning($"[LSMapView] 瓦片图集不存在：{imgName}.img.bytes——地面缺帧");
            }

            // 2) 大图尺寸（各瓦片帧 Blit 位置 = tile 序号 × 帧宽，水平拼）
            int width = 0, height = 0;
            foreach (KeyValuePair<string, NpkSprite[]> kv in atlases)
            {
                if (kv.Value == null || kv.Value.Length == 0) continue;
                NpkSprite s = kv.Value[0];
                width = System.Math.Max(width, s.FrameWidth);
                height = System.Math.Max(height, s.FrameHeight);
            }
            if (width <= 0 || height <= 0)
            {
                Log.Warning("[LSMapView] 瓦片帧全空——不铺地面（碰撞矩阵不受影响）");
                return;
            }
            width *= layout.tiles.Length;   // 水平拼 N 张

            // 3) 逐瓦片像素 Blit
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] buf = new Color32[width * height];
            // 初始化透明
            for (int i = 0; i < buf.Length; i++) buf[i] = new Color32(0, 0, 0, 0);

            int tileWidth = width / layout.tiles.Length;
            for (int t = 0; t < layout.tiles.Length; t++)
            {
                TileLayoutTile tile = layout.tiles[t];
                if (tile?.imgPath == null) continue;
                string imgName = (tile.imgPath.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase)
                    ? tile.imgPath[..^4] : tile.imgPath).ToLowerInvariant();
                if (!atlases.TryGetValue(imgName, out NpkSprite[] frames) || frames == null) continue;
                if (tile.imgFrame < 0 || tile.imgFrame >= frames.Length) continue;
                NpkSprite s = frames[tile.imgFrame];
                if (s.ArgbData == null) continue;

                int offsetX = t * tileWidth;
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    int px = offsetX + x;
                    if (px >= width) continue;
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = height - 1 - y;   // Y 翻转
                    if (dstY < 0 || dstY >= height) continue;
                    buf[dstY * width + px] = new Color32(
                        (byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF), (byte)((argb >> 24) & 0xFF));
                }
            }
            texture.SetPixels32(buf);
            texture.Apply(false, makeNoLongerReadable: true);

            // 4) 地面 SpriteRenderer——PPU 按碰撞网格的世界尺寸反算（贴图像素 ≠ 网格逻辑像素，需缩放对齐）
            GameObject ground = new("MapGround");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            ground.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -1000;
            // 逻辑世界宽度 = gridWidth × CellSize（如 56×0.8=44.8 单位）
            // 贴图世界宽度 = width / PPU → PPU = width / 逻辑宽度（如 896/44.8=20）
            float logicalWidth = (float)layout.gridWidth * (float)layout.cellSizePx / 100f;
            float ppu = width / logicalWidth;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), ppu);

            // 5) 摆位：大图中心对齐原点
            ground.transform.localPosition = new Vector3(0f, 0f, 0f);
            self.Ground = ground;
            Log.Info($"[LSMapView] 地面就绪：{width}x{height}px，{layout.tiles.Length} 瓦片，碰撞 {layout.gridWidth}x{layout.gridHeight} 格");
        }
    }
}
