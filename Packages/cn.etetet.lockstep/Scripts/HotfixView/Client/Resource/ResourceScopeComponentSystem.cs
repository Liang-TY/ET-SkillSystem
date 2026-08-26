using System.Collections.Generic;
using System.IO;
using RectpackSharp;
using Unity.Collections;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 作用域管理系统：按配置驱动加载/卸载 IMG 图集（方案文档 §14）。
    /// 读 resource_scope_rules.json → 执行源类型 → 收集 IMG → NPK 提取 → 打图集。
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
        /// 加载作用域：读配置 → 执行源类型 → 收集 IMG → 从 NPK 提取 → 打图集。
        /// 已加载的 IMG 只追加作用域归属，不重复加载。
        /// scope_ref 递归加载依赖作用域。
        /// </summary>
        public static async ETTask LoadScope(
            this ResourceScopeComponent self,
            string scopeType, string scopeId,
            LSAnimResComponent animRes)
        {
            if (animRes == null) return;

            string scopeKey = $"{scopeType}:{scopeId}";
            self.ScopePaths.TryAdd(scopeKey, new HashSet<string>());

            Room room = self.GetParent<Room>();
            NpkLoaderComponent npkLoader = room.GetComponent<NpkLoaderComponent>();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            string animResDir = "Packages/cn.etetet.lockstep/Bundles/AnimRes";

            // 收集 IMG 名字（执行所有源类型）
            var ctx = new ResourceSourceTypes.ScopeContext { ScopeType = scopeType, ScopeId = scopeId };
            HashSet<string> imgNames = CollectFromConfig(scopeType, scopeId, ctx, room);

            Log.Info($"[ResourceScope] {scopeKey} 收集: {imgNames.Count} 个 IMG");

            foreach (string atlasKey in imgNames)
            {
                self.ScopePaths[scopeKey].Add(atlasKey);
                self.ImgScopes.TryAdd(atlasKey, new HashSet<string>());
                self.ImgScopes[atlasKey].Add(scopeKey);

                // 已在 LSAnimResComponent 中 → 跳过加载
                if (animRes.Atlases.ContainsKey(atlasKey)) continue;

                // 加载：NPK 优先 + .img.bytes fallback
                byte[] imgBytes = npkLoader?.TryReadImg(atlasKey);
                if (imgBytes == null)
                {
                    TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>($"{animResDir}/{atlasKey}.bytes");
                    if (asset != null) imgBytes = asset.bytes;
                }

                if (imgBytes == null)
                {
                    Log.Warning($"[ResourceScope] {scopeKey}: 找不到 {atlasKey}");
                    continue;
                }

                BuildAtlasInternal(animRes, atlasKey, imgBytes);
            }

            Log.Info($"[ResourceScope] {scopeKey} 加载完成");
        }

        /// <summary>读配置 → 执行源类型 → 返回 IMG 集合</summary>
        private static HashSet<string> CollectFromConfig(string scopeType, string scopeId,
            ResourceSourceTypes.ScopeContext ctx, Room room)
        {
            var result = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // 读配置文件（当前硬编码配置，后续改为 JSON 加载）
            // TODO: 从 resource_scope_rules.json 读取
            var rules = GetDefaultRules();

            if (!rules.TryGetValue(scopeType, out var sources))
            {
                Log.Warning($"[ResourceScope] 未知作用域类型: {scopeType}");
                return result;
            }

            foreach (var source in sources)
            {
                if (source.type == "scope_ref")
                {
                    // 递归收集依赖作用域
                    string refScope = source.scope ?? "character";
                    if (!rules.TryGetValue(refScope, out var refSources)) continue;
                    var refCtx = new ResourceSourceTypes.ScopeContext { ScopeType = refScope, ScopeId = scopeId };
                    var refResult = CollectFromConfig(refScope, scopeId, refCtx, room);
                    result.UnionWith(refResult);
                }
                else
                {
                    object param = source.list ?? (object)(source.ids ?? null);
                    var collected = ResourceSourceTypes.Collect(source.type, param, ctx, room);
                    result.UnionWith(collected);
                }
            }

            // 应用 exclude（override 场景）
            if (source_exclude.TryGetValue(scopeType, out var excludes))
                foreach (string ex in excludes) result.Remove(ex);

            return result;
        }

        // ---- 默认配置（后续改为 JSON 文件加载）----

        private struct SourceRule
        {
            public string type;
            public List<string> list;
            public List<int> ids;
            public string scope;
        }

        private static Dictionary<string, List<SourceRule>> GetDefaultRules()
        {
            var rules = new Dictionary<string, List<SourceRule>>();

            rules["character"] = new List<SourceRule>
            {
                new() { type = "character_body" },
                new() { type = "character_weapon" },
                new() { type = "character_skills" },
            };

            rules["town"] = new List<SourceRule>
            {
                new() { type = "character_body" },
                new() { type = "character_weapon" },
                new() { type = "anim_ids", ids = new List<int> { 10, 11 } },
            };

            rules["dungeon"] = new List<SourceRule>
            {
                new() { type = "map_monsters" },
                new() { type = "map_tiles" },
                new() { type = "scope_ref", scope = "character" },
            };

            rules["event"] = new List<SourceRule>
            {
                new() { type = "event_resources" },
            };

            return rules;
        }

        private static readonly Dictionary<string, List<string>> source_exclude = new();

        /// <summary>卸载作用域：释放仅属于该作用域的图集。</summary>
        public static void UnloadScope(this ResourceScopeComponent self, string scopeType, string scopeId)
        {
            string scopeKey = $"{scopeType}:{scopeId}";
            if (!self.ScopePaths.TryGetValue(scopeKey, out HashSet<string> paths)) return;

            LSAnimResComponent animRes = self.GetParent<Room>().GetComponent<LSAnimResComponent>();

            foreach (string atlasKey in paths)
            {
                if (!self.ImgScopes.TryGetValue(atlasKey, out HashSet<string> scopes)) continue;

                scopes.Remove(scopeKey);

                if (scopes.Count == 0)
                {
                    animRes?.Atlases.Remove(atlasKey);
                    animRes?.AtlasCenters.Remove(atlasKey);
                    self.ImgScopes.Remove(atlasKey);
                    self.LoadedAtlasKeys.Remove(atlasKey);
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

        /// <summary>打图集并注册到 LSAnimResComponent</summary>
        private static void BuildAtlasInternal(LSAnimResComponent animRes, string atlasName, byte[] imgBytes)
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

            animRes.Atlases[atlasName] = sprites;
            animRes.AtlasCenters[atlasName] = centers;
            Log.Info($"[ResourceScope] {atlasName}: 图集 {atlasW}x{atlasH}，{rectArr.Length} 精灵");
        }
    }
}
