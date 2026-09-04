using System.Collections.Generic;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// Editor NPK 贴图仓库：复用运行时同款 NpkMountManager + NpkImgParser（ET.NpkParser），
    /// 挂载源从 YooAsset 换成磁盘枚举 ImagePacks2/*.npk.bytes（EditorSimMode 等价物，08 探针实证直读可行）。
    ///
    /// 与运行时 LSAnimResComponentSystem.BuildAtlasFromBytes 的换算逐公式一致（02 §8 一致性红线）：
    /// - Sprite pivot 一律 (0.5, 0.5)——运行时不编码信息到 pivot，摆位用 localPosition；
    /// - 帧中心 center = (X + Width/2, Y + Height/2)（IMG 内帧几何中心，像素）；
    /// - 摆位 = (round(imagePos + center)/100, -round(imagePos + center.y)/100)——
    ///   见 LSSpriteAnimViewComponentSystem §2.1（含奇数宽 .5px snap 整像素）。
    /// 差异仅一处：Editor 不打图集，每帧独立 Texture（rect=整帧），渲染结果等价。
    /// </summary>
    internal static class SkillNpkSpriteStore
    {
        private const string ImagePacksRoot = "Packages/cn.etetet.lockstep/Bundles/ImagePacks2";

        private static NpkMountManager manager;
        private static bool mountAttempted;

        private sealed class SpriteEntry
        {
            public Sprite Sprite;
            /// <summary>帧中心（像素，IMG 坐标系，原点左上）——预览摆位用。</summary>
            public Vector2 Center;
        }

        // path#index → sprite+center
        private static readonly Dictionary<string, SpriteEntry> cache = new();

        public static bool TryGetEntry(string imagePath, int frameIndex, out Sprite sprite, out Vector2 center, out string error)
        {
            sprite = null;
            center = Vector2.zero;
            error = null;
            if (string.IsNullOrEmpty(imagePath))
            {
                error = "帧无 image.path";
                return false;
            }
            EnsureMounted();

            string key = $"{imagePath}#{frameIndex}";
            if (cache.TryGetValue(key, out SpriteEntry cached))
            {
                sprite = cached.Sprite;
                center = cached.Center;
                return true;
            }

            byte[] imgBytes = manager != null
                ? (imagePath.Contains('/')
                    ? manager.Read(imagePath) ?? manager.ReadByFilename(imagePath)
                    : manager.ReadByFilename(imagePath))
                : null;

            // .img.bytes 独立文件 fallback（散装资源；角色 sprite 走 NPK）
            if (imgBytes == null)
            {
                string standalone = System.IO.Path.Combine(
                    ImagePacksRoot, System.IO.Path.GetFileName(imagePath) + ".bytes");
                if (System.IO.File.Exists(standalone)) imgBytes = System.IO.File.ReadAllBytes(standalone);
            }
            if (imgBytes == null)
            {
                error = $"NPK/IMG 中找不到: {imagePath}";
                return false;
            }

            NpkSprite[] sprites = NpkImgParser.Parse(imgBytes);
            if (frameIndex < 0 || frameIndex >= sprites.Length)
            {
                error = $"帧序越界: {imagePath}[{frameIndex}]（共 {sprites.Length} 帧）";
                return false;
            }

            NpkSprite s = sprites[frameIndex];
            SpriteEntry entry = CreateSpriteEntry(s, out error);
            if (entry == null) return false;
            cache[key] = entry;
            sprite = entry.Sprite;
            center = entry.Center;
            return true;
        }

        /// <summary>虚拟路径所在 NPK 归档名（诊断面板展示）；未挂载/找不到返回 null。</summary>
        public static string GetArchiveName(string imagePath)
        {
            EnsureMounted();
            return manager?.GetArchiveName(imagePath) ?? manager?.GetArchiveName(System.IO.Path.GetFileName(imagePath));
        }

        public static void EnsureMounted()
        {
            if (mountAttempted) return;
            mountAttempted = true;
            manager = new NpkMountManager();
            string root = System.IO.Path.GetFullPath(ImagePacksRoot);
            if (!System.IO.Directory.Exists(root))
            {
                UnityEngine.Debug.LogWarning($"[SkillPreview] ImagePacks2 不存在: {root}");
                return;
            }
            foreach (string file in System.IO.Directory.GetFiles(root, "*.npk.bytes"))
            {
                // 归档名与运行时一致 = 文件名去 .bytes 再去 .npk（NpkLoader 地址 = AddressByFileName）
                string archiveName = System.IO.Path.GetFileNameWithoutExtension(
                    System.IO.Path.GetFileNameWithoutExtension(file));
                try
                {
                    manager.Mount(archiveName, System.IO.File.ReadAllBytes(file));
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[SkillPreview] NPK 挂载失败 {archiveName}: {e.Message}");
                }
            }
        }

        public static void UnmountAll()
        {
            manager?.Dispose();
            manager = null;
            mountAttempted = false;
            ClearCache();
        }

        public static void ClearCache()
        {
            foreach (SpriteEntry entry in cache.Values)
            {
                if (entry.Sprite != null)
                {
                    Object.DestroyImmediate(entry.Sprite.texture);
                    Object.DestroyImmediate(entry.Sprite);
                }
            }
            cache.Clear();
        }

        /// <summary>ARGB → RGBA32 + 独立 Texture；Y 翻转写入（IMG 原点左上 → Unity 原点左下）。
        /// 遍历用 Width×Height（实际像素尺寸，ArgbData 按此存储）——与运行时 BuildAtlasFromBytes 一致。</summary>
        private static SpriteEntry CreateSpriteEntry(in NpkSprite s, out string error)
        {
            error = null;
            try
            {
                Texture2D texture = new(s.Width, s.Height, TextureFormat.RGBA32, false, true)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                Color32[] pixels = new Color32[s.Width * s.Height];
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    int argb = s.ArgbData[y * s.Width + x];
                    // 与运行时 BuildAtlasFromBytes 同一换算：小端 ARGB → R,G,B,A
                    pixels[(s.Height - 1 - y) * s.Width + x] = new Color32(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF),
                        (byte)((argb >> 24) & 0xFF));
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);

                Rect rect = new(0, 0, s.Width, s.Height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                return new SpriteEntry
                {
                    Sprite = sprite,
                    // 运行时同款：帧几何中心（IMG 像素坐标，原点左上）
                    Center = new Vector2(s.X + s.Width / 2f, s.Y + s.Height / 2f),
                };
            }
            catch (System.Exception e)
            {
                error = $"Sprite 构建失败: {e.Message}";
                return null;
            }
        }
    }
}
