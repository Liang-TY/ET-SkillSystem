using System.Collections.Generic;
using System.IO;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 地图瓦片视图系统：按 Room.MapId 读 MapDefinition.TileLayoutPath 懒加载（03 文档 §3.2/§4.1）。
    /// tile_layout.json 的 tiles[] 水平拼接 → 合并大 Texture2D 铺地面 + 合成碰撞矩阵进缓存。
    /// </summary>
    [EntitySystemOf(typeof(LSMapViewComponent))]
    [FriendOf(typeof(LSMapViewComponent))]
    [FriendOf(typeof(LSCollisionComponent))]   // 调试叠图读 PassGrid/CellSize/Origin（ET0002）
    public static partial class LSMapViewComponentSystem
    {
        private const int TileColumns = 14;   // DNF .til 固定 14 列
        private const int TileRows = 30;      // DNF .til 固定 30 行
        private const int CellSizePx = 80;    // DNF [img pos] 每格像素

        /// <summary>碰撞调试叠图开关（03 文档 §9）——碰撞对齐调通后置 false 即摘掉</summary>
        private const bool EnableCollisionDebugOverlay = true;

        [EntitySystem]
        private static void Awake(this LSMapViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSMapViewComponent self)
        {
            if (self.Ground != null)
            {
                // 运行时创建的 Sprite/Texture 是独立 UnityEngine.Object，GO 销毁不带走（顺手补：此前每进图泄漏 896x560 纹理）
                SpriteRenderer groundRenderer = self.Ground.GetComponent<SpriteRenderer>();
                Sprite groundSprite = groundRenderer != null ? groundRenderer.sprite : null;
                UnityEngine.Object.Destroy(self.Ground);
                if (groundSprite != null)
                {
                    UnityEngine.Object.Destroy(groundSprite.texture);
                    UnityEngine.Object.Destroy(groundSprite);
                }
                self.Ground = null;
            }
            DestroyCollisionDebugOverlay(self);
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
                        // DNF [pass type]：0=可通行，非 0（1/2/4）=阻挡——与之前假设相反！
                        flat[row * layout.gridWidth + gridCol] = tile.passTypes[srcIdx] == 0 ? '2' : '0';
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

            // 4) 地面 SpriteRenderer——100 PPU 原生分辨率（不缩放，DNF 贴图原生就是这个大小）
            GameObject ground = new("MapGround");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            ground.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -1000;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);

            // 记录贴图实际世界尺寸（碰撞网格的 CellSize 按此对齐——逻辑层 InitCollision 读）
            layout.visualWidth = (FP)width / 100;
            layout.visualHeight = (FP)height / 100;

            // 5) 摆位：大图中心对齐原点
            ground.transform.localPosition = new Vector3(0f, 0f, 0f);
            self.Ground = ground;
            Log.Info($"[LSMapView] 地面就绪：{width}x{height}px，{layout.tiles.Length} 瓦片，碰撞 {layout.gridWidth}x{layout.gridHeight} 格");
        }

        /// <summary>
        /// 碰撞调试叠图（03 文档 §9）：1px=1 格（绿=可走 红=阻挡，半透明），数据取运行时
        /// LSCollisionComponent.PassGrid——IsBlocked 实查的那份（源自战斗地图 .til 通行配置），非 json 原始数据。
        /// 摆放/尺寸按碰撞系统自身映射（Origin/CellSize）而非贴图矩形：碰撞与贴图没对齐时
        /// 叠图不盖满/偏移会直接显形——这正是本工具的诊断价值。
        /// 须在 room.Init（InitCollision）之后调用（LSSceneInitFinish 钩子）——BuildTileTexture 时碰撞组件还没建。
        /// </summary>
        public static void BuildCollisionDebugOverlay(this LSMapViewComponent self)
        {
            if (!EnableCollisionDebugOverlay) return;

            DestroyCollisionDebugOverlay(self);   // 幂等：二次调用先清旧的（public 扩展方法防未来加调用点）

            LSCollisionComponent collision = self.GetParent<Room>().LSWorld?.GetComponent<LSCollisionComponent>();
            if (collision?.PassGrid == null || collision.GridWidth <= 0 || collision.GridHeight <= 0
                || collision.CellSize <= FP.Zero || collision.CellSizeZ <= FP.Zero) return;

            int w = collision.GridWidth, h = collision.GridHeight;

            // 1px = 1 格。Y 翻转：网格 row 0 在世界顶部（OriginZ 侧），Unity 纹理 y=0 在底部
            Texture2D texture = new(w, h, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;   // Point 采样防绿红渗色
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32 walk = new(0, 255, 0, 128);
            Color32 block = new(255, 0, 0, 128);
            Color32[] buf = new Color32[w * h];
            int walkable = 0;
            for (int row = 0; row < h; row++)
            for (int col = 0; col < w; col++)
            {
                bool pass = collision.PassGrid[row * w + col] == 1;
                if (pass) walkable++;
                buf[(h - 1 - row) * w + col] = pass ? walk : block;
            }
            texture.SetPixels32(buf);
            texture.Apply(false, makeNoLongerReadable: true);

            // 世界矩形 = 碰撞自己的 [OriginX, OriginX+w*CellSize] × [OriginZ-h*CellSizeZ, OriginZ]
            // ppu 按 X 轴（1/CellSize → w px 恰好 = w*CellSize 世界宽）；格子非正方形 → Y 轴 localScale 补齐
            FP worldW = (FP)w * collision.CellSize;
            FP worldH = (FP)h * collision.CellSizeZ;
            GameObject overlay = new("CollisionDebugOverlay");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            overlay.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -999;   // 地面(-1000)之上、单位(10+)之下
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f), 1f / (float)collision.CellSize);
            overlay.transform.localPosition = new Vector3(
                (float)(collision.OriginX + worldW / 2), (float)(collision.OriginZ - worldH / 2), 0f);
            overlay.transform.localScale = new Vector3(1f, (float)(collision.CellSizeZ / collision.CellSize), 1f);

            self.CollisionDebugOverlay = overlay;
            self.CollisionDebugTexture = texture;
            Log.Info($"[LSMapView] 碰撞调试叠图：{w}x{h} 格，世界 {worldW}x{worldH}，cell=({collision.CellSize},{collision.CellSizeZ})，" +
                     $"origin=({collision.OriginX},{collision.OriginZ})，可走 {walkable}/{w * h}");
        }

        /// <summary>销毁叠图三件套（GO + 运行时 Sprite + 纹理——三者都是独立 UnityEngine.Object，互不连带）</summary>
        private static void DestroyCollisionDebugOverlay(LSMapViewComponent self)
        {
            if (self.CollisionDebugOverlay != null)
            {
                SpriteRenderer renderer = self.CollisionDebugOverlay.GetComponent<SpriteRenderer>();
                Sprite sprite = renderer != null ? renderer.sprite : null;
                UnityEngine.Object.Destroy(self.CollisionDebugOverlay);
                if (sprite != null) UnityEngine.Object.Destroy(sprite);
                self.CollisionDebugOverlay = null;
            }
            if (self.CollisionDebugTexture != null)
            {
                UnityEngine.Object.Destroy(self.CollisionDebugTexture);
                self.CollisionDebugTexture = null;
            }
        }
    }
}
