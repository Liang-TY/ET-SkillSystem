using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET.UIBuilder
{
    /// <summary>
    /// ubridge 文件桥薄壳（回退通道，方案 §4.3，S5）：
    /// 反射注册 YIUIBuildPanel 到 UBridgeEditorHost（ET.Editor 程序集）。
    /// 刻意不做编译期依赖——ubridge 不在场时静默跳过，主通道（Unity CLI）不受影响。
    /// CLI 侧用法：dotnet run ET.UBridge.dll -- YIUIBuildPanel --path &lt;spec路径&gt;
    /// </summary>
    [InitializeOnLoad]
    public static class UBridgeEntry
    {
        [Serializable]
        private class BuildRequest
        {
            public string Path;
        }

        static UBridgeEntry()
        {
            try
            {
                Type hostType = Type.GetType("ET.UBridgeEditorHost, ET.Editor");
                if (hostType == null)
                    return;

                MethodInfo register = hostType.GetMethod("RegisterHandler",
                    BindingFlags.Public | BindingFlags.Static);
                if (register == null)
                    return;

                register.Invoke(null, new object[]
                {
                    "YIUIBuildPanel",
                    (Func<string, string>)(payload =>
                    {
                        BuildRequest request = JsonUtility.FromJson<BuildRequest>(payload ?? "");
                        BuildResult result = UIBuildPipeline.Build(request?.Path ?? "");
                        return JsonUtility.ToJson(result);
                    })
                });
                Debug.Log("[UIBuilder] ubridge 回退通道已注册: YIUIBuildPanel");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UIBuilder] ubridge 薄壳注册失败（忽略，主通道不受影响）: {ex.Message}");
            }
        }
    }
}
