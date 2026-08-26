using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// NPK 挂载管理器：管理多个 NpkArchive，提供统一的虚拟路径查找与按需提取。
    /// 后挂载的归档优先查找（与 DNF 的 mod 覆盖行为一致）。
    /// </summary>
    public class NpkMountManager : IDisposable
    {
        /// <summary>归档名 → NpkArchive（保持插入顺序，后挂载的覆盖先挂载的同名条目）</summary>
        private readonly List<NpkArchive> _archives = new List<NpkArchive>();

        /// <summary>归档名 → 索引（用于按名卸载）</summary>
        private readonly Dictionary<string, NpkArchive> _archiveMap = new Dictionary<string, NpkArchive>(StringComparer.OrdinalIgnoreCase);

        /// <summary>文件名（小写，如 "bantuamazones.img"）→ 完整虚拟路径（短期方案：JSON 用简单文件名，这里反查）</summary>
        private readonly Dictionary<string, string> _filenameLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>已挂载归档数</summary>
        public int Count => _archives.Count;

        /// <summary>挂载一个 NPK 归档。如已存在同名归载则先卸载再挂载（替换）。</summary>
        /// <param name="name">归档名（如 "sprite_monster_bantu"，建议与文件名一致）</param>
        /// <param name="npkBytes">NPK 文件完整字节流</param>
        public void Mount(string name, byte[] npkBytes)
        {
            // 已存在则先卸载（支持热替换）
            if (_archiveMap.ContainsKey(name))
            {
                Unmount(name);
            }

            NpkArchive archive = NpkArchive.Mount(name, npkBytes);
            _archives.Add(archive);
            _archiveMap[name] = archive;

            // 构建文件名反查表（后挂载覆盖先挂载的同名文件，与 DNF mod 覆盖行为一致）
            foreach (string virtualPath in archive.VirtualPaths)
            {
                string filename = System.IO.Path.GetFileName(virtualPath);
                if (!string.IsNullOrEmpty(filename))
                {
                    _filenameLookup[filename] = virtualPath;
                }
            }
        }

        /// <summary>卸载一个 NPK 归档（释放其字节流引用和索引）</summary>
        public void Unmount(string name)
        {
            if (_archiveMap.TryGetValue(name, out NpkArchive archive))
            {
                _archives.Remove(archive);
                _archiveMap.Remove(name);
                archive.Dispose();
            }
        }

        /// <summary>卸载所有归档</summary>
        public void UnmountAll()
        {
            foreach (NpkArchive archive in _archives)
            {
                archive.Dispose();
            }
            _archives.Clear();
            _archiveMap.Clear();
        }

        /// <summary>
        /// 按文件名（如 "bantuamazones.img"）反查并提取。
        /// 短期方案：JSON 用简单文件名，这里通过文件名→虚拟路径反查表定位。
        /// 长期方案：JSON 改用完整虚拟路径，此方法可废弃。
        /// </summary>
        public byte[] ReadByFilename(string filename)
        {
            if (_filenameLookup.TryGetValue(filename, out string virtualPath))
            {
                return Read(virtualPath);
            }
            return null;
        }

        /// <summary>
        /// 按虚拟路径提取条目字节。从最后挂载的归档开始向前查找（后挂载优先）。
        /// 返回 null 如果所有已挂载归档中都不存在该路径。
        /// </summary>
        public byte[] Read(string virtualPath)
        {
            // 逆序遍历：后挂载的优先（mod 覆盖行为）
            for (int i = _archives.Count - 1; i >= 0; i--)
            {
                if (_archives[i].Contains(virtualPath))
                {
                    return _archives[i].Extract(virtualPath);
                }
            }
            return null;
        }

        /// <summary>检查虚拟路径是否存在于任何已挂载归档</summary>
        public bool Contains(string virtualPath)
        {
            for (int i = _archives.Count - 1; i >= 0; i--)
            {
                if (_archives[i].Contains(virtualPath))
                    return true;
            }
            return false;
        }

        /// <summary>获取虚拟路径所在的归档名（调试用）</summary>
        public string GetArchiveName(string virtualPath)
        {
            for (int i = _archives.Count - 1; i >= 0; i--)
            {
                if (_archives[i].Contains(virtualPath))
                    return _archives[i].Name;
            }
            return null;
        }

        /// <summary>列出所有已挂载归档的名称</summary>
        public IReadOnlyList<string> GetArchiveNames()
        {
            var names = new List<string>(_archiveMap.Keys);
            return names;
        }

        public void Dispose()
        {
            UnmountAll();
        }
    }
}
