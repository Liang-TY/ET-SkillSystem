using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ET.Editor
{
    /// <summary>
    /// 只读探针（08-Step3）：验证 Editor 侧能否拿到 AnimId → 动画资源映射。
    /// 不修改任何运行时/锁步/资源代码，仅直读磁盘 + 打印结果。
    /// </summary>
    public static class SkillAnimResProbe
    {
        private const string AnimResRoot = "Packages/cn.etetet.lockstep/Bundles/AnimRes/";

        [MenuItem("ET/Skill/AnimResProbe")]
        public static void Run()
        {
            var result = new Dictionary<string, object>
            {
                ["registrarAssembly"] = "ET.HotfixView",   // LSAnimClipRegistrar 所在
                ["registryAssembly"] = "ET.NpkParser",     // AnimConfigRegistry/AnimClipData 所在
                ["registryReferencable"] = true,           // 本探针能编译通过即证明 ET.NpkParser 可访问
            };

            // 验证 AnimId 常量 + AnimConfigRegistry 静态可调用（未注册时 Get 返回 null）
            result["animIdHardAttack"] = AnimId.HardAttack;
            result["registryCallable"] = AnimConfigRegistry.Get(AnimId.HardAttack) == null;

            // AnimId=49 HardAttack
            var anim49 = new Dictionary<string, object>();
            string aniPath = AnimResRoot + "character/swordman/animation/hardattack.ani.bytes";
            if (File.Exists(aniPath))
            {
                AnimClipData data = JsonUtility.FromJson<AnimClipData>(File.ReadAllText(aniPath));
                bool hasFrames = data != null && data.frames != null && data.frames.Length > 0;
                anim49["found"] = hasFrames;
                anim49["frames"] = data?.frames?.Length ?? 0;
                anim49["firstFramePath"] = hasFrames ? data.frames[0].image.path : "";
                anim49["pivot"] = hasFrames ? new[] { data.frames[0].imagePos.x, data.frames[0].imagePos.y } : new[] { 0, 0 };
                anim49["frameDelayMs"] = hasFrames ? data.frames[0].delay : 0;
                anim49["totalDuration"] = data?.totalDuration ?? 0;
            }
            else
            {
                anim49["found"] = false;
                anim49["error"] = $"文件不存在: {aniPath}";
            }
            result["animId49"] = anim49;

            // overlay（hardattack1/hardattack2 刀光）
            var overlay = new Dictionary<string, object>();
            string ovPath = AnimResRoot + "character/swordman/effect/animation/hardattack/hardattack_blade_overlay.json";
            if (File.Exists(ovPath))
            {
                AnimOverlayConfig cfg = JsonUtility.FromJson<AnimOverlayConfig>(File.ReadAllText(ovPath));
                overlay["found"] = cfg != null && cfg.overlays != null;
                overlay["layers"] = cfg?.overlays?.Length ?? 0;
            }
            else
            {
                overlay["found"] = false;
                overlay["error"] = $"文件不存在: {ovPath}";
            }
            result["animId49Overlay"] = overlay;

            result["resourceReadMode"] = "直读磁盘 File.ReadAllText（未走 YooAsset/未收集）";

            var notes = new List<string>
            {
                "AnimConfigRegistry/AnimClipData/AnimId 在 ET.NpkParser，Editor 通过 ET.Skill 传递引用可访问（registryReferencable=true 即验证）",
                "LSAnimClipRegistrar（AnimId→.ani 地址映射）在 ET.HotfixView，Editor 引用不到——本探针硬编码了 49→hardattack 地址",
            };
            result["notes"] = notes;

            string outDir = "automation/results";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            string outJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(outDir + "/animres-probe.json", outJson);

            Debug.Log($"[AnimResProbe] 完成，输出 {outDir}/animres-probe.json\n{outJson}");
        }
    }
}
