using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace ET
{
    /// <summary>
    /// 技能内容独立编译菜单：dotnet build DotNet~/ET.SkillContent.csproj →
    /// 输出 DLL/PDB 改名 .bytes 拷到 Bundles/SkillContent/（YooAsset 收集，运行时 Assembly.Load）。
    /// 前置：热更框架（ET.Skill）产物在 Temp/Bin/Debug——没有就先按 F6（ET/Loader/Compile）。
    /// 改技能内容只需本菜单（不触发 Unity 全工程编译）；改框架（skill/Runtime）才需要 F6。
    /// </summary>
    public static class SkillCompileTool
    {
        private const string Csproj = "Packages/cn.etetet.skill/DotNet~/ET.SkillContent.csproj";
        private const string HotOutput = "Temp/Bin/Debug/ET.Skill.dll";
        private const string BinDir = "Packages/cn.etetet.skill/DotNet~/bin";
        private const string OutDir = "Packages/cn.etetet.skill/Bundles/SkillContent";
        private const string DllName = "ET.SkillContent";

        [MenuItem("ET/Skill/Compile")]
        static void Compile()
        {
            if (!File.Exists(HotOutput))
            {
                UnityEngine.Debug.LogError($"[SkillCompile] 缺 {HotOutput}（热更框架产物）——请先按 F6（ET/Loader/Compile）再执行本菜单");
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{Csproj}\" -v q",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();
            // 完整日志落盘（UTF-8），Console 只显示真正的 error 行（警告 MSB3277 版本冲突无害，别刷屏）
            File.WriteAllText("Temp/SkillCompile.log", output, System.Text.Encoding.UTF8);
            if (process.ExitCode != 0)
            {
                var errorLines = new System.Text.StringBuilder();
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("error") || line.Contains("错误"))
                        errorLines.AppendLine(line.TrimEnd('\r'));
                }
                UnityEngine.Debug.LogError(
                    $"[SkillCompile] dotnet build 失败（ExitCode={process.ExitCode}）。\n" +
                    $"--- error 行 ---\n{errorLines}\n--- 完整日志：Temp/SkillCompile.log ---");
                return;
            }

            string srcDll = $"{BinDir}/{DllName}.dll";
            string srcPdb = $"{BinDir}/{DllName}.pdb";
            if (!File.Exists(srcDll))
            {
                UnityEngine.Debug.LogError($"[SkillCompile] 编译成功但找不到输出 {srcDll}");
                return;
            }

            Directory.CreateDirectory(OutDir);
            File.Copy(srcDll, $"{OutDir}/{DllName}.dll.bytes", true);
            if (File.Exists(srcPdb)) File.Copy(srcPdb, $"{OutDir}/{DllName}.pdb.bytes", true);

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[SkillCompile] 完成：{OutDir}/{DllName}.dll.bytes");
        }
    }
}
