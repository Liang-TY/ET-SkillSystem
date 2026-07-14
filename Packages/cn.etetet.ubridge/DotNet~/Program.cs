using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// ============================
// ET.UBridge CLI - Unity Editor M-fM-!M-%M-fM-^NM-%M-eM-^QM-=M-dM-;M-$M-hM-!M-^LM-eM-^PM-^HM-eM-^EM-7
// M-gM-^TM-(M-fM-3M-^U: dotnet run ET.UBridge.dll -- ConsoleGetLogs --count 50 --logType Error
// ============================

namespace ET
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // M-eM-^HM-^]M-eM-'M-^KM-eM-^LM-^V ET M-hM-?M-^PM-hM-!M-^LM-fM-^WM-6M-oM-<M-^HBSON M-eM-:M-^OM-eM-^HM-^WM-eM-^LM-^VM-iM-^\\M-^@M-hM-&M-^A CodeTypes + MongoRegisterM-oM-<M-^I
            UBridgeInit.InitRuntime();

            // M-hM-'M-#M-fM-^^M-^PM-eM-^OM-^BM-fM-^UM-0
            string command = args.Length > 0 ? args[0] : "ConsoleGetLogs";
            int count = 50;
            string logType = "all";
            int timeoutMs = 15000;
            int waitMs = 100;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--count" when i + 1 < args.Length: count = int.Parse(args[++i]); break;
                    case "--logType" when i + 1 < args.Length: logType = args[++i]; break;
                    case "--timeout" when i + 1 < args.Length: timeoutMs = int.Parse(args[++i]); break;
                    case "--waitMs" when i + 1 < args.Length: waitMs = int.Parse(args[++i]); break;
                }
            }

            // M-fM-^^M-^DM-iM-^@M- M-hM-/M-7M-fM-1M-^B
            // 构造请求（不依赖 proto 类型，直接用 JSON）
            string payloadJson = $"{{\"_t\":\"ET.ConsoleGetLogsRequest\",\"RpcId\":1,\"Count\":{count},\"LogType\":\"{logType}\"}}";

            UBridgeRequestEnvelope envelope = new UBridgeRequestEnvelope
            {
                RpcId = Guid.NewGuid().ToString("N"),
                Command = command,
                PayloadJson = payloadJson,
                TimeoutMs = timeoutMs
            };

            // M-eM-^HM-^]M-eM-'M-^KM-eM-^LM-^VM-eM--M-^XM-eM-^BM-(M-gM-^[M-.M-eM-=M-^U
            string root = UBridgePathHelper.ResolveRoot();
            UBridgeFileStore.Initialize(root);

            // M-eM-^FM-^YM-hM-/M-7M-fM-1M-^B
            string requestJson = UBridgeJsonHelper.ToJson(envelope);
            UBridgeFileStore.WriteRequest(envelope.RpcId, requestJson);
            Console.Error.WriteLine($"[UBridge] M-eM-7M-2M-eM-^OM-^QM-iM-^@M-^AM-hM-/M-7M-fM-1M-^B: {command} (rpcId={envelope.RpcId})");

            // M-hM-=M-*M-hM-/M-"M-gM--M-^IM-eM-^SM-^MM-eM-:M-^T
            int elapsed = 0;
            while (elapsed < timeoutMs)
            {
                await Task.Delay(waitMs);
                elapsed += waitMs;

                string responseJson = UBridgeFileStore.TryReadResponse(envelope.RpcId);
                if (responseJson != null)
                {
                    UBridgeResponseEnvelope response = UBridgeJsonHelper.FromJson<UBridgeResponseEnvelope>(responseJson);
                    if (response != null && response.Error == UBridgeErrorCode.Success)
                    {
                        Console.WriteLine(response.PayloadJson ?? "");
                        return 0;
                    }
                    else
                    {
                        Console.Error.WriteLine($"[UBridge] M-iM-^TM-^YM-hM-/M-: {response?.Message ?? "M-fM-^\\M-*M-gM-^_M-%M-iM-^TM-^YM-hM-/M-:"} (code={response?.Error})");
                        return response?.Error ?? -1;
                    }
                }
            }

            Console.Error.WriteLine($"[UBridge] M-hM-6M-^EM-fM-^WM-6 ({timeoutMs}ms)");
            return UBridgeErrorCode.Timeout;
        }
    }

    /// <summary>
    /// ET M-hM-?M-^PM-hM-!M-^LM-fM-^WM-6M-eM-^HM-^]M-eM-'M-^KM-eM-^LM-^VM-oM-<M-^HM-gM-2M->M-gM-.M-^@M-gM-^IM-^HM-oM-<M-^ZM-eM-^OM-*M-eM-^HM-^]M-eM-'M-^KM-eM-^LM-^V BSON M-eM-:M-^OM-eM-^HM-^WM-eM-^LM-^VM-fM-^IM-^@M-iM-^\\M-^@M-gM-^ZM-^DM-fM-^\\M-^@M-eM-0M-^OM-hM-?M-^PM-hM-!M-^LM-fM-^WM-6M-oM-<M-^I
    /// </summary>
    internal static class UBridgeInit
    {
        public static void InitRuntime()
        {
            Assembly[] assemblies = { typeof(UBridgeInit).Assembly };
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(assemblies);
            MongoRegister.Init();
        }
    }
}