using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 动画 clip 注册：加载 .ani.bytes JSON → AnimConfigRegistry.Register。
    /// AnimId → 地址/overlay/切片 映射已抽到 LSAnimAddressTable（ET.NpkParser，ISSUE-013 方案 A1），
    /// 运行时与 Editor 共用同一张表；本类只负责 YooAsset 加载与注册时序。
    /// YooAsset 地址 = AnimRes 下的相对路径（去扩展名），如 character/swordman/animation/stay.ani
    /// </summary>
    public static partial class LSAnimClipRegistrar
    {
        public static async ETTask RegisterAll(Scene root)
        {
            ResourcesLoaderComponent resLoader = root.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[LSAnimClip] ResourcesLoaderComponent 不存在");
                return;
            }

            foreach (LSAnimAddressTable.Entry entry in LSAnimAddressTable.Entries)
            {
                if (entry.Address != null)
                    await RegisterOne(resLoader, entry.Address, entry.AnimId);
                if (entry.Overlay != null)
                    await RegisterOverlay(resLoader, entry.Overlay.File, entry.AnimId, entry.Overlay.Aliases);
            }

            foreach (LSAnimAddressTable.SliceDef sliced in LSAnimAddressTable.SlicedEntries)
                await RegisterSliced(resLoader, sliced.SourceAddress, sliced.Segments);
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
