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

        /// <summary>遍历所有已注册的 AnimClipData（供依赖收集器使用）</summary>
        public static IEnumerable<(int animId, AnimClipData data)> GetAll()
        {
            foreach (var kv in configs)
                yield return (kv.Key, kv.Value);
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


        // 班图女战士（BantuAmazones）怪物段（BantuAmazones.img 54 帧已在库）
        public const int MonsterLowKick = 42;       // monster_lowkick.json（下段踢 7 帧，判定帧 3-5）
        public const int MonsterHighKick = 43;      // monster_highkick.json（高踢 6 帧，判定帧 2-3）
        public const int MonsterIceBreath = 44;     // monster_icebreath.json（冰息施法 8 帧，帧 3 发弹）
        public const int MonsterDown = 45;          // monster_down.json（击倒落地 3 帧 900ms）
        public const int MonsterOverturn = 46;      // 预留（起身动画依赖 IMAGE ROTATE，未注册）
        public const int IceBreathBullet1 = 47;     // icebreath_bullet1.json（冰息弹主层 6 帧 150ms）
        public const int IceBreathBullet2 = 48;     // icebreath_bullet2.json（冰息弹第二层视觉）
    }
}
