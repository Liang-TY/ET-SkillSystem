using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 动画 clip 注册：加载 .ani.bytes JSON → AnimConfigRegistry.Register。
    /// 文件路径镜像 PVF 目录结构（如 character/swordman/animation/stay.ani.bytes）。
    /// </summary>
    public static partial class LSAnimClipRegistrar
    {
        private const string Res = "Packages/cn.etetet.lockstep/Bundles/AnimRes";

        public static async ETTask RegisterAll(Scene root)
        {
            ResourcesLoaderComponent resLoader = root.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSAnimClip] ResourcesLoaderComponent 不存在");
                return;
            }

            // 怪物动画（bantuamazones）
            string mob = $"{Res}/monster/event/bluemarble/bantuamazones/baanimation";
            await RegisterOne(resLoader, $"{mob}/stay.ani.bytes", AnimId.Idle);
            await RegisterOne(resLoader, $"{mob}/move.ani.bytes", AnimId.Walk);
            await RegisterOne(resLoader, $"{mob}/kneekick.ani.bytes", AnimId.Attack1);
            await RegisterOne(resLoader, $"{mob}/damage.ani.bytes", AnimId.Hurt);
            await RegisterOne(resLoader, $"{mob}/lowkick.ani.bytes", AnimId.MonsterLowKick);
            await RegisterOne(resLoader, $"{mob}/highkick.ani.bytes", AnimId.MonsterHighKick);
            await RegisterOne(resLoader, $"{mob}/icebreath.ani.bytes", AnimId.MonsterIceBreath);
            await RegisterOne(resLoader, $"{mob}/down.ani.bytes", AnimId.MonsterDown);

            // 鬼剑士动画
            string sw = $"{Res}/character/swordman/animation";
            await RegisterOne(resLoader, $"{sw}/stay.ani.bytes", AnimId.SwordmanIdle);
            await RegisterOne(resLoader, $"{sw}/move.ani.bytes", AnimId.SwordmanWalk);
            await RegisterOne(resLoader, $"{sw}/attack1.ani.bytes", AnimId.SwordmanAttack1);
            await RegisterOne(resLoader, $"{sw}/attack2.ani.bytes", AnimId.SwordmanAttack2);
            await RegisterOne(resLoader, $"{sw}/attack3.ani.bytes", AnimId.SwordmanAttack3);
            await RegisterOne(resLoader, $"{sw}/damage1.ani.bytes", AnimId.SwordmanHurt);
            await RegisterOne(resLoader, $"{sw}/bloodboom.ani.bytes", AnimId.SwordmanBloodboom);

            // 浴血之怒特效
            string bb = $"{Res}/character/swordman/effect/animation/bloodboom";
            await RegisterOne(resLoader, $"{bb}/boom1_bloodboom_casting.ani.bytes", AnimId.BloodboomCasting);
            await RegisterOne(resLoader, $"{bb}/boom1_bloodboom_casting_back.ani.bytes", AnimId.BloodboomCastingBack);
            await RegisterOne(resLoader, $"{bb}/boom1_bloodboom_boomfront.ani.bytes", AnimId.BloodboomBoomFront);
            await RegisterOne(resLoader, $"{bb}/boom1_bloodboom_boomback.ani.bytes", AnimId.BloodboomBoomBack);

            // 浴血之怒施法特效叠加（bloodboom.ani.als）
            Dictionary<string, int> bloodboomAlias = new()
            {
                ["casting_bloodboom_casting_back"] = AnimId.BloodboomCastingBack,
                ["casting_bloodboom_casting"] = AnimId.BloodboomCasting,
            };
            await RegisterOverlay(resLoader, $"{sw}/bloodboom.ani.als.bytes",
                AnimId.SwordmanBloodboom, bloodboomAlias);

            // 波动剑（被动对象动画）
            await RegisterOne(resLoader, $"{Res}/passiveobject/character/swordman/animation/normalwave.ani.bytes", AnimId.NormalWave);

            // 冰息弹（手组装，无 .ani 源，保留旧 JSON）
            await RegisterOne(resLoader, $"{Res}/icebreath_bullet1.json", AnimId.IceBreathBullet1);
            await RegisterOne(resLoader, $"{Res}/icebreath_bullet2.json", AnimId.IceBreathBullet2);
            Dictionary<string, int> bulletAlias = new() { ["icebreath2"] = AnimId.IceBreathBullet2 };
            await RegisterOverlay(resLoader, $"{Res}/icebreath_bullet_overlay.json",
                AnimId.IceBreathBullet1, bulletAlias);
        }

        private static async ETTask RegisterOne(ResourcesLoaderComponent resLoader, string path, int animId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
            if (asset == null)
            {
                Log.Warning($"[LSAnimClip] 找不到: {path}");
                return;
            }
            AnimClipData data = JsonUtility.FromJson<AnimClipData>(asset.text);
            AnimConfigRegistry.Register(animId, data);
            Log.Info($"[LSAnimClip] {animId}: {data.frames.Length} frames, loop={data.loop}");
        }

        private static async ETTask RegisterOverlay(ResourcesLoaderComponent resLoader, string path, int parentAnimId,
            Dictionary<string, int> aliasToAnimId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
            if (asset == null)
            {
                Log.Warning($"[LSAnimClip] overlay 找不到: {path}");
                return;
            }
            AnimOverlayConfig config = JsonUtility.FromJson<AnimOverlayConfig>(asset.text);
            if (config?.overlays == null || config.overlays.Length == 0)
            {
                Log.Warning($"[LSAnimClip] overlay 为空：{path}");
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
