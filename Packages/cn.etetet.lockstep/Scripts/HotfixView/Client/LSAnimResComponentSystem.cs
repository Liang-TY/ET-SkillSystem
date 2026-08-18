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
            self.AtlasOffsets ??= new(System.StringComparer.OrdinalIgnoreCase);
        }

        [EntitySystem]
        private static void Destroy(this LSAnimResComponent self)
        {
            self.Atlases.Clear();
            self.AtlasOffsets.Clear();
            // Texture2D 运行时创建的图集不 Destroy（场景级共享，泄露量恒定可接受）
        }

        public static async ETTask InitAsync(this LSAnimResComponent self)
        {
            Room room = self.Room();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();

            // 多图集：角色图集 + 投射物图集（key = 动画 json 里 image.path 的文件名，忽略大小写）
            await BuildAtlas(self, resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bantuamazones.img.bytes");
            await BuildAtlas(self, resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/NormalWave1.img.bytes");
            Log.Info($"[LSAnimRes] 图集构建完成：{self.Atlases.Count} 张（{string.Join(", ", self.Atlases.Keys)}）");
        }

        /// <summary>加载一张 img.bytes → 解析 → RectpackSharp 打包成运行时图集 → Sprite + 帧偏移注册</summary>
        private static async ETTask BuildAtlas(LSAnimResComponent self, ResourcesLoaderComponent resLoader, string assetPath)
        {
            // key 取文件名并去掉 .bytes 后缀（= json 里 image.path 的文件名形态）
            string atlasName = System.IO.Path.GetFileName(assetPath);
            if (atlasName.EndsWith(".bytes")) atlasName = atlasName[..^".bytes".Length];

            TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>(assetPath);
            NpkSprite[] npk = NpkImgParser.Parse(imgAsset.bytes);
            Log.Info($"[LSAnimRes] {atlasName}: 解析 {npk.Length} 帧");

            // 收集要打包的矩形（+1 padding 防渗色），Id = 数组下标（= json 的 image.index）
            // 注意：用数组下标 i 作 key，不用 npk[i].Index（引用帧的 Index 字段会被拷贝成被引用者的值，不正确）
            List<PackingRectangle> rectList = new();
            for (int i = 0; i < npk.Length; i++)
            {
                if (npk[i].ArgbData == null) continue;
                rectList.Add(new PackingRectangle(0, 0, (uint)(npk[i].Width + 1), (uint)(npk[i].Height + 1), i));
            }

            // 打包。2048=上限；bounds=实际用到的包围盒（按它建图集，不浪费内存）
            PackingRectangle[] rectArr = rectList.ToArray();
            RectanglePacker.Pack(rectArr, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
            int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

            // 建单张图集（RGBA32 必须匹配 Color32 内存布局），设 Point/Clamp
            Texture2D atlas = new(atlasW, atlasH, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;       // 像素图防糊
            atlas.wrapMode = TextureWrapMode.Clamp;     // 图集防 UV 溢出渗色
            NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

            // 零拷贝填充：ARGB int → RGBA 字节；Y 翻转（packer top-left ↔ Unity bottom-left）
            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    int argb = s.ArgbData[y * s.Width + x];
                    int dstY = atlasH - 1 - (int)r.Y - y;
                    buf[dstY * atlasW + ((int)r.X + x)] = new Color32(
                        (byte)((argb >> 16) & 0xFF),   // R
                        (byte)((argb >>  8) & 0xFF),   // G
                        (byte)((argb        ) & 0xFF), // B
                        (byte)((argb >> 24) & 0xFF));  // A
                }
            }
            atlas.Apply(false, makeNoLongerReadable: true);   // 上传 GPU 后释放 CPU 副本

            // Sprite（子区域，中心 pivot）+ 帧偏移注册
            Dictionary<int, Sprite> sprites = new();
            Dictionary<int, Vector2> offsets = new();
            // 校准帧 = 首个实体帧（rectArr 空时 default 不被使用——后续循环不执行）
            NpkSprite refSprite = rectArr.Length > 0 ? npk[rectArr[0].Id] : default;
            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                Rect rect = new(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);  // Y 翻转
                sprites[r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
                // 摆位修正（相对校准帧）：imagePos 锚定 DNF 帧画布，内容中心还要加 X+宽/2（y 翻转）——见 02-坐标系 §2.1
                offsets[r.Id] = new Vector2(
                    s.X + s.Width / 2f - (refSprite.X + refSprite.Width / 2f),
                    -(s.Y + s.Height / 2f) + refSprite.Y + refSprite.Height / 2f);
            }
            self.Atlases[atlasName] = sprites;
            self.AtlasOffsets[atlasName] = offsets;
            Log.Info($"[LSAnimRes] {atlasName}: 图集 {atlasW}x{atlasH}，{rectArr.Length} 精灵");
        }

        public static Sprite GetSprite(this LSAnimResComponent self, string atlasName, int imgIndex)
        {
            if (atlasName == null || atlasName.Length == 0) return null;   // 空路径帧（隐形占位）
            if (self.Atlases.TryGetValue(atlasName, out Dictionary<int, Sprite> sprites))
            {
                sprites.TryGetValue(imgIndex, out Sprite sprite);
                return sprite;
            }
            Log.Warning($"[LSAnimRes] 未注册图集 {atlasName}");
            return null;
        }

        public static Vector2 GetFrameOffset(this LSAnimResComponent self, string atlasName, int imgIndex)
        {
            if (self.AtlasOffsets.TryGetValue(atlasName, out Dictionary<int, Vector2> offsets))
            {
                offsets.TryGetValue(imgIndex, out Vector2 offset);
                return offset;
            }
            return Vector2.zero;
        }
    }
}
