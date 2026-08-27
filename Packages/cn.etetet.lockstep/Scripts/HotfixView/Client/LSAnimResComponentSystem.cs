using System.Collections.Generic;
using RectpackSharp;
using Unity.Collections;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSAnimResComponent))]
    public static partial class LSAnimResComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimResComponent self)
        {
            self.Atlases ??= new(System.StringComparer.OrdinalIgnoreCase);
            self.AtlasCenters ??= new(System.StringComparer.OrdinalIgnoreCase);
        }

        [EntitySystem]
        private static void Destroy(this LSAnimResComponent self)
        {
            self.Atlases.Clear();
            self.AtlasCenters.Clear();
        }

        public static async ETTask InitAsync(this LSAnimResComponent self)
        {
            Room room = self.Room();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();

            // NPK 挂载
            NpkLoaderComponent npkLoader = room.GetComponent<NpkLoaderComponent>();
            if (npkLoader == null)
            {
                npkLoader = room.AddComponent<NpkLoaderComponent>();
                await npkLoader.LoadAllNpks();
            }

            // 依赖收集：从 AnimConfigRegistry 沿配置链收集所有 IMG 引用（替代硬编码列表）
            HashSet<string> imgNames = ResourceDependencyCollector.CollectForDungeon();
            Log.Info($"[LSAnimRes] 依赖收集: {imgNames.Count} 个 IMG — {string.Join(", ", imgNames)}");

            string animResDir = "Packages/cn.etetet.lockstep/Bundles/AnimRes";

            foreach (string atlasKey in imgNames)
            {
                byte[] imgBytes = npkLoader.TryReadImg(atlasKey);

                if (imgBytes != null)
                {
                    BuildAtlasFromBytes(self, atlasKey, imgBytes);
                }
                else
                {
                    Log.Warning($"[LSAnimRes] NPK 未找到 {atlasKey}，尝试 .img.bytes fallback");
                    await BuildAtlas(self, resLoader, $"{animResDir}/{atlasKey}.bytes");
                }
            }

            // LINEARDODGE 加法混合材质
            Shader additiveShader = Shader.Find("ET/SpriteAdditive");
            if (additiveShader != null)
            {
                self.AdditiveMaterial = new Material(additiveShader);
                Log.Info("[LSAnimRes] 加法混合材质就绪");
            }
            else
            {
                Log.Warning("[LSAnimRes] 找不到 ET/SpriteAdditive shader");
            }

            Log.Info($"[LSAnimRes] 图集构建完成：{self.Atlases.Count} 张（{string.Join(", ", self.Atlases.Keys)}）");
        }

        /// <summary>
        /// 从原始 IMG 字节构建图集（NPK 管线：直接从 NpkArchive 提取的字节开始）。
        /// </summary>
        private static void BuildAtlasFromBytes(LSAnimResComponent self, string atlasName, byte[] imgBytes)
        {
            NpkSprite[] npk = NpkImgParser.Parse(imgBytes);
            Log.Info($"[LSAnimRes/NPK] {atlasName}: 解析 {npk.Length} 帧");

            List<PackingRectangle> rectList = new();
            for (int i = 0; i < npk.Length; i++)
            {
                if (npk[i].ArgbData == null) continue;
                rectList.Add(new PackingRectangle(0, 0, (uint)(npk[i].Width + 1), (uint)(npk[i].Height + 1), i));
            }

            PackingRectangle[] rectArr = rectList.ToArray();
            RectanglePacker.Pack(rectArr, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
            int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

            Texture2D atlas = new(atlasW, atlasH, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;
            atlas.wrapMode = TextureWrapMode.Clamp;
            NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = atlasH - 1 - (int)r.Y - y;
                    buf[dstY * atlasW + ((int)r.X + x)] = new Color32(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >>  8) & 0xFF),
                        (byte)((argb        ) & 0xFF),
                        (byte)((argb >> 24) & 0xFF));
                }
            }
            atlas.Apply(false, makeNoLongerReadable: true);

            Dictionary<int, Sprite> sprites = new();
            Dictionary<int, Vector2> centers = new();
            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                Rect rect = new(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);
                sprites[r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
                centers[r.Id] = new Vector2(s.X + s.Width / 2f, s.Y + s.Height / 2f);
            }
            self.Atlases[atlasName] = sprites;
            self.AtlasCenters[atlasName] = centers;
            Log.Info($"[LSAnimRes/NPK] {atlasName}: 图集 {atlasW}x{atlasH}，{rectArr.Length} 精灵");
        }

        /// <summary>加载一张 img.bytes（fallback 旧管线）</summary>
        private static async ETTask BuildAtlas(LSAnimResComponent self, ResourcesLoaderComponent resLoader, string assetPath)
        {
            string atlasName = System.IO.Path.GetFileName(assetPath);
            if (atlasName.EndsWith(".bytes")) atlasName = atlasName[..^".bytes".Length];

            TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>(assetPath);
            if (imgAsset == null)
            {
                Log.Warning($"[LSAnimRes] fallback 也找不到: {assetPath}");
                return;
            }
            NpkSprite[] npk = NpkImgParser.Parse(imgAsset.bytes);
            Log.Info($"[LSAnimRes] {atlasName}: 解析 {npk.Length} 帧");

            List<PackingRectangle> rectList = new();
            for (int i = 0; i < npk.Length; i++)
            {
                if (npk[i].ArgbData == null) continue;
                rectList.Add(new PackingRectangle(0, 0, (uint)(npk[i].Width + 1), (uint)(npk[i].Height + 1), i));
            }

            PackingRectangle[] rectArr = rectList.ToArray();
            RectanglePacker.Pack(rectArr, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
            int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

            Texture2D atlas = new(atlasW, atlasH, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;
            atlas.wrapMode = TextureWrapMode.Clamp;
            NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = atlasH - 1 - (int)r.Y - y;
                    buf[dstY * atlasW + ((int)r.X + x)] = new Color32(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >>  8) & 0xFF),
                        (byte)((argb        ) & 0xFF),
                        (byte)((argb >> 24) & 0xFF));
                }
            }
            atlas.Apply(false, makeNoLongerReadable: true);

            Dictionary<int, Sprite> sprites = new();
            Dictionary<int, Vector2> centers = new();
            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                Rect rect = new(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);
                sprites[r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
                centers[r.Id] = new Vector2(s.X + s.Width / 2f, s.Y + s.Height / 2f);
            }
            self.Atlases[atlasName] = sprites;
            self.AtlasCenters[atlasName] = centers;
            Log.Info($"[LSAnimRes] {atlasName}: 图集 {atlasW}x{atlasH}，{rectArr.Length} 精灵");
        }

        public static Sprite GetSprite(this LSAnimResComponent self, string atlasName, int imgIndex)
        {
            if (atlasName == null || atlasName.Length == 0) return null;
            // 规范化：新版 JSON path 是完整虚拟路径（sprite/.../xxx.img），取文件名查图集
            if (atlasName.Contains('/'))
                atlasName = System.IO.Path.GetFileName(atlasName);
            if (self.Atlases.TryGetValue(atlasName, out Dictionary<int, Sprite> sprites))
            {
                if (!sprites.TryGetValue(imgIndex, out Sprite sprite))
                    Log.Warning($"[LSAnimRes] {atlasName}[{imgIndex}] 越界（图集 {sprites.Count} 帧）");
                return sprite;
            }
            Log.Warning($"[LSAnimRes] 未注册图集 {atlasName}");
            return null;
        }

        public static Vector2 GetFrameCenter(this LSAnimResComponent self, string atlasName, int imgIndex)
        {
            if (atlasName == null || atlasName.Length == 0) return Vector2.zero;
            if (atlasName.Contains('/'))
                atlasName = System.IO.Path.GetFileName(atlasName);
            if (self.AtlasCenters.TryGetValue(atlasName, out Dictionary<int, Vector2> centers))
            {
                centers.TryGetValue(imgIndex, out Vector2 center);
                return center;
            }
            return Vector2.zero;
        }
    }
}
