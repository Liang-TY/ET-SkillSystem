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
