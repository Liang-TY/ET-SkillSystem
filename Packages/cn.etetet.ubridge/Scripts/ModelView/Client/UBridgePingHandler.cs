using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// Ping 命令处理器
    /// 连通性检测，返回 Editor 当前状态
    /// </summary>
    public static class UBridgePingHandler
    {
        public static string Handle(string payloadJson)
        {
            PingResponse response = PingResponse.Create();
            response.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            response.IsCompiling = EditorApplication.isCompiling;
            response.IsPlaying = EditorApplication.isPlaying;
            response.IsPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
            response.CodeMode = GetCodeMode();
            response.UnityVersion = Application.unityVersion;
            return UBridgeJsonHelper.ToJson(response);
        }

        /// <summary>
        /// 反射读取 ET.GlobalConfig 的 CodeMode 字段
        /// </summary>
        private static string GetCodeMode()
        {
            try
            {
                UnityEngine.Object config = Resources.Load("GlobalConfig");
                if (config != null)
                {
                    FieldInfo field = config.GetType().GetField("CodeMode",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        object value = field.GetValue(config);
                        return value?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }
    }
}