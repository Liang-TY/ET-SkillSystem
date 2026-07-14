using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ET
{
    /// <summary>
    /// 桥接路径辅助
    /// </summary>
    public static class UBridgePathHelper
    {
        /// <summary>环境变量名，可覆盖默认根目录</summary>
        public const string EnvVarName = "ET_UNITY_BRIDGE_ROOT";

        public static string ResolveRoot()
        {
            string envRoot = Environment.GetEnvironmentVariable(EnvVarName);
            if (!string.IsNullOrEmpty(envRoot))
                return envRoot;

            return Path.GetFullPath("Temp/UnityBridge");
        }

        public static string GetRequestsDir(string root)     => Path.Combine(root, "requests");
        public static string GetProcessingDir(string root)   => Path.Combine(root, "processing");
        public static string GetResponsesDir(string root)    => Path.Combine(root, "responses");
        public static string GetDeadLetterDir(string root)   => Path.Combine(root, "deadletter");
        public static string GetRequestPath(string root, string rpcId) => Path.Combine(root, "requests", $"{rpcId}.json");
        public static string GetResponsePath(string root, string rpcId) => Path.Combine(root, "responses", $"{rpcId}.json");

        public static void EnsureDirectories(string root)
        {
            Directory.CreateDirectory(GetRequestsDir(root));
            Directory.CreateDirectory(GetProcessingDir(root));
            Directory.CreateDirectory(GetResponsesDir(root));
            Directory.CreateDirectory(GetDeadLetterDir(root));
        }
    }

    /// <summary>
    /// 文件系统原子操作
    /// </summary>
    public static class UBridgeFileStore
    {
#if !UBRIDGE_CLI
        [StaticField]
#endif
        private static string m_Root;

        public static void Initialize(string root)
        {
            m_Root = root;
            UBridgePathHelper.EnsureDirectories(root);
        }

#if !UBRIDGE_CLI
        [StaticField]
#endif
        public static string Root => m_Root;

        /// <summary>
        /// 原子写入：先写临时文件，再 Move 到目标路径
        /// </summary>
        public static void WriteTextAtomic(string path, string content)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tmpPath = path + ".tmp";
            File.WriteAllText(tmpPath, content, Encoding.UTF8);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmpPath, path);
        }

        /// <summary>
        /// 写请求到 requests/ 目录
        /// </summary>
        public static string WriteRequest(string rpcId, string content)
        {
            string path = UBridgePathHelper.GetRequestPath(m_Root, rpcId);
            WriteTextAtomic(path, content);
            return path;
        }

        /// <summary>
        /// 尝试取下一个请求（原子锁：File.Move 到 processing/）
        /// 返回 (null, null) 表示没有待处理的请求
        /// </summary>
        public static (string rpcId, string content) TryTakeNextRequest()
        {
            string requestsDir = UBridgePathHelper.GetRequestsDir(m_Root);
            if (!Directory.Exists(requestsDir))
                return (null, null);

            foreach (string filePath in Directory.GetFiles(requestsDir, "*.json"))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string processingPath = Path.Combine(UBridgePathHelper.GetProcessingDir(m_Root), Path.GetFileName(filePath));

                Directory.CreateDirectory(UBridgePathHelper.GetProcessingDir(m_Root));

                try
                {
                    // 原子 Move：如果多个进程竞争，只有一个能成功
                    File.Move(filePath, processingPath);
                }
                catch (IOException)
                {
                    continue; // 被其他进程抢走了，试下一个
                }

                string content = File.ReadAllText(processingPath, Encoding.UTF8);
                File.Delete(processingPath);
                return (fileName, content);
            }

            return (null, null);
        }

        /// <summary>
        /// 写响应到 responses/ 目录
        /// </summary>
        public static void WriteResponse(string rpcId, string content)
        {
            string path = UBridgePathHelper.GetResponsePath(m_Root, rpcId);
            WriteTextAtomic(path, content);
        }

        /// <summary>
        /// 读取指定 rpcId 的响应，读取后删除
        /// </summary>
        public static string TryReadResponse(string rpcId)
        {
            string path = UBridgePathHelper.GetResponsePath(m_Root, rpcId);
            if (!File.Exists(path))
                return null;

            string content = File.ReadAllText(path, Encoding.UTF8);
            File.Delete(path);
            return content;
        }
    }

    /// <summary>
    /// BSON JSON 序列化辅助（使用 ET.Core 的 MongoHelper）
    /// </summary>
    public static class UBridgeJsonHelper
    {
        /// <summary>
        /// 将对象序列化为带 _t 判别器的 BSON JSON
        /// </summary>
        public static string ToJson(object value)
        {
            if (value == null)
                return string.Empty;

            return MongoHelper.ToJson(value);
        }

        /// <summary>
        /// 从 BSON JSON 反序列化为指定类型
        /// </summary>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return MongoHelper.FromJson<T>(json);
        }

        /// <summary>
        /// 从 BSON JSON 反序列化为指定类型
        /// </summary>
        public static object FromJson(Type type, string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return MongoHelper.FromJson(type, json);
        }
    }
}