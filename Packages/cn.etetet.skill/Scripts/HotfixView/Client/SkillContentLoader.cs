using System.Reflection;
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
            // 四类内容同源注册：技能 / Buff 配��� / Action 效果节点 / 投射物配置（ContentLoader 泛型）
            SkillLoader.RegisterAssembly(assembly);
            BuffLoader.RegisterAssembly(assembly);
            ActionLoader.RegisterAssembly(assembly);
            BulletLoader.RegisterAssembly(assembly);
            AreaLoader.RegisterAssembly(assembly);

            // 技能系统配置 json（改配置零编译：改 json → YooAsset 重收集 → Play）
            TextAsset configAsset = await resLoader.LoadAssetAsync<TextAsset>(
                "Packages/cn.etetet.lockstep/Bundles/AnimRes/skillconfig.json");
            if (configAsset != null)
            {
                SkillSystemConfigData data = JsonUtility.FromJson<SkillSystemConfigData>(configAsset.text);
                SkillSystemConfig.HitFlashEnabled = data.hitFlashEnabled;
                SkillSystemConfig.ScreenShakeEnabled = data.screenShakeEnabled;
                SkillSystemConfig.DebugDrawHitbox = data.debugDrawHitbox;
                Log.Info($"[SkillConfig] 加载：hitFlash={SkillSystemConfig.HitFlashEnabled} " +
                         $"screenShake={SkillSystemConfig.ScreenShakeEnabled} debugBox={SkillSystemConfig.DebugDrawHitbox}");
            }
        }
    }
}
