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
