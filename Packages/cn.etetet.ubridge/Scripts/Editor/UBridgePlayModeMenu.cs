using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// UBridge Editor Play 模式切换：控制 ET.Model 从 Library 加载还是从 .bytes 加载
    /// </summary>
    public static class UBridgePlayModeMenu
    {
        private const string ConfigPath = "Assets/Resources/ETModelLoadFromBytes.json";

        [MenuItem("ET/LockStep/设置EditorPlay从Bytes加载ET.Model", false, 10)]
        public static void SetPlayLoadFromBytes()
        {
            if (!File.Exists(ConfigPath))
            {
                File.WriteAllText(ConfigPath, "{ \"_comment\": \"存在时 Editor Play 从 .bytes 加载 ET.Model（默认热更行为），删除后从 Library 加载（UBridge Play 可用）\" }");
                AssetDatabase.Refresh();
                Debug.Log("[UBridge] Editor Play 将从 .bytes 加载 ET.Model");
            }
            else
            {
                Debug.Log("[UBridge] 已是从 .bytes 加载模式");
            }
        }

        [MenuItem("ET/LockStep/设置EditorPlay从Library加载ET.Model", false, 11)]
        public static void SetPlayLoadFromLibrary()
        {
            if (File.Exists(ConfigPath))
            {
                AssetDatabase.DeleteAsset(ConfigPath);
                Debug.Log("[UBridge] Editor Play 将从 Library 加载 ET.Model（CLI 可用）");
            }
            else
            {
                Debug.Log("[UBridge] 已是从 Library 加载模式");
            }
        }
    }
}
