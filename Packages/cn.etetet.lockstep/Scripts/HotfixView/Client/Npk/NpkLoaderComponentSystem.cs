using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// NPK 加载器系统：挂载所有 .npk.bytes → 提供虚拟路径读取 IMG 字节的统一入口。
    /// </summary>
    [EntitySystemOf(typeof(NpkLoaderComponent))]
    [FriendOf(typeof(NpkLoaderComponent))]
    public static partial class NpkLoaderComponentSystem
    {
        /// <summary>
        /// atlas key（文件名去 .bytes）→ NPK 虚拟路径 映射表。
        /// 当前手工维护，后续由配置驱动（MapDefinition/MonsterAi → 动画 → IMG 路径链）。
        /// 未在此表的 IMG 走旧管线（.img.bytes fallback），保证迁移期间不炸。
        /// </summary>
        private static readonly (string key, string path)[] AtlasToVirtual = new (string, string)[]
        {
            ("bantuamazones.img",         "sprite/monster/bantu/bantuamazones.img"),
            ("NormalWave1.img",           "sprite/character/swordman/effect/NormalWave1.img"),
            ("bloodboom_boomback.img",    "sprite/character/swordman/effect/bloodboom/bloodboom_boomback.img"),
            ("bloodboom_boomfront.img",   "sprite/character/swordman/effect/bloodboom/bloodboom_boomfront.img"),
            ("bloodboom_casting.img",     "sprite/character/swordman/effect/bloodboom/bloodboom_casting.img"),
            ("bloodboom_casting_back.img","sprite/character/swordman/effect/bloodboom/bloodboom_casting_back.img"),
            ("rwi_bodyglow.img",          "sprite/character/swordman/effect/rwi_bodyglow.img"),
            ("rwi_creature.img",          "sprite/character/swordman/effect/rwi_creature.img"),
            ("rwi_speedline.img",         "sprite/character/swordman/effect/rwi_speedline.img"),
            ("rwi_wave.img",              "sprite/character/swordman/effect/rwi_wave.img"),
            ("blackdust.img",             "sprite/character/priest/effect/devilspincutter/blackdust.img"),
            ("circle.img",                "sprite/common/commoneffect/glow/circle.img"),
            ("explosionelectric02.img",   "sprite/character/priest/effect/atblessofangel/explosionelectric02.img"),
            ("lightningfairy12.img",      "sprite/common/commoneffect/glow/lightningfairy12.img"),
            ("sphereexplosionnormal01.img","sprite/common/commoneffect/glow/sphereexplosionnormal01.img"),
            ("twister00.img",             "sprite/common/commoneffect/glow/twister00.img"),
            ("icebreath1.img",            "sprite/monster/bantu/icebreath1.img"),
            ("icebreath2.img",            "sprite/monster/bantu/icebreath2.img"),
            ("sm_body0000.img",           "sprite/character/swordman/equipment/avatar/skin/sm_body0000.img"),
            // 以下 4 个在当前 NPK 中未找到，走 .img.bytes fallback
            // ("AT_Up.img",               "sprite/character/swordman/effect/AT_Up.img"),
            // ("katana_blade.img",        "sprite/character/swordman/equipment/weapon/katana/katana_blade.img"),
            // ("katana_handle.img",       "sprite/character/swordman/equipment/weapon/katana/katana_handle.img"),
            // ("releasewave3.img",        "sprite/character/swordman/effect/releasewave3.img"),
        };

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
        /// 在 InitAsync 开头调用（替代旧的逐个加载 .img.bytes 前置步骤）。
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
        /// 通过 atlas key 查找 NPK 虚拟路径并提取 IMG 字节。
        /// 返回 null = 不在映射表或 NPK 中找不到（调用方走 fallback）。
        /// </summary>
        public static byte[] TryReadImg(this NpkLoaderComponent self, string atlasKey)
        {
            if (self.Manager == null || self.Manager.Count == 0) return null;

            foreach (var (key, path) in AtlasToVirtual)
            {
                if (string.Equals(key, atlasKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    return self.Manager.Read(path);
                }
            }

            return null;
        }
    }
}
