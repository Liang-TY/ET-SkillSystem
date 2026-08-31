using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 动画 clip 注册：加载 .ani.bytes JSON → AnimConfigRegistry.Register。
    /// YooAsset 地址 = AnimRes 下的相对路径（去扩展名），如 character/swordman/animation/stay.ani
    /// </summary>
    public static partial class LSAnimClipRegistrar
    {
        // AnimRes CollectPath 下的相对路径就是 YooAsset 地址（AddressByFilePath 规则）
        private const string Res = "";
        private const string A = "character/swordman/animation";
        private const string E = "character/swordman/effect/animation/bloodboom";
        private const string P = "passiveobject/character/swordman/animation";
        private const string M = "monster/event/bluemarble/bantuamazones/baanimation";

        public static async ETTask RegisterAll(Scene root)
        {
            ResourcesLoaderComponent resLoader = root.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSAnimClip] ResourcesLoaderComponent 不存在");
                return;
            }

            // 怪物动画（bantuamazones）
            await RegisterOne(resLoader, $"{M}/stay.ani", AnimId.Idle);
            await RegisterOne(resLoader, $"{M}/move.ani", AnimId.Walk);
            await RegisterOne(resLoader, $"{M}/kneekick.ani", AnimId.Attack1);
            await RegisterOne(resLoader, $"{M}/damage.ani", AnimId.Hurt);
            await RegisterOne(resLoader, $"{M}/lowkick.ani", AnimId.MonsterLowKick);
            await RegisterOne(resLoader, $"{M}/highkick.ani", AnimId.MonsterHighKick);
            await RegisterOne(resLoader, $"{M}/icebreath.ani", AnimId.MonsterIceBreath);
            await RegisterOne(resLoader, $"{M}/down.ani", AnimId.MonsterDown);

            // 鬼剑士动画
            await RegisterOne(resLoader, $"{A}/stay.ani", AnimId.SwordmanIdle);
            await RegisterOne(resLoader, $"{A}/move.ani", AnimId.SwordmanWalk);
            await RegisterOne(resLoader, $"{A}/attack1.ani", AnimId.SwordmanAttack1);
            await RegisterOne(resLoader, $"{A}/attack2.ani", AnimId.SwordmanAttack2);
            await RegisterOne(resLoader, $"{A}/attack3.ani", AnimId.SwordmanAttack3);
            await RegisterOne(resLoader, $"{A}/damage1.ani", AnimId.SwordmanHurt);
            await RegisterOne(resLoader, $"{A}/bloodboom.ani", AnimId.SwordmanBloodboom);
            await RegisterOne(resLoader, $"{A}/hardattack.ani", AnimId.HardAttack);

            // 上挑（F2/F3 自带攻击盒 → 帧驱动自动激活；刀光走官方 .als overlay）
            await RegisterOne(resLoader, $"{A}/up_attack.ani", AnimId.SwordmanUpAttack);
            await RegisterOne(resLoader, $"character/swordman/effect/animation/upperslash1.ani", AnimId.UpperslashFx);
            await RegisterOverlay(resLoader, $"{A}/up_attack.ani.als", AnimId.SwordmanUpAttack,
                new Dictionary<string, int> { ["sub2"] = AnimId.UpperslashFx });

            // 三段斩（5 段动画，当前用 1-3；无攻击盒 → 技能手动盒；弧光/扬尘走手组装 overlay）
            for (int i = 1; i <= 5; i++)
                await RegisterOne(resLoader, $"{A}/tripleslash{i}.ani", AnimId.SwordmanTripleSlash1 + i - 1);
            string tsFx = "character/swordman/effect/animation/tripleslash";
            for (int i = 1; i <= 5; i++)
                await RegisterOne(resLoader, $"{tsFx}/slash{i}.ani", AnimId.TripleSlashFx1 + i - 1);
            await RegisterOne(resLoader, $"{tsFx}/move1.ani", AnimId.TripleSlashMoveDust1);
            await RegisterOne(resLoader, $"{tsFx}/move2.ani", AnimId.TripleSlashMoveDust2);
            for (int i = 1; i <= 5; i++)
            {
                await RegisterOverlay(resLoader, $"{A}/tripleslash{i}_overlay.json",
                    AnimId.SwordmanTripleSlash1 + i - 1,
                    new Dictionary<string, int>
                    {
                        [$"slash{i}"] = AnimId.TripleSlashFx1 + i - 1,
                        ["move1"] = AnimId.TripleSlashMoveDust1,
                        ["move2"] = AnimId.TripleSlashMoveDust2,
                    });
            }

            // 连突刺（本体 F2-F6 自带攻击盒；剑气弹视图）
            await RegisterOne(resLoader, $"{A}/dashattackmultihit.ani", AnimId.SwordmanDashAttackMultiHit);
            await RegisterOne(resLoader, $"{P}/dashattackmultihitsub.ani", AnimId.ThrustBeam);

            // 银光落刃（下落刺击本体 + 落地冲击波环/尘土）
            await RegisterOne(resLoader, $"{A}/jumpattack.ani", AnimId.SwordmanJumpAttack);
            await RegisterOne(resLoader, $"{P}/ashenforksub.ani", AnimId.AshenForkSubRing);
            await RegisterOne(resLoader, $"{P}/ashenforksubdust.ani", AnimId.AshenForkSubDust);

            // 跳跃：jump.ani 两个 10000ms 悬停帧（DNF 物理切帧哨兵）按悬停帧切成 2 段，
            // 运行时由跳跃物理驱动切换（起跳=JumpUp，最高点转下落=JumpFall）——LSFlightComponentSystem
            await RegisterSliced(resLoader, $"{A}/jump.ani", new (int start, int end, int animId)[]
            {
                (0, 6, AnimId.JumpUp),      // F0-F6 起跳段 600ms
                (8, 13, AnimId.JumpFall),   // F8-F13 下落段 520ms（F7/F14 悬停帧丢弃，F15 落地帧由落地逻辑切走）
            });

            // ---- 第 2 批（2026-08-29）----

            // 空中连斩（融入普攻：空中 X 首击复用 JumpAttack，链斩交替）
            await RegisterOne(resLoader, $"{A}/jumpattackmultislash1.ani", AnimId.JumpAttackMultiSlash1);
            await RegisterOne(resLoader, $"{A}/jumpattackmultislash2.ani", AnimId.JumpAttackMultiSlash2);

            // 崩山击（蓄力→前跃下砸 F3-F5 自带盒→落地冲击波）
            await RegisterOne(resLoader, $"{A}/hopsmashready.ani", AnimId.SwordmanHopSmashReady);
            await RegisterOne(resLoader, $"{A}/hopsmash.ani", AnimId.SwordmanHopSmash);
            await RegisterOne(resLoader, $"{P}/hopsmashsubfront1.ani", AnimId.HopSmashWaveFront);
            await RegisterOne(resLoader, $"{P}/hopsmashsubfront2.ani", AnimId.HopSmashWaveGlow);

            // 裂波斩（上斩→旋转→波轮多段→终结；波轮 F13 悬停帧已钳 80ms）
            await RegisterOne(resLoader, $"{A}/vaneslashtry.ani", AnimId.SwordmanVaneSlashTry);
            await RegisterOne(resLoader, $"{A}/vaneslash.ani", AnimId.SwordmanVaneSlash);
            await RegisterOne(resLoader, $"{P}/vaneslashwheel.ani", AnimId.VaneSlashWheel);
            await RegisterOne(resLoader, $"{P}/vaneslashnormal.ani", AnimId.VaneSlashNormal);

            // 十字斩（两刀+血之十字两相位+追击）
            await RegisterOne(resLoader, $"{A}/gorecross.ani", AnimId.SwordmanGoreCross);
            await RegisterOne(resLoader, $"{P}/gorecross1.ani", AnimId.GoreCrossFlash);
            await RegisterOne(resLoader, $"{P}/gorecross2.ani", AnimId.GoreCrossCross);
            await RegisterOne(resLoader, $"{P}/gorecross3.ani", AnimId.GoreCross3Cross);
            await RegisterOne(resLoader, $"{P}/gorecross4.ani", AnimId.GoreCross3CrossFade);

            // 里·鬼剑术（太刀 4 段，json 自带攻击盒；刀光走 .als overlay——含 followParent 新节）
            string wc = "character/swordman/effect/animation/weaponcombo";
            await RegisterOne(resLoader, $"{wc}/katananew_1_1.ani", AnimId.KatananewFx11);
            await RegisterOne(resLoader, $"{wc}/katananew_1_2.ani", AnimId.KatananewFx12);
            await RegisterOne(resLoader, $"{wc}/katananew_1-1.ani", AnimId.KatananewFx1m1);
            await RegisterOne(resLoader, $"{wc}/ura_katana_eff.ani", AnimId.UraKatanaEff);
            await RegisterOne(resLoader, $"{wc}/katananew_2_1.ani", AnimId.KatananewFx21);
            await RegisterOne(resLoader, $"{wc}/katananew_2_2.ani", AnimId.KatananewFx22);
            await RegisterOne(resLoader, $"{wc}/katananew_2-1.ani", AnimId.KatananewFx2m1);
            await RegisterOne(resLoader, $"{wc}/katananew_2-2.ani", AnimId.KatananewFx2m2);
            await RegisterOne(resLoader, $"{wc}/katana_new1_under_effect.ani", AnimId.KatanaNew1Under);
            await RegisterOne(resLoader, $"{wc}/katana_new1_upper_effect.ani", AnimId.KatanaNew1Upper);
            await RegisterOne(resLoader, $"{wc}/katananew_3_1.ani", AnimId.KatananewFx31);
            await RegisterOne(resLoader, $"{wc}/katananew_3_2.ani", AnimId.KatananewFx32);
            await RegisterOne(resLoader, $"{wc}/katananew_3-1.ani", AnimId.KatananewFx3m1);
            await RegisterOne(resLoader, $"{wc}/katananew_3-2.ani", AnimId.KatananewFx3m2);
            await RegisterOne(resLoader, $"{wc}/katana_new2_under_effect.ani", AnimId.KatanaNew2Under);
            await RegisterOne(resLoader, $"{wc}/katana_new2_upper_effect.ani", AnimId.KatanaNew2Upper);
            Dictionary<string, int> blade1Alias = new()
            {
                ["katananew_1_1"] = AnimId.KatananewFx11,
                ["katananew_1_2"] = AnimId.KatananewFx12,
                ["katananew_1-1"] = AnimId.KatananewFx1m1,
                ["ura_katana_eff"] = AnimId.UraKatanaEff,
            };
            await RegisterOverlay(resLoader, $"{A}/weaponcomboblade1.ani.als", AnimId.SwordmanWeaponComboBlade1, blade1Alias);
            Dictionary<string, int> blade2Alias = new()
            {
                ["katananew_2_1"] = AnimId.KatananewFx21,
                ["katananew_2_2"] = AnimId.KatananewFx22,
                ["katananew_2-1"] = AnimId.KatananewFx2m1,
                ["katananew_2-2"] = AnimId.KatananewFx2m2,
                ["katana_new1_under_effect"] = AnimId.KatanaNew1Under,
                ["katana_new1_upper_effect"] = AnimId.KatanaNew1Upper,
            };
            await RegisterOverlay(resLoader, $"{A}/weaponcomboblade2.ani.als", AnimId.SwordmanWeaponComboBlade2, blade2Alias);
            Dictionary<string, int> blade3Alias = new()
            {
                ["katananew_1_1"] = AnimId.KatananewFx11,
                ["katananew_1_2"] = AnimId.KatananewFx12,
                ["katananew_1-1"] = AnimId.KatananewFx1m1,
                ["ura_katana_eff"] = AnimId.UraKatanaEff,
            };
            await RegisterOverlay(resLoader, $"{A}/weaponcomboblade3.ani.als", AnimId.SwordmanWeaponComboBlade3, blade3Alias);
            Dictionary<string, int> blade4Alias = new()
            {
                ["katananew_3_1"] = AnimId.KatananewFx31,
                ["katananew_3_2"] = AnimId.KatananewFx32,
                ["katananew_3-1"] = AnimId.KatananewFx3m1,
                ["katananew_3-2"] = AnimId.KatananewFx3m2,
                ["katana_new2_under_effect"] = AnimId.KatanaNew2Under,
                ["katana_new2_upper_effect"] = AnimId.KatanaNew2Upper,
            };
            await RegisterOverlay(resLoader, $"{A}/weaponcomboblade4.ani.als", AnimId.SwordmanWeaponComboBlade4, blade4Alias);

            // ---- 第 3 批（2026-08-29）----

            // 月光斩（三段 json 自带盒 → 帧驱动；月牙/满月斩光手组装 overlay）
            await RegisterOne(resLoader, $"{A}/moonlightslash1.ani", AnimId.SwordmanMoonlightSlash1);
            await RegisterOne(resLoader, $"{A}/moonlightslash2.ani", AnimId.SwordmanMoonlightSlash2);
            await RegisterOne(resLoader, $"{A}/moonlightslashfull.ani", AnimId.SwordmanMoonlightSlashFull);
            string mf = "character/swordman/effect/animation";
            await RegisterOne(resLoader, $"{mf}/moonlightslashfx1.ani", AnimId.MoonlightSlashFx1);
            await RegisterOne(resLoader, $"{mf}/moonlightslashfx2.ani", AnimId.MoonlightSlashFx2);
            await RegisterOne(resLoader, $"{mf}/moonlightslashfxfull.ani", AnimId.MoonlightSlashFxFull);
            await RegisterOverlay(resLoader, $"{A}/moonlightslash1_overlay.json", AnimId.SwordmanMoonlightSlash1,
                new Dictionary<string, int> { ["moonlightfx1"] = AnimId.MoonlightSlashFx1 });
            await RegisterOverlay(resLoader, $"{A}/moonlightslash2_overlay.json", AnimId.SwordmanMoonlightSlash2,
                new Dictionary<string, int> { ["moonlightfx2"] = AnimId.MoonlightSlashFx2 });
            await RegisterOverlay(resLoader, $"{A}/moonlightslashfull_overlay.json", AnimId.SwordmanMoonlightSlashFull,
                new Dictionary<string, int> { ["moonlightfxfull"] = AnimId.MoonlightSlashFxFull });

            // 邪光斩（施法 = wave.ani 切片 F1-F8，DNF 引擎借用同款；波体 PO loop + 挥剑特效 overlay）
            await RegisterSliced(resLoader, $"{A}/wave.ani", new (int start, int end, int animId)[]
            {
                (1, 8, AnimId.SwordmanWaveCast),   // F1-F8 挥剑 500ms（F0 10000ms 蓄势哨兵帧丢弃）
            });
            await RegisterOne(resLoader, $"{P}/grandwavewheel.ani", AnimId.GrandWaveWheel);
            await RegisterOne(resLoader, $"{P}/grandwave_GrandWave_light_GrandWave_light.ani", AnimId.GrandWaveLight);
            await RegisterOne(resLoader, $"{P}/grandwave_GrandWave_light_GrandWave1.ani", AnimId.GrandWaveLight1);
            await RegisterOverlay(resLoader, $"{P}/grandwavewheel.ani.als", AnimId.GrandWaveWheel,
                new Dictionary<string, int>
                {
                    ["light_GrandWave1"] = AnimId.GrandWaveLight1,
                    ["light_GrandWave_light"] = AnimId.GrandWaveLight,
                });
            await RegisterOne(resLoader, $"{mf}/grandwavefx.ani", AnimId.GrandWaveFx);
            await RegisterOverlay(resLoader, $"{A}/wave_overlay.json", AnimId.SwordmanWaveCast,
                new Dictionary<string, int> { ["grandwavefx"] = AnimId.GrandWaveFx });

            // 拔刀斩（F0=500 蓄势原帧直用；大波视觉区）
            await RegisterOne(resLoader, $"{A}/momentaryslash.ani", AnimId.SwordmanMomentarySlash);
            await RegisterOne(resLoader, $"{P}/momentaryslashwave.ani", AnimId.MomentarySlashWave);
            await RegisterOne(resLoader, $"{P}/momentaryslashwaveb.ani", AnimId.MomentarySlashWaveB);

            // 破军升龙击（冲撞/上挑 json 自带盒；上挑弧光 = 上挑区视觉）
            await RegisterOne(resLoader, $"{A}/chargecrashdash.ani", AnimId.SwordmanChargeCrashDash);
            await RegisterOne(resLoader, $"{A}/chargecrashupper.ani", AnimId.SwordmanChargeCrashUpper);
            await RegisterOne(resLoader, $"{mf}/chargecrashupslash.ani", AnimId.ChargeCrashUpSlash);
            await RegisterOne(resLoader, $"{P}/chargecrashsubback.ani", AnimId.ChargeCrashSubBack);

            // 怒气爆发（血柱全套：前段 + 主血柱 .als 挂 blood1-8 + 内圈宽柱）
            await RegisterOne(resLoader, $"{P}/blastbloodpresubback.ani", AnimId.BlastBloodPre);
            await RegisterOne(resLoader, $"{P}/blastbloodpresubfront.ani", AnimId.BlastBloodPreFront);
            await RegisterOne(resLoader, $"{P}/blastblood1.ani", AnimId.BlastBlood1);
            await RegisterOne(resLoader, $"{P}/blastbloodsub.ani", AnimId.BlastBloodCore);
            string bb = "passiveobject/character/swordman/animation";
            await RegisterOne(resLoader, $"{bb}/blastblood_blood1.ani", AnimId.BlastBloodBlood1);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood2.ani", AnimId.BlastBloodBlood2);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood3.ani", AnimId.BlastBloodBlood3);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood4.ani", AnimId.BlastBloodBlood4);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood5.ani", AnimId.BlastBloodBlood5);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood6.ani", AnimId.BlastBloodBlood6);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood7.ani", AnimId.BlastBloodBlood7);
            await RegisterOne(resLoader, $"{bb}/blastblood_blood8.ani", AnimId.BlastBloodBlood8);
            await RegisterOne(resLoader, $"{bb}/blastblood_floor_over.ani", AnimId.BlastBloodFloorOver);
            await RegisterOne(resLoader, $"{bb}/blastblood_blast_blood_light.ani", AnimId.BlastBloodLight);
            Dictionary<string, int> blastBloodAlias = new()
            {
                ["blood1"] = AnimId.BlastBloodBlood1,
                ["blood2"] = AnimId.BlastBloodBlood2,
                ["blood3"] = AnimId.BlastBloodBlood3,
                ["blood4"] = AnimId.BlastBloodBlood4,
                ["blood5"] = AnimId.BlastBloodBlood5,
                ["blood6"] = AnimId.BlastBloodBlood6,
                ["blood7"] = AnimId.BlastBloodBlood7,
                ["blood8"] = AnimId.BlastBloodBlood8,
                ["floor_over"] = AnimId.BlastBloodFloorOver,
                ["blast_blood_light"] = AnimId.BlastBloodLight,
            };
            await RegisterOverlay(resLoader, $"{P}/blastblood1.ani.als", AnimId.BlastBlood1, blastBloodAlias);

            // 鬼斩刀光特效 + overlay（手组装：无 .als，直接用 .ani 文件名做别名）
            string ha = $"character/swordman/effect/animation/hardattack";
            await RegisterOne(resLoader, $"{ha}/hardattack1.ani", AnimId.HardAttackBlade1);
            await RegisterOne(resLoader, $"{ha}/hardattack2.ani", AnimId.HardAttackBlade2);
            Dictionary<string, int> hardAttackAlias = new()
            {
                ["hardattack1"] = AnimId.HardAttackBlade1,
                ["hardattack2"] = AnimId.HardAttackBlade2,
            };
            await RegisterOverlay(resLoader, $"{ha}/hardattack_blade_overlay.json",
                AnimId.HardAttack, hardAttackAlias);

            // 浴血之怒特效
            await RegisterOne(resLoader, $"{E}/boom1_bloodboom_casting.ani", AnimId.BloodboomCasting);
            await RegisterOne(resLoader, $"{E}/boom1_bloodboom_casting_back.ani", AnimId.BloodboomCastingBack);
            await RegisterOne(resLoader, $"{E}/boom1_bloodboom_boomfront.ani", AnimId.BloodboomBoomFront);
            await RegisterOne(resLoader, $"{E}/boom1_bloodboom_boomback.ani", AnimId.BloodboomBoomBack);

            // 浴血之怒施法特效叠加
            Dictionary<string, int> bloodboomAlias = new()
            {
                ["casting_bloodboom_casting_back"] = AnimId.BloodboomCastingBack,
                ["casting_bloodboom_casting"] = AnimId.BloodboomCasting,
            };
            await RegisterOverlay(resLoader, $"{A}/bloodboom.ani.als", AnimId.SwordmanBloodboom, bloodboomAlias);

            // 波动剑
            await RegisterOne(resLoader, $"{P}/normalwave.ani", AnimId.NormalWave);

            // 冰息弹（手组装，保留旧 JSON，地址仍用文件名）
            await RegisterOne(resLoader, "icebreath_bullet1.json", AnimId.IceBreathBullet1);
            await RegisterOne(resLoader, "icebreath_bullet2.json", AnimId.IceBreathBullet2);
            Dictionary<string, int> bulletAlias = new() { ["icebreath2"] = AnimId.IceBreathBullet2 };
            await RegisterOverlay(resLoader, "icebreath_bullet_overlay.json", AnimId.IceBreathBullet1, bulletAlias);
        }

        private static async ETTask RegisterOne(ResourcesLoaderComponent resLoader, string address, int animId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(address);
            if (asset == null)
            {
                Log.Warning($"[LSAnimClip] 找不到: {address}");
                return;
            }
            AnimClipData data = JsonUtility.FromJson<AnimClipData>(asset.text);
            AnimConfigRegistry.Register(animId, data);
            Log.Info($"[LSAnimClip] {animId}: {data.frames.Length} frames, loop={data.loop}");
        }

        /// <summary>切片注册：一个 .ani.json 按帧段切出多个动画（jump.ani 悬停帧专用——DNF 物理切帧哨兵
        /// 在纯时长驱动动画系统里不能原样消费）。帧 index 保留原值（视图层只按数组序推进）。</summary>
        private static async ETTask RegisterSliced(ResourcesLoaderComponent resLoader, string address,
            (int start, int end, int animId)[] slices)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(address);
            if (asset == null)
            {
                Log.Warning($"[LSAnimClip] 切片源找不到: {address}");
                return;
            }
            AnimClipData full = JsonUtility.FromJson<AnimClipData>(asset.text);
            if (full?.frames == null)
            {
                Log.Warning($"[LSAnimClip] 切片源解析失败: {address}");
                return;
            }
            foreach ((int start, int end, int animId) in slices)
            {
                AnimFrameData[] frames = new AnimFrameData[end - start + 1];
                int total = 0;
                for (int i = start; i <= end; i++)
                {
                    frames[i - start] = full.frames[i];
                    total += full.frames[i].delay;
                }
                AnimConfigRegistry.Register(animId, new AnimClipData
                {
                    loop = false,
                    frames = frames,
                    frameMax = frames.Length,
                    totalDuration = total,
                });
                Log.Info($"[LSAnimClip] 切片 {animId}: F{start}-F{end} {total}ms");
            }
        }

        private static async ETTask RegisterOverlay(ResourcesLoaderComponent resLoader, string address, int parentAnimId,
            Dictionary<string, int> aliasToAnimId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(address);
            if (asset == null)
            {
                Log.Warning($"[LSAnimClip] overlay 找不到: {address}");
                return;
            }
            AnimOverlayConfig config = JsonUtility.FromJson<AnimOverlayConfig>(asset.text);
            if (config?.overlays == null || config.overlays.Length == 0)
            {
                Log.Warning($"[LSAnimClip] overlay 为空：{address}");
                return;
            }
            foreach (AnimOverlayEntry entry in config.overlays)
            {
                if (aliasToAnimId.TryGetValue(entry.effectAni, out int animId))
                    entry.effectAnimId = animId;
                else
                    entry.effectAnimId = AnimId.None;
            }
            AnimConfigRegistry.RegisterOverlay(parentAnimId, config);
            Log.Info($"[LSAnimClip] overlay {parentAnimId}: {config.overlays.Length} 层");
        }
    }
}
