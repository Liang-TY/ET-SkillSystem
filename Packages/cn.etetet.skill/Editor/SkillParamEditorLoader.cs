using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET.Editor
{
    /// <summary>Editor 直读 SkillParams 的小入口，供编辑器窗口和 CI 校验复用。</summary>
    public static class SkillParamEditorLoader
    {
        private const string Root = "Packages/cn.etetet.skill/Bundles/SkillParams";

        public static SkillParamValidationReport LastReport { get; private set; }

        [MenuItem("ET/Skill/Validate Params")]
        public static void ValidateFromMenu()
        {
            bool ok = ReloadFromDisk();
            if (ok) UnityEngine.Debug.Log("[SkillParams] Editor 校验通过");
        }

        public static bool ReloadFromDisk()
        {
            string root = Path.GetFullPath(Root);
            if (!Directory.Exists(root))
            {
                UnityEngine.Debug.LogError($"[SkillParams] 目录不存在：{root}");
                return false;
            }

            SkillParamLoader.Clear();
            bool loaded = true;
            loaded &= LoadDirectory(Path.Combine(root, "skills"), SkillParamFileKind.Skill);
            loaded &= LoadDirectory(Path.Combine(root, "bullets"), SkillParamFileKind.Bullet);
            loaded &= LoadDirectory(Path.Combine(root, "areas"), SkillParamFileKind.Area);
            loaded &= LoadDirectory(Path.Combine(root, "buffs"), SkillParamFileKind.Buff);
            loaded &= LoadDirectory(Path.Combine(root, "actions"), SkillParamFileKind.Action);

            string indexPath = Path.Combine(root, "index.json");
            if (File.Exists(indexPath))
                loaded &= SkillParamLoader.LoadButtonMappingsJson(File.ReadAllText(indexPath), indexPath);

            LastReport = SkillParamLoader.ValidateAll();
            return loaded && LastReport.IsValid;
        }

        private static bool LoadDirectory(string directory, SkillParamFileKind kind)
        {
            if (!Directory.Exists(directory)) return true;
            bool result = true;
            string[] files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                bool ok = kind switch
                {
                    SkillParamFileKind.Skill => SkillParamLoader.LoadSkillJson(json, file),
                    SkillParamFileKind.Bullet => SkillParamLoader.LoadBulletJson(json, file),
                    SkillParamFileKind.Area => SkillParamLoader.LoadAreaJson(json, file),
                    SkillParamFileKind.Buff => SkillParamLoader.LoadBuffJson(json, file),
                    SkillParamFileKind.Action => SkillParamLoader.LoadActionJson(json, file),
                    _ => false,
                };
                result &= ok;
            }
            return result;
        }

        private enum SkillParamFileKind
        {
            Skill,
            Bullet,
            Area,
            Buff,
            Action,
        }
    }
}
