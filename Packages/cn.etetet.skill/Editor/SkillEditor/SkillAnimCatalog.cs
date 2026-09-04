using System.Collections.Generic;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// Editor 动画资源目录（02 §5 AnimCatalog）：读 LSAnimAddressTable（ET.NpkParser，
    /// 与运行时单一真源）定位 AnimId → 资源文件；直读磁盘解析 AnimClipData（EditorSimMode
    /// 实证 File.ReadAllText 可行，见 08 探针结论）。不进 Play、不碰 YooAsset。
    /// </summary>
    internal static class SkillAnimCatalog
    {
        private const string AnimResRoot = "Packages/cn.etetet.lockstep/Bundles/AnimRes";

        // static readonly 字段随域重载自动重建（域重载 = 重新执行类初始化），无需特殊标注
        private static readonly Dictionary<int, AnimClipData> cache = new();

        private static readonly Dictionary<int, LSAnimAddressTable.Entry> entryById = BuildIndex();

        private static Dictionary<int, LSAnimAddressTable.Entry> BuildIndex()
        {
            Dictionary<int, LSAnimAddressTable.Entry> index = new();
            foreach (LSAnimAddressTable.Entry entry in LSAnimAddressTable.Entries)
            {
                if (entry.Address != null && !index.ContainsKey(entry.AnimId))
                    index[entry.AnimId] = entry;
            }
            return index;
        }

        public static bool IsKnown(int animId) => entryById.ContainsKey(animId);

        /// <summary>表里的原始地址（overlay-only 条目除外）；未知返回 null。</summary>
        public static string GetAddress(int animId)
            => entryById.TryGetValue(animId, out LSAnimAddressTable.Entry entry) ? entry.Address : null;

        /// <summary>表条目（overlay 配置随条目携带）；未知返回 null。</summary>
        public static LSAnimAddressTable.Entry GetEntry(int animId)
            => entryById.TryGetValue(animId, out LSAnimAddressTable.Entry entry) ? entry : null;

        /// <summary>
        /// 取 AnimClipData：先查缓存，再直读磁盘解析（含切片复现）。
        /// 失败返回 null 并记 error（调用方显示品红/线框占位与原因）。
        /// </summary>
        public static AnimClipData GetClip(int animId, out string error)
        {
            error = null;
            if (animId <= 0)
            {
                error = "animId 未设置";
                return null;
            }
            if (cache.TryGetValue(animId, out AnimClipData cached)) return cached;

            // 切片动画：源 clip 按帧段复现（与运行时 RegisterSliced 同逻辑）
            foreach (LSAnimAddressTable.SliceDef sliced in LSAnimAddressTable.SlicedEntries)
            {
                foreach ((int start, int end, int id) in sliced.Segments)
                {
                    if (id != animId) continue;
                    AnimClipData source = LoadClip(sliced.SourceAddress, out string sourceError);
                    if (source == null)
                    {
                        error = $"切片源加载失败: {sourceError}";
                        return null;
                    }
                    AnimClipData slicedClip = Slice(source, start, end);
                    cache[animId] = slicedClip;
                    return slicedClip;
                }
            }

            AnimClipData clip = LoadClip(GetAddress(animId) ?? string.Empty, out error);
            if (clip != null) cache[animId] = clip;
            return clip;
        }

        /// <summary>
        /// AnimationFrame 时间基准的唯一换算：atFrame → clip 内累计毫秒
        /// （与运行时 CurrentFrameIndex 推帧一致：delay<=0 用 50ms）。atFrame 越界返回 -1。
        /// </summary>
        public static int FrameToMs(AnimClipData clip, int atFrame)
        {
            if (clip?.frames == null || atFrame < 0 || atFrame > clip.frames.Length) return -1;
            int elapsed = 0;
            for (int i = 0; i < atFrame && i < clip.frames.Length; i++)
                elapsed += clip.frames[i].delay > 0 ? clip.frames[i].delay : 50;
            return elapsed;   // atFrame == Length 时 = 全片时长（Landing 近似用）
        }

        /// <summary>overlay 配置（别名→AnimId 已按表解析）。</summary>
        public static AnimOverlayConfig GetOverlay(int animId)
        {
            if (!entryById.TryGetValue(animId, out LSAnimAddressTable.Entry entry) || entry.Overlay == null)
                return null;
            if (!TryResolveFile(entry.Overlay.File, out string path)) return null;
            AnimOverlayConfig config = JsonUtility.FromJson<AnimOverlayConfig>(
                System.IO.File.ReadAllText(path));
            if (config?.overlays == null) return null;
            foreach (AnimOverlayEntry overlayEntry in config.overlays)
            {
                overlayEntry.effectAnimId = entry.Overlay.Aliases != null
                    && entry.Overlay.Aliases.TryGetValue(overlayEntry.effectAni, out int id)
                        ? id
                        : AnimId.None;
            }
            return config;
        }

        /// <summary>地址 → 磁盘文件：地址去 .bytes（YooAsset AddressByFileName 语义），磁盘实际带 .bytes
        /// （.ani→.ani.bytes、.als→.als.bytes；.json overlay 无 .bytes）。先试原样，失败再补 .bytes。</summary>
        private static bool TryResolveFile(string address, out string path)
        {
            path = System.IO.Path.Combine(AnimResRoot, address);
            if (System.IO.File.Exists(path)) return true;
            string withBytes = path + ".bytes";
            if (System.IO.File.Exists(withBytes))
            {
                path = withBytes;
                return true;
            }
            return false;
        }

        private static AnimClipData LoadClip(string address, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(address))
            {
                error = "地址表中无此 AnimId";
                return null;
            }
            if (!TryResolveFile(address, out string path))
            {
                error = $"资源文件不存在: {address}";
                return null;
            }
            try
            {
                AnimClipData clip = JsonUtility.FromJson<AnimClipData>(System.IO.File.ReadAllText(path));
                if (clip?.frames == null || clip.frames.Length == 0)
                {
                    error = $"clip 无帧: {address}";
                    return null;
                }
                return clip;
            }
            catch (System.Exception e)
            {
                error = $"解析失败 {address}: {e.Message}";
                return null;
            }
        }

        private static AnimClipData Slice(AnimClipData source, int start, int end)
        {
            AnimFrameData[] frames = new AnimFrameData[end - start + 1];
            int total = 0;
            for (int i = start; i <= end; i++)
            {
                frames[i - start] = source.frames[i];
                total += source.frames[i].delay;
            }
            return new AnimClipData { loop = false, frames = frames, frameMax = frames.Length, totalDuration = total };
        }
    }
}
