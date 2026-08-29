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

        // 鬼剑士主动技（第 1 批起，49+）
        public const int HardAttack = 49;            // hardattack.ani（鬼斩 18 帧 950ms）
        public const int HardAttackBlade1 = 50;      // hardattack1.ani（鬼斩刀光1 8 帧 550ms）
        public const int HardAttackBlade2 = 51;      // hardattack2.ani（鬼斩刀光2 8 帧 550ms）

        // 第 1 批剩余 4 技 + 跳跃（2026-08-29，52+）
        public const int SwordmanUpAttack = 52;      // up_attack.ani（上挑 9 帧 550ms，F2/F3 自带攻击盒）
        public const int UpperslashFx = 53;          // upperslash1.ani（上挑刀光 4 帧 200ms，.als 挂层）
        public const int SwordmanTripleSlash1 = 54;  // tripleslash1.ani（三段斩·段1 5 帧 580ms，无攻击盒→手动盒）
        public const int SwordmanTripleSlash2 = 55;  // tripleslash2.ani（段2）
        public const int SwordmanTripleSlash3 = 56;  // tripleslash3.ani（段3 终段击倒）
        public const int SwordmanTripleSlash4 = 57;  // tripleslash4.ani（5 连扩展预留，当前不用）
        public const int SwordmanTripleSlash5 = 58;  // tripleslash5.ani（同上）
        public const int TripleSlashFx1 = 59;        // slash1.ani（段1 挥砍弧光 5 帧 350ms，overlay 挂层）
        public const int TripleSlashFx2 = 60;        // slash2.ani
        public const int TripleSlashFx3 = 61;        // slash3.ani
        public const int TripleSlashFx4 = 62;        // slash4.ani
        public const int TripleSlashFx5 = 63;        // slash5.ani
        public const int TripleSlashMoveDust1 = 64;  // move1.ani（前冲扬尘 5 帧 350ms，overlay 挂层）
        public const int TripleSlashMoveDust2 = 65;  // move2.ani
        public const int SwordmanDashAttackMultiHit = 66; // dashattackmultihit.ani（连突刺 8 帧 500ms，F2-F6 自带攻击盒）
        public const int ThrustBeam = 67;            // dashattackmultihitsub.ani（激光剑气弹 6 帧 425ms，视图层自推帧）
        public const int SwordmanJumpAttack = 68;    // jumpattack.ani（银光落刃下落刺击 6 帧 300ms，F2 贯地盒）
        public const int AshenForkSubRing = 69;      // ashenforksub.ani（落地冲击波环 6 帧 330ms，区域视图主层）
        public const int AshenForkSubDust = 70;      // ashenforksubdust.ani（冲击波尘土 11 帧 537ms，区域视图背层）
        public const int JumpUp = 71;                // jump.ani 切片：起跳段 F0-F6（600ms）
        public const int JumpFall = 72;              // jump.ani 切片：下落段 F8-F13（520ms）

        // 第 2 批（2026-08-29，73+）
        public const int JumpAttackMultiSlash1 = 73; // jumpattackmultislash1.ani（空中链斩1 5帧370ms，无盒→手动盒）
        public const int JumpAttackMultiSlash2 = 74; // jumpattackmultislash2.ani（空中链斩2）
        public const int SwordmanHopSmashReady = 75; // hopsmashready.ani（崩山击蓄力 1帧400ms）
        public const int SwordmanHopSmash = 76;      // hopsmash.ani（前跃下砸 7帧900ms，F3-F5 自带盒）
        public const int HopSmashWaveFront = 77;     // HopSmashSubFront1.ani（冲击波主层 6帧480ms）
        public const int HopSmashWaveGlow = 78;      // HopSmashSubFront2.ani（冲击波辉光背层）
        public const int SwordmanVaneSlashTry = 79;  // vaneslashtry.ani（裂波上斩 5帧350ms，F2/F3 自带盒）
        public const int SwordmanVaneSlash = 80;     // vaneslash.ani（裂波旋转 7帧1490ms）
        public const int VaneSlashWheel = 81;        // VaneSlash.ani PO（波轮 20帧，悬停帧钳80ms→1410ms）
        public const int VaneSlashNormal = 82;       // VaneSlashNormal.ani PO（终结爆发 4帧280ms）
        public const int SwordmanGoreCross = 83;     // gorecross.ani（十字斩 29帧1330ms，无盒→手动盒）
        public const int GoreCrossFlash = 84;        // gorecross1.ani PO（十字闪光 4帧320ms）
        public const int GoreCrossCross = 85;        // gorecross2.ani PO（十字叠加层）
        public const int GoreCross3Cross = 86;       // gorecross3.ani PO（三联爆发 4帧320ms）
        public const int GoreCross3CrossFade = 87;   // gorecross4.ani PO（渐隐 4帧320ms）
        public const int SwordmanWeaponComboBlade1 = 88; // weaponcomboblade1.ani（里鬼太刀段1 700ms，自带盒）
        public const int SwordmanWeaponComboBlade2 = 89; // weaponcomboblade2.ani（段2 640ms）
        public const int SwordmanWeaponComboBlade3 = 90; // weaponcomboblade3.ani（段3 700ms）
        public const int SwordmanWeaponComboBlade4 = 91; // weaponcomboblade4.ani（段4 640ms）
        // 里鬼刀光特效（.als 别名 → AnimId）
        public const int KatananewFx11 = 92;         // katananew_1_1.ani（blade1/3 复用）
        public const int KatananewFx12 = 93;         // katananew_1_2.ani
        public const int KatananewFx1m1 = 94;        // katananew_1-1.ani
        public const int UraKatanaEff = 95;          // ura_katana_eff.ani（followParent 辉光）
        public const int KatananewFx21 = 96;         // katananew_2_1.ani（blade2）
        public const int KatananewFx22 = 97;         // katananew_2_2.ani
        public const int KatananewFx2m1 = 98;        // katananew_2-1.ani
        public const int KatananewFx2m2 = 99;        // katananew_2-2.ani
        public const int KatanaNew1Under = 100;      // katana_new1_under_effect.ani
        public const int KatanaNew1Upper = 101;      // katana_new1_upper_effect.ani
        public const int KatananewFx31 = 102;        // katananew_3_1.ani（blade4）
        public const int KatananewFx32 = 103;        // katananew_3_2.ani
        public const int KatananewFx3m1 = 104;       // katananew_3-1.ani
        public const int KatananewFx3m2 = 105;       // katananew_3-2.ani
        public const int KatanaNew2Under = 106;      // katana_new2_under_effect.ani
        public const int KatanaNew2Upper = 107;      // katana_new2_upper_effect.ani
    }
}
