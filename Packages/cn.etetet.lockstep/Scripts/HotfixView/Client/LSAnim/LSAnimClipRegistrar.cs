using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视图层（HotfixView/Client）：在场景初始化时（room.Init 之前）注册动画 clip。
    /// 放视图层因为要用 ResourcesLoaderComponent（在 ModelView 程序集，Hotfix 看不到）。
    /// 时机：由 LSSceneChangeStart_AddComponent（订阅场景切换事件，await PublishAsync 完成后才 room.Init）调用。
    /// clip 含 delay/damageBox 等逻辑数据，但"加载 JSON"这步客户端只能走视图层资源系统；
    /// 服务器以后改走配置表。
    /// </summary>
    public static partial class LSAnimClipRegistrar
    {
        public static async ETTask RegisterAll(Scene root)
        {
            ResourcesLoaderComponent resLoader = root.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSAnimClip] ResourcesLoaderComponent 不存在，跳过 clip 注册");
                return;
            }
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/stay.json", AnimId.Idle);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/move.json", AnimId.Walk);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/kneekick.json", AnimId.Attack1);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/damage.json", AnimId.Hurt);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/normalwave.json", AnimId.NormalWave);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/firecircle.json", AnimId.FireCircle);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/firecircleend.json", AnimId.FireCircleEnd);
            // 鬼剑士（玩家）
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_stay.json", AnimId.SwordmanIdle);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_move.json", AnimId.SwordmanWalk);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_attack1.json", AnimId.SwordmanAttack1);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_attack2.json", AnimId.SwordmanAttack2);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_attack3.json", AnimId.SwordmanAttack3);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_damage.json", AnimId.SwordmanHurt);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_bloodboom.json", AnimId.SwordmanBloodboom);
            // 浴血之怒特效（bloodboom_cast_overlay.json 的别名 → AnimId）
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bloodboom_casting.json", AnimId.BloodboomCasting);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bloodboom_casting_back.json", AnimId.BloodboomCastingBack);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bloodboom_boomfront.json", AnimId.BloodboomBoomFront);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bloodboom_boomback.json", AnimId.BloodboomBoomBack);

            // 浴血之怒施法特效叠加（bloodboom.ani.als 翻译产物 → 挂在施法动画上）
            Dictionary<string, int> bloodboomAlias = new()
            {
                ["casting_bloodboom_casting_back"] = AnimId.BloodboomCastingBack,
                ["casting_bloodboom_casting"] = AnimId.BloodboomCasting,
                // 其余 4 别名（boomfront/boomback/circle1/circle2）是空占位动画（本客户端版本无贴图），不翻译不映射
            };
            await RegisterOverlay(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/bloodboom_cast_overlay.json",
                AnimId.SwordmanBloodboom, bloodboomAlias);

            // 波动爆发（角色冲刺 + 特效 + 爆炸）
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/swordman_releasewavedash.json", AnimId.SwordmanReleaseWaveDash);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_creature.json", AnimId.ReleaseWaveCreature);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_creature_01.json", AnimId.ReleaseWaveCreature01);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_bodyglow.json", AnimId.ReleaseWaveBodyGlow);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_speedline.json", AnimId.ReleaseWaveSpeedLine);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_speedline_01.json", AnimId.ReleaseWaveSpeedLine01);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_speedline_01_01.json", AnimId.ReleaseWaveSpeedLine01_01);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_speedline_01_02.json", AnimId.ReleaseWaveSpeedLine01_02);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_center.json", AnimId.ReleaseWaveCenter);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_center_01.json", AnimId.ReleaseWaveCenter01);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_elec02.json", AnimId.ReleaseWaveElec02);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_elec02_01.json", AnimId.ReleaseWaveElec02_01);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_backwind.json", AnimId.ReleaseWaveBackwind);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_castlightning.json", AnimId.ReleaseWaveCastLightning);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_circle.json", AnimId.ReleaseWaveCircle);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_smoke.json", AnimId.ReleaseWaveSmoke);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_gust.json", AnimId.ReleaseWaveGust);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_burst1.json", AnimId.ReleaseWaveBurst1);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_burst2.json", AnimId.ReleaseWaveBurst2);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_burst3.json", AnimId.ReleaseWaveBurst3);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_burst4.json", AnimId.ReleaseWaveBurst4);

            // 冲刺 .als（releasewavedash_body.ani.als，[none effect add] 11 层全映射）
            Dictionary<string, int> dashAlias = new()
            {
                ["ReleaseWaveIDash_creature"] = AnimId.ReleaseWaveCreature,
                ["ReleaseWaveIDash_creature_01"] = AnimId.ReleaseWaveCreature01,
                ["ReleaseWaveIDash_bodyGlow"] = AnimId.ReleaseWaveBodyGlow,
                ["ReleaseWaveIDash_StartSpeedLine"] = AnimId.ReleaseWaveSpeedLine,
                ["ReleaseWaveIDash_StartSpeedLine_01"] = AnimId.ReleaseWaveSpeedLine01,
                ["ReleaseWaveIDash_StartSpeedLine_01_01"] = AnimId.ReleaseWaveSpeedLine01_01,
                ["ReleaseWaveIDash_StartSpeedLine_01_02"] = AnimId.ReleaseWaveSpeedLine01_02,
                ["ReleaseWaveIDash_CenterElectric3"] = AnimId.ReleaseWaveCenter,
                ["ReleaseWaveIDash_CenterElectric3_01"] = AnimId.ReleaseWaveCenter01,
                ["ReleaseWaveIDash_ExplosionElectric02"] = AnimId.ReleaseWaveElec02,
                ["ReleaseWaveIDash_ExplosionElectric02_01"] = AnimId.ReleaseWaveElec02_01,
            };
            await RegisterOverlay(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_dash_overlay.json",
                AnimId.SwordmanReleaseWaveDash, dashAlias);

            // 爆炸 .als（手组装：releasewave1.ani.als 的 3 子层 + backwind 蓄气 5 层合并挂爆炸主动画）
            Dictionary<string, int> burstAlias = new()
            {
                ["rw_burst2"] = AnimId.ReleaseWaveBurst2,
                ["rw_burst3"] = AnimId.ReleaseWaveBurst3,
                ["rw_burst4"] = AnimId.ReleaseWaveBurst4,
                ["rw_backwind"] = AnimId.ReleaseWaveBackwind,
                ["rw_smoke"] = AnimId.ReleaseWaveSmoke,
                ["rw_gust"] = AnimId.ReleaseWaveGust,
                ["rw_castlightning"] = AnimId.ReleaseWaveCastLightning,
                ["rw_circle"] = AnimId.ReleaseWaveCircle,
            };
            await RegisterOverlay(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/rw_burst_overlay.json",
                AnimId.ReleaseWaveBurst1, burstAlias);

            // 班图女战士（怪物）：技能动画 + 倒地 + 冰息弹
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/monster_lowkick.json", AnimId.MonsterLowKick);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/monster_highkick.json", AnimId.MonsterHighKick);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/monster_icebreath.json", AnimId.MonsterIceBreath);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/monster_down.json", AnimId.MonsterDown);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/icebreath_bullet1.json", AnimId.IceBreathBullet1);
            await RegisterOne(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/icebreath_bullet2.json", AnimId.IceBreathBullet2);

            // 冰息弹第二层视觉（BantuIceBreath1.obj [add object effect] → .als overlay 挂主层）
            Dictionary<string, int> bulletAlias = new() { ["icebreath2"] = AnimId.IceBreathBullet2 };
            await RegisterOverlay(resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/icebreath_bullet_overlay.json",
                AnimId.IceBreathBullet1, bulletAlias);
        }

        private static async ETTask RegisterOne(ResourcesLoaderComponent resLoader, string path, int animId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
            AnimClipData data = JsonUtility.FromJson<AnimClipData>(asset.text);
            AnimConfigRegistry.Register(animId, data);
            Log.Info($"[LSAnimClip] Registered anim {animId}: {data.frames.Length} frames, loop={data.loop}, total={data.totalDuration}ms");
        }

        /// <summary>注册 .als 特效叠加配置（aliasToAnimId 把 .als 别名解析成 AnimId；
        /// 未映射别名该层跳过——空占位动画等场景）</summary>
        private static async ETTask RegisterOverlay(ResourcesLoaderComponent resLoader, string path, int parentAnimId,
            Dictionary<string, int> aliasToAnimId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
            AnimOverlayConfig config = JsonUtility.FromJson<AnimOverlayConfig>(asset.text);
            if (config?.overlays == null || config.overlays.Length == 0)
            {
                Log.Warning($"[LSAnimClip] overlay 配置为空：{path}，跳过");
                return;
            }
            foreach (AnimOverlayEntry entry in config.overlays)
            {
                if (aliasToAnimId.TryGetValue(entry.effectAni, out int animId))
                {
                    entry.effectAnimId = animId;
                }
                else
                {
                    entry.effectAnimId = AnimId.None;
                    Log.Warning($"[LSAnimClip] overlay 别名未映射：{entry.effectAni}（父动画 {parentAnimId}），该层跳过");
                }
            }
            AnimConfigRegistry.RegisterOverlay(parentAnimId, config);
            Log.Info($"[LSAnimClip] Registered overlay for anim {parentAnimId}: {config.overlays.Length} 层");
        }
    }
}
