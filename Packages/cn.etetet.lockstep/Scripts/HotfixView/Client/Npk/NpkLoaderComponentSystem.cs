using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// NPK 加载器系统：挂载所有 .npk.bytes → 提供文件名读取 IMG 字节的统一入口。
    /// 短期方案：NpkMountManager 内部有文件名→虚拟路径反查表，JSON/C# 用简单文件名即可。
    /// 长期方案见方案文档 §12。
    /// </summary>
    [EntitySystemOf(typeof(NpkLoaderComponent))]
    [FriendOf(typeof(NpkLoaderComponent))]
    public static partial class NpkLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NpkLoaderComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this NpkLoaderComponent self)
        {
            self.Manager?.Dispose();
            self.LoadedArchiveNames.Clear();
        }

        /// <summary>
        /// 从 Bundles/NPK/ 加载所有 .npk.bytes 并挂载。
        /// 在 InitAsync 开头调用。
        /// </summary>
        public static async ETTask LoadAllNpks(this NpkLoaderComponent self)
        {
            Room room = self.GetParent<Room>();
            if (room == null) return;
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null) return;

            string npkDir = "Packages/cn.etetet.lockstep/Bundles/NPK";

            string[] npkFiles = new string[]
            {
                "sprite_monster_bantu",
                "sprite_character_swordman_effect",
                "sprite_character_swordman_effect_bloodboom",
                "sprite_character_swordman_equipment_weapon_katana",
                "sprite_character_swordman_equipment_avatar_skin",
                "sprite_common_commoneffect_glow",
                "sprite_map_village_aganzo",
            };

            foreach (string npkName in npkFiles)
            {
                if (self.LoadedArchiveNames.Contains(npkName)) continue;

                string path = $"{npkDir}/{npkName}.npk.bytes";
                TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
                if (asset == null)
                {
                    Log.Warning($"[NpkLoader] 找不到 NPK: {path}");
                    continue;
                }

                self.Manager.Mount(npkName, asset.bytes);
                self.LoadedArchiveNames.Add(npkName);
            }

            Log.Info($"[NpkLoader] 挂载完成: {self.LoadedArchiveNames.Count} 个归档");
        }

        /// <summary>
        /// 从 NPK 提取 IMG 字节。双版本兼容：
        /// 新版 JSON path = 完整虚拟路径（sprite/character/...）→ Read() 直接命中
        /// 旧版 JSON path = 纯文件名（bantuamazones.img）→ ReadByFilename() 反查命中
        /// 返回 null = NPK 中找不到（调用方走 .img.bytes fallback）。
        /// </summary>
        public static byte[] TryReadImg(this NpkLoaderComponent self, string path)
        {
            if (self.Manager == null || self.Manager.Count == 0) return null;

            // 新版：path 含 / 的是完整虚拟路径，直接 Read
            if (path.Contains('/'))
            {
                byte[] result = self.Manager.Read(path);
                if (result != null) return result;
            }

            // 旧版/兜底：文件名反查
            return self.Manager.ReadByFilename(path);
        }
    }
}
