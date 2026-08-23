using System.Collections.Generic;
using System.IO;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇地图视图系统（阶段B，03 文档 §2/§4.4）：hendonmyre_tile_layout.json → 铺地面 + TownCollisionComponent + 调试叠图。
    /// 管线与战斗 LSMapViewComponentSystem 同构（拷贝适配——战斗侧走 MapLoader/MapTileLayoutCache，城镇直读路径）。
    /// </summary>
    [EntitySystemOf(typeof(TownMapViewComponent))]
    [FriendOf(typeof(TownMapViewComponent))]
    [FriendOf(typeof(TownCollisionComponent))]
    [FriendOf(typeof(TownPlayerComponent))]   // TryMove 写 Position
    public static partial class TownMapViewComponentSystem
    {
        private const int TileColumns = 14;   // DNF .til 固定 14 列（同战斗）
        private const int TileRows = 30;      // DNF .til 固定 30 行

        /// <summary>城镇瓦片布局路径（demo 唯一城镇：赫顿玛尔 5 切片）</summary>
        private const string TileLayoutPath =
            "Packages/cn.etetet.lockstep/Bundles/MapRes/hendonmyre_town/hendonmyre_tile_layout.json";

        /// <summary>碰撞调试叠图开关——绿区与街道美术核对无误后置 false</summary>
        private const bool EnableCollisionDebugOverlay = true;

        [EntitySystem]
        private static void Awake(this TownMapViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TownMapViewComponent self)
        {
            if (self.Ground != null)
            {
                // 运行时创建的 Sprite/Texture 是独立 UnityEngine.Object，GO 销毁不带走
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

        public static async ETTask InitAsync(this TownMapViewComponent self)
        {
            Room room = self.GetParent<Room>();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[TownMapView] ResourcesLoaderComponent 不存在，跳过城镇地图加载");
                return;
            }

            TileLayoutData layout = await LoadTileLayout(resLoader, TileLayoutPath);
            if (layout == null) return;

            await BuildTileTexture(self, resLoader, layout);
            if (self.BaseFrameHeight <= 0) return;   // 贴图全空（BuildTileTexture 早退）——不建碰撞
            BuildCollision(room, layout, self);
            BuildCollisionDebugOverlay(self, room.GetComponent<TownCollisionComponent>());
        }

        /// <summary>读 tile_layout.json 并校验（同战斗 LoadTileLayout）</summary>
        private static async ETTask<TileLayoutData> LoadTileLayout(ResourcesLoaderComponent resLoader, string jsonPath)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(jsonPath);
            if (asset == null)
            {
                Log.Warning($"[TownMapView] tile_layout.json 不存在：{jsonPath}");
                return null;
            }

            TileLayoutData layout = Newtonsoft.Json.JsonConvert.DeserializeObject<TileLayoutData>(asset.text);
            if (layout == null || layout.tiles == null || layout.tiles.Length == 0
                || layout.gridWidth <= 0 || layout.gridHeight <= 0)
            {
                Log.Error($"[TownMapView] tile_layout.json 数据不完整：{jsonPath}");
                return null;
            }
            return layout;
        }

        /// <summary>瓦片帧 Blit 到一张大 Texture2D → SpriteRenderer 铺地面（战斗管线 + 城镇扩展条）</summary>
        private static async ETTask BuildTileTexture(
            TownMapViewComponent self, ResourcesLoaderComponent resLoader, TileLayoutData layout)
        {
            string dir = Path.GetDirectoryName(TileLayoutPath)?.Replace('\\', '/');

            // 主瓦片 + 扩展瓦片的图集都进同一字典
            Dictionary<string, NpkSprite[]> atlases = new(System.StringComparer.OrdinalIgnoreCase);
            async ETTask LoadAtlas(string imgPathRaw)
            {
                if (imgPathRaw == null) return;
                string imgName = (imgPathRaw.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase)
                    ? imgPathRaw[..^4] : imgPathRaw).ToLowerInvariant();
                if (atlases.ContainsKey(imgName)) return;
                TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>($"{dir}/{imgName}.img.bytes");
                atlases[imgName] = imgAsset != null ? NpkImgParser.Parse(imgAsset.bytes) : null;
                if (imgAsset == null) Log.Warning($"[TownMapView] 瓦片图集不存在：{imgName}.img.bytes");
            }
            foreach (TileLayoutTile tile in layout.tiles) await LoadAtlas(tile?.imgPath);
            if (layout.extendedTiles != null)
            {
                foreach (TileLayoutTile tile in layout.extendedTiles) await LoadAtlas(tile?.imgPath);
            }
            if (layout.decorations != null)
            {
                foreach (TileDecoration deco in layout.decorations) await LoadAtlas(deco?.imgPath);
            }

            NpkSprite? FrameOf(string imgPathRaw, int frameIdx)
            {
                if (imgPathRaw == null) return null;
                string imgName = (imgPathRaw.EndsWith(".img", System.StringComparison.OrdinalIgnoreCase)
                    ? imgPathRaw[..^4] : imgPathRaw).ToLowerInvariant();
                if (!atlases.TryGetValue(imgName, out NpkSprite[] frames) || frames == null) return null;
                if (frameIdx < 0 || frameIdx >= frames.Length) return null;
                return frames[frameIdx];
            }

            int width = 0, baseHeight = 0, extHeight = 0;
            foreach (TileLayoutTile tile in layout.tiles)
            {
                NpkSprite? s = FrameOf(tile?.imgPath, tile?.imgFrame ?? 0);
                if (s == null) continue;
                width = System.Math.Max(width, s.Value.FrameWidth);
                baseHeight = System.Math.Max(baseHeight, s.Value.FrameHeight);
            }
            if (layout.extendedTiles != null)
            {
                foreach (TileLayoutTile tile in layout.extendedTiles)
                {
                    NpkSprite? s = FrameOf(tile?.imgPath, tile?.imgFrame ?? 0);
                    if (s == null) continue;
                    extHeight = System.Math.Max(extHeight, s.Value.FrameHeight);
                }
            }
            if (width <= 0 || baseHeight <= 0)
            {
                Log.Warning("[TownMapView] 瓦片帧全空——不铺地面");
                return;
            }
            width *= layout.tiles.Length;
            int height = baseHeight + extHeight;   // 扩展条拼在主瓦片下方（.map [extended tile]）
            self.BaseFrameHeight = baseHeight;
            self.TotalFrameHeight = height;

            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] buf = new Color32[width * height];
            for (int i = 0; i < buf.Length; i++) buf[i] = new Color32(0, 0, 0, 0);

            int tileWidth = width / layout.tiles.Length;
            void Blit(TileLayoutTile tile, int t, int baseTop)
            {
                NpkSprite? got = FrameOf(tile?.imgPath, tile?.imgFrame ?? 0);
                if (got?.ArgbData == null) return;
                NpkSprite s = got.Value;
                int offsetX = t * tileWidth;
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    // 帧域偏移（城镇 hmtile 是部分帧：图像 224x138 画在 224x480 帧域 pos(0,342)）；
                    // baseTop = 该瓦片区在画布顶起算的起始 y（主=0，扩展=baseHeight）
                    int px = offsetX + s.X + x;
                    if (px >= width) continue;
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = height - 1 - (baseTop + s.Y + y);   // 画布内偏移后整体 Y 翻转
                    if (dstY < 0 || dstY >= height) continue;
                    buf[dstY * width + px] = new Color32(
                        (byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF), (byte)((argb >> 24) & 0xFF));
                }
            }

            // 装饰 Blit：绝对坐标（.map 摆放坐标 + .ani IMAGE POS 已合成进 json）；透明像素跳过（不凿穿上层）
            void DecoBlit(string imgPath, int frameIdx, int x, int y)
            {
                NpkSprite? got = FrameOf(imgPath, frameIdx);
                if (got?.ArgbData == null) return;
                NpkSprite s = got.Value;
                for (int yy = 0; yy < s.Height; yy++)
                for (int xx = 0; xx < s.Width; xx++)
                {
                    int argb = s.ArgbData[yy * s.Width + xx];
                    if ((argb >> 24 & 0xFF) == 0) continue;   // 透明跳过——装饰叠画不覆盖下层
                    int px = x + s.X + xx;
                    if (px < 0 || px >= width) continue;
                    int dstY = height - 1 - (y + s.Y + yy);
                    if (dstY < 0 || dstY >= height) continue;
                    buf[dstY * width + px] = new Color32(
                        (byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF), (byte)((argb >> 24) & 0xFF));
                }
            }

            void BlitDecorations(string layer, bool tileX)
            {
                if (layout.decorations == null) return;
                foreach (TileDecoration deco in layout.decorations)
                {
                    if (deco?.imgPath == null || deco.layer != layer) continue;
                    if (tileX)
                    {
                        NpkSprite? f = FrameOf(deco.imgPath, deco.imgFrame);
                        int fw = f?.FrameWidth ?? 0;
                        if (fw <= 0) continue;
                        for (int bx = 0; bx < width; bx += fw) DecoBlit(deco.imgPath, deco.imgFrame, bx, deco.y);
                    }
                    else
                    {
                        DecoBlit(deco.imgPath, deco.imgFrame, deco.x, deco.y);
                    }
                }
            }

            // 烘焙次序（DNF 图层序）：远/中景背景（平铺）→ closeback 装饰 → 主瓦片 → 扩展瓦片 → bottom/normal 装饰
            BlitDecorations("distantback", true);
            BlitDecorations("middleback", true);
            BlitDecorations("closeback", false);
            for (int t = 0; t < layout.tiles.Length; t++) Blit(layout.tiles[t], t, 0);
            if (layout.extendedTiles != null)
            {
                for (int t = 0; t < layout.extendedTiles.Length && t < layout.tiles.Length; t++)
                    Blit(layout.extendedTiles[t], t, baseHeight);
            }
            BlitDecorations("bottom", false);
            BlitDecorations("normal", false);
            texture.SetPixels32(buf);
            texture.Apply(false, makeNoLongerReadable: true);

            GameObject ground = new("TownMapGround");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            ground.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -1000;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            ground.transform.localPosition = new Vector3(0f, 0f, 0f);
            self.Ground = ground;

            layout.visualWidth = (FP)width / 100;
            layout.visualHeight = (FP)baseHeight / 100;   // 碰撞参考高=主瓦片区（扩展条不进碰撞）
            Log.Info($"[TownMapView] 城镇地面就绪：{width}x{height}px（主 {baseHeight}+扩 {extHeight}），" +
                     $"{layout.tiles.Length} 瓦片，网格 {layout.gridWidth}x{layout.gridHeight}");
        }

        /// <summary>建 TownCollisionComponent（X 对齐总宽、Z 对齐主瓦片高；OriginZ = 总高/2 = 画布顶）</summary>
        private static void BuildCollision(Room room, TileLayoutData layout, TownMapViewComponent mapView)
        {
            TownCollisionComponent collision = room.AddComponent<TownCollisionComponent>();
            collision.GridWidth = layout.gridWidth;
            collision.GridHeight = layout.gridHeight;
            collision.CellSize = layout.visualWidth / collision.GridWidth;
            collision.CellSizeZ = layout.visualHeight / collision.GridHeight;
            collision.OriginX = -(FP)collision.GridWidth * collision.CellSize / 2;
            // 画布（含扩展条）中心对齐世界原点 → row0（画布顶）的世界 z = 总高/2
            collision.OriginZ = (FP)mapView.TotalFrameHeight / 100 / 2;

            collision.PassGrid = new byte[layout.gridWidth * layout.gridHeight];
            for (int t = 0; t < layout.tiles.Length; t++)
            {
                TileLayoutTile tile = layout.tiles[t];
                if (tile?.passTypes == null) continue;
                int baseCol = t * TileColumns;
                for (int row = 0; row < TileRows; row++)
                for (int col = 0; col < TileColumns; col++)
                {
                    int srcIdx = row * TileColumns + col;
                    if (srcIdx >= tile.passTypes.Length) break;
                    int gridCol = baseCol + col;
                    if (gridCol >= layout.gridWidth) break;
                    // DNF [pass type]：0=可通行，非 0=阻挡
                    collision.PassGrid[row * layout.gridWidth + gridCol] = tile.passTypes[srcIdx] == 0 ? (byte)1 : (byte)0;
                }
            }

            int walkable = 0;
            foreach (byte b in collision.PassGrid) if (b == 1) walkable++;
            Log.Info($"[TownMapView] 城镇碰撞就绪：{collision.GridWidth}x{collision.GridHeight}，" +
                     $"cell=({collision.CellSize},{collision.CellSizeZ})，origin=({collision.OriginX},{collision.OriginZ})，可走 {walkable}/{collision.PassGrid.Length}");
        }

        /// <summary>碰撞调试叠图（同战斗 BuildCollisionDebugOverlay，数据源 TownCollisionComponent）</summary>
        private static void BuildCollisionDebugOverlay(TownMapViewComponent self, TownCollisionComponent collision)
        {
            if (!EnableCollisionDebugOverlay) return;
            if (collision?.PassGrid == null || collision.GridWidth <= 0 || collision.GridHeight <= 0
                || collision.CellSize <= FP.Zero || collision.CellSizeZ <= FP.Zero) return;

            int w = collision.GridWidth, h = collision.GridHeight;
            Texture2D texture = new(w, h, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32 walk = new(0, 255, 0, 128);
            Color32 block = new(255, 0, 0, 128);
            Color32[] buf = new Color32[w * h];
            for (int row = 0; row < h; row++)
            for (int col = 0; col < w; col++)
            {
                bool pass = collision.PassGrid[row * w + col] == 1;
                buf[(h - 1 - row) * w + col] = pass ? walk : block;   // Y 翻转：row 0=世界顶部
            }
            texture.SetPixels32(buf);
            texture.Apply(false, makeNoLongerReadable: true);

            FP worldW = (FP)w * collision.CellSize;
            FP worldH = (FP)h * collision.CellSizeZ;
            GameObject overlay = new("TownCollisionDebugOverlay");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            overlay.transform.SetParent(globalComponent.Unit, false);
            SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -999;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f), 1f / (float)collision.CellSize);
            overlay.transform.localPosition = new Vector3(
                (float)(collision.OriginX + worldW / 2), (float)(collision.OriginZ - worldH / 2), 0f);
            overlay.transform.localScale = new Vector3(1f, (float)(collision.CellSizeZ / collision.CellSize), 1f);

            self.CollisionDebugOverlay = overlay;
            self.CollisionDebugTexture = texture;
            Log.Info($"[TownMapView] 城镇碰撞叠图：{w}x{h} 格，世界 {worldW}x{worldH}，中心对齐地面");
        }

        // ---------- 碰撞数学（战斗 LSCollisionComponentSystem 同款拷贝——城镇客户端权威版）----------

        /// <summary>位置是否被阻挡（网格外一律阻挡）</summary>
        public static bool IsBlocked(this TownCollisionComponent self, TSVector position)
        {
            if (self.PassGrid == null || self.GridWidth <= 0 || self.GridHeight <= 0) return false;
            if (self.CellSize <= FP.Zero || self.CellSizeZ <= FP.Zero) return false;

            int col = (int)TSMath.Floor((position.x - self.OriginX) / self.CellSize);
            if (col < 0 || col >= self.GridWidth) return true;

            int row = (int)TSMath.Floor((self.OriginZ - position.z) / self.CellSizeZ);
            if (row < 0 || row >= self.GridHeight) return true;

            return self.PassGrid[row * self.GridWidth + col] == 0;
        }

        /// <summary>移动滑动：先试整 delta，被挡则逐轴回退（贴墙滑行；y 恒通过）</summary>
        public static void TryMove(this TownCollisionComponent self, TownPlayerComponent player, TSVector delta)
        {
            TSVector oldPos = player.Position;
            if (!self.IsBlocked(oldPos + delta))
            {
                player.Position = oldPos + delta;
                return;
            }

            TSVector moved = oldPos;
            TSVector stepX = new(delta.x, FP.Zero, FP.Zero);
            if (!self.IsBlocked(moved + stepX)) moved += stepX;
            TSVector stepZ = new(FP.Zero, FP.Zero, delta.z);
            if (!self.IsBlocked(moved + stepZ)) moved += stepZ;
            player.Position = moved;
        }
    }
}
