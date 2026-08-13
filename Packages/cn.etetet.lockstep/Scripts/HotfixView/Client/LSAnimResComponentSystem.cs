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
            self.Sprites = new Dictionary<int, Sprite>();
            self.Atlas = null;
        }

        [EntitySystem]
        private static void Destroy(this LSAnimResComponent self)
        {
            self.Sprites.Clear();
            if (self.Atlas != null)
            {
                UnityEngine.Object.Destroy(self.Atlas);
                self.Atlas = null;
            }
        }

        public static async ETTask InitAsync(this LSAnimResComponent self)
        {
            Room room = self.Room();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();

            // 1. 加载并解析 IMG
            TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>("Packages/cn.etetet.lockstep/Bundles/AnimRes/bantuamazones.img.bytes");
            Log.Info($"[LSAnimRes] IMG loaded, size: {imgAsset.bytes.Length} bytes");
            NpkSprite[] npk = NpkImgParser.Parse(imgAsset.bytes);
            Log.Info($"[LSAnimRes] Parsed {npk.Length} sprites from IMG");

            // 2. 收集要打包的矩形（+1 padding 防渗色），Id = 数组下标（= JSON 的 image.index）
            //    注意：用数组下标 i 作 key，不用 npk[i].Index（引用帧的 Index 字段会被拷贝成被引用者的值，不正确）
            List<PackingRectangle> rectList = new();
            for (int i = 0; i < npk.Length; i++)
            {
                if (npk[i].ArgbData == null) continue;
                rectList.Add(new PackingRectangle(0, 0, (uint)(npk[i].Width + 1), (uint)(npk[i].Height + 1), i));
            }

            // 3. RectpackSharp 打包。2048=上限；bounds=实际用到的包围盒（按它建图集，不浪费内存）
            PackingRectangle[] rectArr = rectList.ToArray();
            RectanglePacker.Pack(rectArr, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
            int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

            // 4. 建单张图集（RGBA32 必须匹配 Color32 内存布局），设 Point/Clamp
            Texture2D atlas = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;       // 像素图防糊
            atlas.wrapMode = TextureWrapMode.Clamp;     // 图集防 UV 溢出渗色
            NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

            // 5. 零拷贝填充：ARGB int → RGBA 字节；Y 翻转（packer top-left ↔ Unity bottom-left）
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
                        (byte)( argb        & 0xFF),   // B
                        (byte)((argb >> 24) & 0xFF));  // A
                }
            }
            atlas.Apply(false, makeNoLongerReadable: true);   // 上传 GPU 后释放 CPU 副本
            self.Atlas = atlas;
            Log.Info($"[LSAnimRes] Atlas built: {atlasW}x{atlasH}, {rectArr.Length} sprites");

            // 6. 每帧 Sprite.Create 为子区域，存进字典（key = image.index）
            foreach (PackingRectangle r in rectArr)
            {
                NpkSprite s = npk[r.Id];
                Rect rect = new Rect(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);  // Y 翻转
                self.Sprites[r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
            }

            // clip 注册已挪到逻辑层 LSAnimClipRegistrar（在 room.Init 之前），视图层只管图集
        }

        public static Sprite GetSprite(this LSAnimResComponent self, int imgIndex)
        {
            self.Sprites.TryGetValue(imgIndex, out Sprite sprite);
            return sprite;
        }
    }
}
