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
