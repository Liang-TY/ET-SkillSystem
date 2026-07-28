using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class AssemblyEditor
    {
        private static readonly string[] DllNames = { "ET.Hotfix", "ET.HotfixView", "ET.Model", "ET.ModelView" };
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    {
                        OnExitingEditMode();
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// 退出编辑模式时处理(即将进入运行模式)
        /// 检查 Assets/ETModelLoadFromBytes.json 配置文件决定加载方式
        /// </summary>
        static void OnExitingEditMode()
        {
            // 打包后正常运行：删 DLL 从 .bytes 加载
            if (!Application.isEditor) goto Delete;
            // Editor 下：配置文件不存在 → 保留 DLL（UBridge Play 可用）
            if (!File.Exists($"{Application.dataPath}/Resources/ETModelLoadFromBytes.json")) return;

        Delete:
            foreach (string dll in DllNames)
            {
                string dllFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll";
                if (File.Exists(dllFile)) File.Delete(dllFile);
                string pdbFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb";
                if (File.Exists(pdbFile)) File.Delete(pdbFile);
            }
        }
    }
}