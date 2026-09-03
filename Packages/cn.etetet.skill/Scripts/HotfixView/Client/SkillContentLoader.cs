using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能内容程序集加载（HotfixView：视图层资源系统）。
    /// 时机：LSSceneChangeStart_AddComponent 里、room.Init 之前（与 LSAnimClipRegistrar 同点）——
    /// 场景初始化 await 完成后才建 unit，技能注册必先于首次 TryCast。
    /// editor/打包统一走 resLoader（同 AnimRes 模式）；Assembly.Load 后 SkillLoader.RegisterAssembly 反射注册。
    /// </summary>
    public static partial class SkillContentLoader
    {
        private const string SkillParamRoot = "Packages/cn.etetet.skill/Bundles/SkillParams/";

        public static async ETTask Load(Scene root)
        {
            ResourcesLoaderComponent resLoader = root.GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("[SkillContent] ResourcesLoaderComponent 不存在，跳过技能内容加载");
                return;
            }

            TextAsset dllAsset = await resLoader.LoadAssetAsync<TextAsset>(
                "Packages/cn.etetet.skill/Bundles/SkillContent/ET.SkillContent.dll.bytes");
            if (dllAsset == null)
            {
                Log.Warning("[SkillContent] ET.SkillContent.dll.bytes 不存在——先执行菜单 ET/Skill/Compile");
                return;
            }

            Assembly assembly = Assembly.Load(dllAsset.bytes);
            // 七类内容同源注册：技能 / Buff / Action / 投射物 / 区域 / 怪物AI配置 / 地图（ContentLoader 泛型）
            SkillLoader.RegisterAssembly(assembly);
            BuffLoader.RegisterAssembly(assembly);
            ActionLoader.RegisterAssembly(assembly);
            BulletLoader.RegisterAssembly(assembly);
            AreaLoader.RegisterAssembly(assembly);
            MonsterAiLoader.RegisterAssembly(assembly);
            MapLoader.RegisterAssembly(assembly);

            await LoadSkillParams(resLoader);

            // Anim registrations happen immediately before this loader in the
            // scene bootstrap; refresh the display-only id directory now.
            ContentIds.RefreshAnimations();

            // 技能系统配置 json（改配置零编译：改 json → YooAsset 重收集 → Play）
            TextAsset configAsset = await resLoader.LoadAssetAsync<TextAsset>(
                "Packages/cn.etetet.lockstep/Bundles/AnimRes/skillconfig.json");
            if (configAsset != null)
            {
                SkillSystemConfigData data = JsonUtility.FromJson<SkillSystemConfigData>(configAsset.text);
                SkillSystemConfig.HitFlashEnabled = data.hitFlashEnabled;
                SkillSystemConfig.ScreenShakeEnabled = data.screenShakeEnabled;
                SkillSystemConfig.DebugDrawHitbox = data.debugDrawHitbox;
                SkillSystemConfig.RngSeed = data.rngSeed;
                Log.Info($"[SkillConfig] 加载：hitFlash={SkillSystemConfig.HitFlashEnabled} " +
                         $"screenShake={SkillSystemConfig.ScreenShakeEnabled} debugBox={SkillSystemConfig.DebugDrawHitbox} " +
                         $"rngSeed={SkillSystemConfig.RngSeed}");
            }
        }

        private static async ETTask LoadSkillParams(ResourcesLoaderComponent resLoader)
        {
            TextAsset manifestAsset = await resLoader.LoadAssetAsync<TextAsset>(SkillParamRoot + "manifest.json");
            if (manifestAsset == null)
            {
                Log.Warning("[SkillParams] manifest.json 不存在，保留已有参数缓存");
                return;
            }

            SkillParamManifestJson manifest;
            try
            {
                manifest = JsonConvert.DeserializeObject<SkillParamManifestJson>(manifestAsset.text);
            }
            catch (System.Exception e)
            {
                Log.Error($"[SkillParams] manifest.json 解析失败：{e.Message}");
                return;
            }

            if (manifest == null)
            {
                Log.Error("[SkillParams] manifest.json 根对象为空");
                return;
            }

            SkillParamLoader.Clear();
            await LoadParamFiles(resLoader, manifest.skills, SkillParamFileKind.Skill);
            await LoadParamFiles(resLoader, manifest.bullets, SkillParamFileKind.Bullet);
            await LoadParamFiles(resLoader, manifest.areas, SkillParamFileKind.Area);
            await LoadParamFiles(resLoader, manifest.buffs, SkillParamFileKind.Buff);
            await LoadParamFiles(resLoader, manifest.actions, SkillParamFileKind.Action);

            if (!string.IsNullOrWhiteSpace(manifest.index))
            {
                TextAsset indexAsset = await resLoader.LoadAssetAsync<TextAsset>(SkillParamRoot + manifest.index);
                if (indexAsset == null)
                    Log.Error($"[SkillParams] 找不到按键映射：{manifest.index}");
                else
                    SkillParamLoader.LoadButtonMappingsJson(indexAsset.text, SkillParamRoot + manifest.index);
            }

            SkillParamValidationReport report = SkillParamLoader.ValidateAll();
            if (report.IsValid)
                Log.Info($"[SkillParams] 加载完成：skills={SkillParamLoader.Skills.Count} " +
                         $"bullets={SkillParamLoader.Bullets.Count} areas={SkillParamLoader.Areas.Count} " +
                         $"buffs={SkillParamLoader.Buffs.Count} actions={SkillParamLoader.Actions.Count}");
            else
                Log.Error($"[SkillParams] 校验失败：{report.Errors.Count} 个引用错误");
        }

        private static async ETTask LoadParamFiles(
            ResourcesLoaderComponent resLoader, string[] paths, SkillParamFileKind kind)
        {
            if (paths == null) return;
            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                TextAsset asset = await resLoader.LoadAssetAsync<TextAsset>(SkillParamRoot + path);
                if (asset == null)
                {
                    Log.Error($"[SkillParams] 找不到参数文件：{path}");
                    continue;
                }

                string source = SkillParamRoot + path;
                switch (kind)
                {
                    case SkillParamFileKind.Skill: SkillParamLoader.LoadSkillJson(asset.text, source); break;
                    case SkillParamFileKind.Bullet: SkillParamLoader.LoadBulletJson(asset.text, source); break;
                    case SkillParamFileKind.Area: SkillParamLoader.LoadAreaJson(asset.text, source); break;
                    case SkillParamFileKind.Buff: SkillParamLoader.LoadBuffJson(asset.text, source); break;
                    case SkillParamFileKind.Action: SkillParamLoader.LoadActionJson(asset.text, source); break;
                }
            }
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
