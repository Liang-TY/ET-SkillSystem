using System.Collections.Generic;
using System.IO;
using RectpackSharp;
using Unity.Collections;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 作用域管理系统：按场景类型加载/卸载 IMG 图集。
    /// 共享资源防误卸：卸载作用域时只释放仅属于该作用域的 IMG。
    /// </summary>
    [EntitySystemOf(typeof(ResourceScopeComponent))]
    [FriendOf(typeof(ResourceScopeComponent))]
    [FriendOf(typeof(LSAnimResComponent))]
    public static partial class ResourceScopeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourceScopeComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ResourceScopeComponent self)
        {
            self.UnloadAllScopes();
        }

        /// <summary>
        /// 加载作用域：收集 IMG 路径 → 逐个从 NPK 提取 → 打图集 → 注册到 LSAnimResComponent。
        /// 已加载的 IMG 只追加作用域归属，不重复加载。
        /// </summary>
        public static async ETTask LoadScope(
            this ResourceScopeComponent self,
            string scopeType, string scopeId,
            HashSet<string> imgNames,
            LSAnimResComponent animRes)
        {
            if (animRes == null) return;

            string scopeKey = $"{scopeType}:{scopeId}";
            self.ScopePaths.TryAdd(scopeKey, new HashSet<string>());

            NpkLoaderComponent npkLoader = self.GetParent<Room>().GetComponent<NpkLoaderComponent>();
            ResourcesLoaderComponent resLoader = self.GetParent<Room>().GetComponent<ResourcesLoaderComponent>();
            string animResDir = "Packages/cn.etetet.lockstep/Bundles/AnimRes";

            foreach (string atlasKey in imgNames)
            {
                self.ScopePaths[scopeKey].Add(atlasKey);

                // 已加载 → 只追加作用域归属
                if (self.LoadedImgs.TryGetValue(atlasKey, out LoadedImgInfo existing))
                {
                    existing.Scopes.Add(scopeKey);
                    continue;
                }

                // 加载
                byte[] imgBytes = npkLoader?.TryReadImg(atlasKey);
                if (imgBytes == null)
                {
                    // fallback .img.bytes
                    TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>($"{animResDir}/{atlasKey}.bytes");
                    if (asset != null) imgBytes = asset.bytes;
                }

                if (imgBytes == null)
                {
                    Log.Warning($"[ResourceScope] {scopeKey}: 找不到 {atlasKey}");
                    continue;
                }

                // 打图集
                var (sprites, centers, texture) = BuildAtlas(imgBytes, atlasKey);
                animRes.Atlases[atlasKey] = sprites;
                animRes.AtlasCenters[atlasKey] = centers;

                self.LoadedImgs[atlasKey] = new LoadedImgInfo
                {
                    AtlasKey = atlasKey,
                    Scopes = { scopeKey },
                    Texture = texture,
                    Sprites = sprites,
                    Centers = centers,
                };
            }

            Log.Info($"[ResourceScope] {scopeKey} 加载完成: {self.ScopePaths[scopeKey].Count} 个 IMG");
        }

        /// <summary>
        /// 卸载作用域：释放仅属于该作用域的图集（共享的不释放）。
        /// </summary>
        public static void UnloadScope(this ResourceScopeComponent self, string scopeType, string scopeId)
        {
            string scopeKey = $"{scopeType}:{scopeId}";
            if (!self.ScopePaths.TryGetValue(scopeKey, out HashSet<string> paths)) return;

            LSAnimResComponent animRes = self.GetParent<Room>().GetComponent<LSAnimResComponent>();

            foreach (string atlasKey in paths)
            {
                if (!self.LoadedImgs.TryGetValue(atlasKey, out LoadedImgInfo info)) continue;

                info.Scopes.Remove(scopeKey);

                if (info.Scopes.Count == 0)
                {
                    // 没有其他作用域引用了 → 释放
                    animRes?.Atlases.Remove(atlasKey);
                    animRes?.AtlasCenters.Remove(atlasKey);

                    if (info.Texture != null)
                        UnityEngine.Object.Destroy(info.Texture);

                    self.LoadedImgs.Remove(atlasKey);
                }
            }

            self.ScopePaths.Remove(scopeKey);
            Log.Info($"[ResourceScope] {scopeKey} 卸载完成");
        }

        public static void UnloadAllScopes(this ResourceScopeComponent self)
        {
            var keys = new List<string>(self.ScopePaths.Keys);
            foreach (string key in keys)
            {
                string[] parts = key.Split(':');
                if (parts.Length == 2)
                    self.UnloadScope(parts[0], parts[1]);
            }
        }

        /// <summary>打图集（复用 LSAnimResComponent 的图集构建逻辑）</summary>
        private static (Dictionary<int, Sprite>, Dictionary<int, Vector2>, Texture2D) BuildAtlas(byte[] imgBytes, string atlasName)
        {
            NpkSprite[] npk = NpkImgParser.Parse(imgBytes);

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

            Log.Info($"[ResourceScope] {atlasName}: 图集 {atlasW}x{atlasH}，{rectArr.Length} 精灵");
            return (sprites, centers, atlas);
        }
    }
}
