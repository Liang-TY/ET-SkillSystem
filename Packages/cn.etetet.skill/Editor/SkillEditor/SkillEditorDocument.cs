using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ET.Editor
{
    internal enum SkillEditorAssetKind
    {
        Skill,
        Bullet,
        Area,
        Buff,
        Action,
    }

    internal sealed class SkillEditorAsset
    {
        public SkillEditorAssetKind Kind;
        public string Path;
        public int Id;
        public string Name;
        public string Error;

        public string DisplayName
            => Error == null ? $"{Id}  {Name}" : $"!  {System.IO.Path.GetFileName(Path)}  ({Error})";
    }

    /// <summary>
    /// Editor-side document. The runtime model is intentionally read-only, so the
    /// editor keeps the mutable JSON DTO and serializes it back without introducing
    /// a second asset format.
    /// </summary>
    internal sealed class SkillEditorDocument
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
        };

        private static readonly JsonSerializerSettings WriteSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Culture = System.Globalization.CultureInfo.InvariantCulture,
        };

        public readonly SkillEditorAsset Asset;
        public object Raw { get; private set; }

        private SkillEditorDocument(SkillEditorAsset asset, object raw)
        {
            Asset = asset;
            Raw = raw;
        }

        public SkillParamJson Skill => Raw as SkillParamJson;
        public BulletParamJson Bullet => Raw as BulletParamJson;
        public AreaParamJson Area => Raw as AreaParamJson;
        public BuffParamJson Buff => Raw as BuffParamJson;
        public ActionParamJson Action => Raw as ActionParamJson;

        public static bool TryLoad(SkillEditorAsset asset, out SkillEditorDocument document, out string error)
        {
            document = null;
            error = null;
            try
            {
                string json = File.ReadAllText(asset.Path);
                object raw = asset.Kind switch
                {
                    SkillEditorAssetKind.Skill => JsonConvert.DeserializeObject<SkillParamJson>(json, SerializerSettings),
                    SkillEditorAssetKind.Bullet => JsonConvert.DeserializeObject<BulletParamJson>(json, SerializerSettings),
                    SkillEditorAssetKind.Area => JsonConvert.DeserializeObject<AreaParamJson>(json, SerializerSettings),
                    SkillEditorAssetKind.Buff => JsonConvert.DeserializeObject<BuffParamJson>(json, SerializerSettings),
                    SkillEditorAssetKind.Action => JsonConvert.DeserializeObject<ActionParamJson>(json, SerializerSettings),
                    _ => null,
                };
                if (raw == null)
                {
                    error = "根对象为空";
                    return false;
                }

                document = new SkillEditorDocument(asset, raw);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public string CaptureSnapshot()
            => JsonConvert.SerializeObject(Raw, Formatting.None, WriteSettings);

        public bool RestoreSnapshot(string snapshot, out string error)
        {
            error = null;
            try
            {
                Raw = Asset.Kind switch
                {
                    SkillEditorAssetKind.Skill => JsonConvert.DeserializeObject<SkillParamJson>(snapshot, SerializerSettings),
                    SkillEditorAssetKind.Bullet => JsonConvert.DeserializeObject<BulletParamJson>(snapshot, SerializerSettings),
                    SkillEditorAssetKind.Area => JsonConvert.DeserializeObject<AreaParamJson>(snapshot, SerializerSettings),
                    SkillEditorAssetKind.Buff => JsonConvert.DeserializeObject<BuffParamJson>(snapshot, SerializerSettings),
                    SkillEditorAssetKind.Action => JsonConvert.DeserializeObject<ActionParamJson>(snapshot, SerializerSettings),
                    _ => null,
                };
                return Raw != null;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public void Save()
        {
            Save((SkillParamJson)Raw);
        }

        /// <summary>保存指定 DTO（patch 流程用：引擎改裸 DTO 后落盘）。原子替换。</summary>
        public void Save(SkillParamJson skill)
        {
            string json = JsonConvert.SerializeObject(skill ?? Raw, WriteSettings);
            string temporaryPath = Asset.Path + ".tmp";
            File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
            // 同卷原子替换：复制中途失败不会截断原文件（03 §6.2 失败时保留原文件）
            if (File.Exists(Asset.Path)) File.Replace(temporaryPath, Asset.Path, null);
            else File.Move(temporaryPath, Asset.Path);
        }

        public int Id
        {
            get => Skill?.id ?? Bullet?.id ?? Area?.id ?? Buff?.id ?? Action?.id ?? 0;
        }

        public string Name
        {
            get => Skill?.name ?? Bullet?.name ?? Area?.name ?? Buff?.name ?? Action?.name;
        }
    }

    internal static class SkillEditorAssetCatalog
    {
        public const string Root = "Packages/cn.etetet.skill/Bundles/SkillParams";

        public static List<SkillEditorAsset> Scan(SkillEditorAssetKind kind)
        {
            string directory = System.IO.Path.Combine(Root, DirectoryName(kind));
            List<SkillEditorAsset> result = new();
            if (!Directory.Exists(directory)) return result;

            string[] files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                SkillEditorAsset asset = new() { Kind = kind, Path = file };
                if (SkillEditorDocument.TryLoad(asset, out SkillEditorDocument document, out string error))
                {
                    asset.Id = document.Id;
                    asset.Name = document.Name ?? string.Empty;
                }
                else
                {
                    asset.Error = error;
                }
                result.Add(asset);
            }
            return result;
        }

        public static string DirectoryName(SkillEditorAssetKind kind)
            => kind switch
            {
                SkillEditorAssetKind.Skill => "skills",
                SkillEditorAssetKind.Bullet => "bullets",
                SkillEditorAssetKind.Area => "areas",
                SkillEditorAssetKind.Buff => "buffs",
                SkillEditorAssetKind.Action => "actions",
                _ => string.Empty,
            };
    }
}
