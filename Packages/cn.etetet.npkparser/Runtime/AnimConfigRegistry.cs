using System.Collections.Generic;

namespace ET
{
    public static class AnimConfigRegistry
    {
        [StaticField]
        private static readonly Dictionary<int, AnimClipData> configs = new();

        [StaticField]
        private static readonly Dictionary<int, AnimOverlayConfig> overlayConfigs = new();

        public static void Register(int animId, AnimClipData data)
        {
            configs[animId] = data;
        }

        public static AnimClipData Get(int animId)
        {
            configs.TryGetValue(animId, out AnimClipData data);
            return data;
        }

        /// <summary>注册 .als 特效叠加配置（animId = 挂接的父动画；entry.effectAnimId 需先解析好）</summary>
        public static void RegisterOverlay(int animId, AnimOverlayConfig config)
        {
            overlayConfigs[animId] = config;
        }

        /// <summary>查父动画的叠加配置（无 = null）</summary>
        public static AnimOverlayConfig GetOverlay(int animId)
        {
            overlayConfigs.TryGetValue(animId, out AnimOverlayConfig config);
            return config;
        }
    }

    public static class AnimId
    {
        public const int None = 0;
        public const int Idle = 1;
        public const int Walk = 2;
        public const int Attack1 = 3;   // 普攻第一段（暂用班图膝踢 kneekick.json，判定帧 1-3 有攻击盒）
        public const int Hurt = 4;      // 受击僵直（damage.json；末帧长 delay 停帧，靠硬直计时切走）
        public const int NormalWave = 5; // 地裂波动剑投射物（normalwave.json + NormalWave1.img，视图层自推帧）
        public const int FireCircle = 6;  // 火圈持续燃烧（firecircle.json + AT_Up.img，循环）
        public const int FireCircleEnd = 7; // 火圈熄灭收尾（firecircleend.json，不循环）

        // 鬼剑士（玩家专用段）
        public const int SwordmanIdle = 10;      // swordman_stay.json
        public const int SwordmanWalk = 11;      // swordman_move.json
        public const int SwordmanAttack1 = 12;   // swordman_attack1.json
        public const int SwordmanAttack2 = 13;   // swordman_attack2.json
        public const int SwordmanAttack3 = 14;   // swordman_attack3.json（有 2 个 attackBox）
        public const int SwordmanHurt = 15;      // swordman_damage.json
        public const int SwordmanBloodboom = 16; // swordman_bloodboom.json（施法动画，叠加 bloodboom_cast_overlay）

        // 浴血之怒特效段（bloodboom_cast_overlay.json 的别名 → AnimId 映射）
        public const int BloodboomCastingBack = 17;  // bloodboom_casting_back.json（施法蓄力背面层）
        public const int BloodboomCasting = 18;      // bloodboom_casting.json（施法蓄力正面层）
        public const int BloodboomBoomFront = 19;    // bloodboom_boomfront.json（爆炸正面，区域视图主层）
        public const int BloodboomBoomBack = 20;     // bloodboom_boomback.json（爆炸背面，区域视图背层）

        // 波动爆发段（rw_dash_overlay.json / rw_burst_overlay.json 的别名 → AnimId 映射）
        public const int SwordmanReleaseWaveDash = 21;  // swordman_releasewavedash.json（角色冲刺 3 帧 230ms）
        public const int ReleaseWaveCreature = 22;      // rw_creature.json（冲刺幻影）
        public const int ReleaseWaveCreature01 = 23;    // rw_creature_01.json（冲刺幻影拖尾）
        public const int ReleaseWaveBodyGlow = 24;      // rw_bodyglow.json（体光）
        public const int ReleaseWaveSpeedLine = 25;     // rw_speedline.json（速度线 ×4）
        public const int ReleaseWaveSpeedLine01 = 26;
        public const int ReleaseWaveSpeedLine01_01 = 27;
        public const int ReleaseWaveSpeedLine01_02 = 28;
        public const int ReleaseWaveCenter = 29;        // rw_center.json（中心电光，sphere 占位）
        public const int ReleaseWaveCenter01 = 30;
        public const int ReleaseWaveElec02 = 31;        // rw_elec02.json（爆发闪电信光 ×2）
        public const int ReleaseWaveElec02_01 = 32;
        public const int ReleaseWaveBackwind = 33;      // rw_backwind.json（蓄气主层，twister 占位+染蓝）
        public const int ReleaseWaveCastLightning = 34; // rw_castlightning.json（蓄气闪电，lightningfairy12 占位）
        public const int ReleaseWaveCircle = 35;        // rw_circle.json（地面光环，渐隐）
        public const int ReleaseWaveSmoke = 36;         // rw_smoke.json（黑烟）
        public const int ReleaseWaveGust = 37;          // rw_gust.json（蓄气波，rwi_wave）
        public const int ReleaseWaveBurst1 = 38;        // rw_burst1.json（爆炸主层 5 帧 490ms）
        public const int ReleaseWaveBurst2 = 39;        // rw_burst2.json（爆炸子层：冲击环）
        public const int ReleaseWaveBurst3 = 40;        // rw_burst3.json（爆炸子层：蓄风）
        public const int ReleaseWaveBurst4 = 41;        // rw_burst4.json（爆炸子层：终结爆炸）
    }
}
