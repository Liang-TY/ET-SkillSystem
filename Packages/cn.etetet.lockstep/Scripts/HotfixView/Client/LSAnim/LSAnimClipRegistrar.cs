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
        }

        private static async ETTask RegisterOne(ResourcesLoaderComponent resLoader, string path, int animId)
        {
            TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(path);
            AnimClipData data = JsonUtility.FromJson<AnimClipData>(asset.text);
            AnimConfigRegistry.Register(animId, data);
            Log.Info($"[LSAnimClip] Registered anim {animId}: {data.frames.Length} frames, loop={data.loop}, total={data.totalDuration}ms");
        }
    }
}
