using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ET
{
    /// <summary>
    /// NPK 归档挂载器（lazy 模式）：只读 header + 索引表建虚拟路径查找，不解析任何 IMG 内容。
    /// 按需提取：Extract(virtualPath) 从字节流按 offset/size 取指定条目。
    /// 基于 parse-img-ani 的 NpkAccess.cs 改造——去掉 eagerly 解析 IMG 的逻辑。
    /// </summary>
    public class NpkArchive : IDisposable
    {
        private const int MagicLength = 16;
        private const int EntryHeaderSize = 8;   // offset(4) + length(4)
        private const int NameSize = 256;
        private const int IndexEntrySize = EntryHeaderSize + NameSize; // 264

        private static readonly byte[] Magic = new byte[]
        {
            (byte)'N', (byte)'e', (byte)'o', (byte)'p', (byte)'l', (byte)'e',
            (byte)'P', (byte)'a', (byte)'c', (byte)'k', (byte)'_', (byte)'B',
            (byte)'i', (byte)'l', (byte)'l', (byte)'\0'
        };

        /// <summary>XOR 解密密钥（DNF 固定值，"puchikon@neople dungeon and fighter DNF..."）</summary>
        private static readonly byte[] NameKey = InitializeNameKey();

        /// <summary>NPK 完整字节流（由外部持有，本类不负责释放）</summary>
        private byte[] _rawBytes;

        /// <summary>虚拟路径 → (offset, length)。key 已解密，统一小写比较。</summary>
        private Dictionary<string, NpkEntryInfo> _entries;

        /// <summary>归档名（如 "sprite_monster_bantu"）</summary>
        public string Name { get; private set; }

        /// <summary>条目总数</summary>
        public int Count => _entries?.Count ?? 0;

        /// <summary>所有已注册的虚拟路径（已解密）</summary>
        public IEnumerable<string> VirtualPaths => _entries?.Keys ?? (IEnumerable<string>)Array.Empty<string>();

        /// <summary>
        /// 挂载 NPK 归档：解析 header + 索引表，建立虚拟路径查找。
        /// 不解析任何 IMG 内容（lazy）。SHA-256 校验可选。
        /// </summary>
        /// <param name="name">归档名（如 "sprite_monster_bantu"）</param>
        /// <param name="npkBytes">NPK 文件完整字节流</param>
        /// <param name="verifyChecksum">是否验证 SHA-256 校验和（默认跳过，性能考虑）</param>
        /// <exception cref="ArgumentException">魔数不匹配或格式无效</exception>
        public static NpkArchive Mount(string name, byte[] npkBytes, bool verifyChecksum = false)
        {
            var archive = new NpkArchive
            {
                Name = name,
                _rawBytes = npkBytes,
                _entries = new Dictionary<string, NpkEntryInfo>(StringComparer.OrdinalIgnoreCase),
            };

            archive.ParseIndex(verifyChecksum);
            return archive;
        }

        /// <summary>
        /// 按虚拟路径提取条目的原始字节（不含 IMG 解析）。
        /// 返回 null 如果路径不存在。
        /// </summary>
        public byte[] Extract(string virtualPath)
        {
            if (_entries == null || !_entries.TryGetValue(virtualPath, out NpkEntryInfo entry))
                return null;

            byte[] result = new byte[entry.Length];
            Array.Copy(_rawBytes, entry.Offset, result, 0, entry.Length);
            return result;
        }

        /// <summary>检查虚拟路径是否存在于本归档</summary>
        public bool Contains(string virtualPath)
        {
            return _entries != null && _entries.ContainsKey(virtualPath);
        }

        /// <summary>尝试获取条目信息</summary>
        public bool TryGetEntry(string virtualPath, out int offset, out int length)
        {
            offset = 0;
            length = 0;
            if (_entries == null || !_entries.TryGetValue(virtualPath, out NpkEntryInfo entry))
                return false;

            offset = entry.Offset;
            length = entry.Length;
            return true;
        }

        public void Dispose()
        {
            _rawBytes = null;
            _entries?.Clear();
            _entries = null;
        }

        // ---------------- 内部实现 ----------------

        private void ParseIndex(bool verifyChecksum)
        {
            if (_rawBytes == null || _rawBytes.Length < MagicLength + 4)
                throw new ArgumentException($"NPK 文件太小: {_rawBytes?.Length ?? 0} bytes");

            // 验证魔数
            for (int i = 0; i < MagicLength; i++)
            {
                if (_rawBytes[i] != Magic[i])
                    throw new ArgumentException($"NPK 魔数不匹配: 期望 'NeoplePack_Bill\\0'");
            }

            // 读条目数
            int entryCount = BitConverter.ToInt32(_rawBytes, MagicLength);

            // 验证索引区完整
            int indexStart = MagicLength + 4;
            int indexEnd = indexStart + entryCount * IndexEntrySize;
            if (indexEnd > _rawBytes.Length)
                throw new ArgumentException($"NPK 索引区越界: 需要 {indexEnd} 字节，实际 {_rawBytes.Length}");

            // 可选 SHA-256 校验（索引区后面 32 字节是校验和）
            if (verifyChecksum && indexEnd + 32 <= _rawBytes.Length)
            {
                VerifyChecksum(indexEnd);
            }

            // 解析索引表（跳过 SHA-256 区，数据区从索引+校验和之后开始）
            int dataStart = indexEnd + 32; // 索引区 + SHA-256 校验和

            for (int i = 0; i < entryCount; i++)
            {
                int entryOffset = BitConverter.ToInt32(_rawBytes, indexStart + i * IndexEntrySize);
                int entryLength = BitConverter.ToInt32(_rawBytes, indexStart + i * IndexEntrySize + 4);

                // 解密名称
                int nameStart = indexStart + i * IndexEntrySize + EntryHeaderSize;
                string name = DecryptName(_rawBytes, nameStart);

                if (!string.IsNullOrEmpty(name))
                {
                    _entries[name] = new NpkEntryInfo { Offset = entryOffset, Length = entryLength };
                }
            }
        }

        private void VerifyChecksum(int checksumOffset)
        {
            // header = 魔数(16) + 条目数(4) + 索引区(entryCount * 264)
            int headerLength = checksumOffset;
            int specimenLength = (headerLength / 17) * 17;

            using var sha256 = SHA256.Create();
            byte[] computed = sha256.ComputeHash(_rawBytes, 0, specimenLength);

            for (int i = 0; i < 32; i++)
            {
                if (computed[i] != _rawBytes[checksumOffset + i])
                {
                    throw new ArgumentException($"NPK SHA-256 校验失败: {Name}");
                }
            }
        }

        private static string DecryptName(byte[] source, int offset)
        {
            // XOR 解密（密钥为 DNF 固定值）
            var decrypted = new byte[NameSize];
            for (int i = 0; i < NameSize; i++)
            {
                decrypted[i] = (byte)(source[offset + i] ^ NameKey[i]);
            }

            // 去掉 null 尾部
            int length = 0;
            while (length < NameSize && decrypted[length] != 0) length++;

            return Encoding.UTF8.GetString(decrypted, 0, length);
        }

        private static byte[] InitializeNameKey()
        {
            var key = new byte[NameSize];
            string baseStr = "puchikon@neople dungeon and fighter ";
            byte[] baseBytes = Encoding.ASCII.GetBytes(baseStr);

            Array.Copy(baseBytes, 0, key, 0, baseBytes.Length);

            int pos = baseBytes.Length;
            while (pos < NameSize)
            {
                key[pos++] = (byte)'D';
                if (pos >= NameSize) break;
                key[pos++] = (byte)'N';
                if (pos >= NameSize) break;
                key[pos++] = (byte)'F';
            }

            key[255] = 0;
            return key;
        }

        private struct NpkEntryInfo
        {
            public int Offset;
            public int Length;
        }
    }
}
