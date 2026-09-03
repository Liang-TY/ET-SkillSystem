using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ET.Editor
{
    /// <summary>
    /// 磁盘事实层（02 §5 DocumentStore + AssetCatalog 的 Step 1 合并实现）：
    /// 目录扫描、按整数 id 寻址、文件哈希、保存外壳。
    /// 只做加载/回写，不做校验（校验在 SkillEditorValidation），
    /// 不碰 SkillParamLoader 全局缓存（03 §6.5）。
    /// </summary>
    internal sealed class SkillEditorDocumentStore
    {
        private readonly Dictionary<SkillEditorAssetKind, List<SkillEditorAsset>> catalog = new();

        /// <summary>按类型取目录快照；同一次编辑会话内缓存，Invalidate 后重扫。</summary>
        public IReadOnlyList<SkillEditorAsset> Scan(SkillEditorAssetKind kind, bool force = false)
        {
            if (force || !catalog.TryGetValue(kind, out List<SkillEditorAsset> list))
            {
                list = SkillEditorAssetCatalog.Scan(kind);
                catalog[kind] = list;
            }
            return list;
        }

        public void InvalidateCatalog() => catalog.Clear();

        public SkillEditorAsset Find(SkillEditorAssetKind kind, int id)
        {
            foreach (SkillEditorAsset asset in Scan(kind))
            {
                if (asset.Error == null && asset.Id == id) return asset;
            }
            return null;
        }

        /// <summary>校验跨表引用用的 id 集合（只含能正常解析的文件）。</summary>
        public HashSet<int> CollectIds(SkillEditorAssetKind kind)
        {
            HashSet<int> ids = new();
            foreach (SkillEditorAsset asset in Scan(kind))
            {
                if (asset.Error == null) ids.Add(asset.Id);
            }
            return ids;
        }

        public bool Save(SkillEditorDocument document, out string error)
        {
            error = null;
            if (document == null)
            {
                error = "没有可保存的文档";
                return false;
            }
            try
            {
                document.Save();
                return true;
            }
            catch (Exception e)
            {
                error = $"保存失败（原文件保留）: {e.Message}";
                return false;
            }
        }

        /// <summary>文件当前字节的 sha256（03 §5 expectedHash 基准）；文件不存在返回 null。</summary>
        public static string ComputeSha256(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
