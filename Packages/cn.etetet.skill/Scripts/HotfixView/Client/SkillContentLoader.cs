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
            // 三类内容同源注册：技能 / Buff 配置 / Action 效果节点（ContentLoader 泛型）
            SkillLoader.RegisterAssembly(assembly);
            BuffLoader.RegisterAssembly(assembly);
            ActionLoader.RegisterAssembly(assembly);
        }
    }
}
