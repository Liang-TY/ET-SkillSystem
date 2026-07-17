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

            // 通用参数
            int timeoutMs = 15000;
            int waitMs = 100;

            // 各命令专用参数
            int count = 50;
            string logType = "all";
            string format = "png";
            int quality = 85;
            bool allowEditMode = false;
            string menuPath = "";
            string name = "";
            string path = "";
            string filter = "";
            string type = "";
            int instanceId = 0;
            bool active = true;
            float minX = 0, minY = 0, maxX = 1, maxY = 1;
            float posX = 0, posY = 0;
            float pivotX = 0.5f, pivotY = 0.5f;
            float rotX = 0, rotY = 0, rotZ = 0;
            float scaleX = 1, scaleY = 1, scaleZ = 1;
            float rectWidth = 100, rectHeight = 100;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--count" when i + 1 < args.Length: count = int.Parse(args[++i]); break;
                    case "--logType" when i + 1 < args.Length: logType = args[++i]; break;
                    case "--format" when i + 1 < args.Length: format = args[++i]; break;
                    case "--quality" when i + 1 < args.Length: quality = int.Parse(args[++i]); break;
                    case "--allowEditMode" when i + 1 < args.Length: allowEditMode = bool.Parse(args[++i]); break;
                    case "--menuPath" when i + 1 < args.Length: menuPath = args[++i]; break;
                    case "--name" when i + 1 < args.Length: name = args[++i]; break;
                    case "--path" when i + 1 < args.Length: path = args[++i]; break;
                    case "--filter" when i + 1 < args.Length: filter = args[++i]; break;
                    case "--type" when i + 1 < args.Length: type = args[++i]; break;
                    case "--instanceId" when i + 1 < args.Length: instanceId = int.Parse(args[++i]); break;
                    case "--active" when i + 1 < args.Length: active = bool.Parse(args[++i]); break;
                    case "--minX" when i + 1 < args.Length: minX = float.Parse(args[++i]); break;
                    case "--minY" when i + 1 < args.Length: minY = float.Parse(args[++i]); break;
                    case "--maxX" when i + 1 < args.Length: maxX = float.Parse(args[++i]); break;
                    case "--maxY" when i + 1 < args.Length: maxY = float.Parse(args[++i]); break;
                    case "--posX" when i + 1 < args.Length: posX = float.Parse(args[++i]); break;
                    case "--posY" when i + 1 < args.Length: posY = float.Parse(args[++i]); break;
                    case "--pivotX" when i + 1 < args.Length: pivotX = float.Parse(args[++i]); break;
                    case "--pivotY" when i + 1 < args.Length: pivotY = float.Parse(args[++i]); break;
                    case "--rotX" when i + 1 < args.Length: rotX = float.Parse(args[++i]); break;
                    case "--rotY" when i + 1 < args.Length: rotY = float.Parse(args[++i]); break;
                    case "--rotZ" when i + 1 < args.Length: rotZ = float.Parse(args[++i]); break;
                    case "--scaleX" when i + 1 < args.Length: scaleX = float.Parse(args[++i]); break;
                    case "--scaleY" when i + 1 < args.Length: scaleY = float.Parse(args[++i]); break;
                    case "--scaleZ" when i + 1 < args.Length: scaleZ = float.Parse(args[++i]); break;
                    case "--rectWidth" when i + 1 < args.Length: rectWidth = float.Parse(args[++i]); break;
                    case "--rectHeight" when i + 1 < args.Length: rectHeight = float.Parse(args[++i]); break;
                    case "--timeout" when i + 1 < args.Length: timeoutMs = int.Parse(args[++i]); break;
                    case "--waitMs" when i + 1 < args.Length: waitMs = int.Parse(args[++i]); break;
                }
            }

            // M-fM-^^M-^DM-iM-^@M- M-hM-/M-7M-fM-1M-^B
            string payloadJson;
            switch (command)
            {
                case "ScreenshotCapture":
                    payloadJson = $"{{\"_t\":\"ET.ScreenshotCaptureRequest\",\"RpcId\":1,\"Target\":\"game\",\"Format\":\"{format}\",\"Quality\":{quality},\"AllowEditMode\":{allowEditMode.ToString().ToLower()}}}";
                    break;
                case "Ping":
                    payloadJson = "{\"_t\":\"ET.Ping\",\"RpcId\":1}";
                    break;
                case "MenuItemExecute":
                    payloadJson = $"{{\"_t\":\"ET.MenuItemExecuteRequest\",\"RpcId\":1,\"MenuPath\":\"{menuPath}\"}}";
                    break;
                // Inspector
                case "InspectorGetComponents":
                    payloadJson = $"{{\"_t\":\"ET.InspectorGetComponentsRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "InspectorGetProperties":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"InstanceId\":{instanceId},\"ComponentName\":\"{type}\",\"IncludeChildren\":true}}";
                    break;
                case "InspectorGetProperty":
                case "InspectorSetProperty":
                case "InspectorSetProperties":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"InstanceId\":{instanceId},\"PropertyName\":\"{name}\",\"ComponentName\":\"{type}\"}}";
                    break;
                case "InspectorFindProperty":
                    payloadJson = $"{{\"_t\":\"ET.InspectorFindPropertyRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"Keyword\":\"{filter}\"}}";
                    break;
                case "InspectorAddComponent":
                case "InspectorRemoveComponent":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"InstanceId\":{instanceId},\"TypeName\":\"{type}\",\"ComponentName\":\"{type}\"}}";
                    break;
                // YIUI
                case "YIUICreatePanel":
                    payloadJson = $"{{\"_t\":\"ET.YIUICreatePanelRequest\",\"RpcId\":1,\"Path\":\"{path}\",\"Name\":\"{name}\"}}";
                    break;
                case "PrefabLoadForEdit":
                case "PrefabSaveModified":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"PrefabPath\":\"{path}\"}}";
                    break;
                // RectTransform
                case "RectGet":
                    payloadJson = $"{{\"_t\":\"ET.RectGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "RectSetAnchor":
                    payloadJson = $"{{\"_t\":\"ET.RectSetAnchorRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"MinX\":{minX},\"MinY\":{minY},\"MaxX\":{maxX},\"MaxY\":{maxY}}}";
                    break;
                case "RectSetSize":
                    payloadJson = $"{{\"_t\":\"ET.RectSetSizeRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"RectWidth\":{rectWidth},\"RectHeight\":{rectHeight}}}";
                    break;
                case "RectSetPos":
                    payloadJson = $"{{\"_t\":\"ET.RectSetPosRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"X\":{posX},\"Y\":{posY}}}";
                    break;
                case "RectSetPivot":
                    payloadJson = $"{{\"_t\":\"ET.RectSetPivotRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"X\":{pivotX},\"Y\":{pivotY}}}";
                    break;
                case "RectSetRotation":
                    payloadJson = $"{{\"_t\":\"ET.RectSetRotationRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"X\":{rotX},\"Y\":{rotY},\"Z\":{rotZ}}}";
                    break;
                case "RectSetScale":
                    payloadJson = $"{{\"_t\":\"ET.RectSetScaleRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"X\":{scaleX},\"Y\":{scaleY},\"Z\":{scaleZ}}}";
                    break;
                // Misc
                case "TestEcho":
                    payloadJson = $"{{\"_t\":\"ET.TestEcho\",\"RpcId\":1,\"Text\":\"{name}\"}}";
                    break;
                case "EditorLog":
                    payloadJson = $"{{\"_t\":\"ET.EditorLogRequest\",\"RpcId\":1,\"Message\":\"{name}\",\"LogType\":\"{type}\"}}";
                    break;
                case "GameViewGetResolution":
                case "GameViewListResolutions":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1}}";
                    break;
                case "GameViewSetResolution":
                    payloadJson = $"{{\"_t\":\"ET.GameViewSetResolutionRequest\",\"RpcId\":1,\"Width\":{count},\"Height\":{count}}}";
                    break;
                // Asset deferred
                case "AssetImport":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"AssetPath\":\"{path}\"}}";
                    break;
                case "AssetRefresh":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"ForceUpdate\":true}}";
                    break;
                // Prefab
                case "PrefabSave":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"GameObjectPath\":\"{name}\",\"SavePath\":\"{path}\"}}";
                    break;
                case "PrefabInstantiate":
                case "PrefabGetHierarchy":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"PrefabPath\":\"{path}\"}}";
                    break;
                case "PrefabGetInfo":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"GameObjectPath\":\"{name}\"}}";
                    break;
                case "PrefabApply":
                case "PrefabUnpack":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"GameObjectPath\":\"{name}\"}}";
                    break;
                default:
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1}}";
                    break;
                    payloadJson = $"{{\"_t\":\"ET.ConsoleGetLogsRequest\",\"RpcId\":1,\"Count\":{count},\"LogType\":\"{logType}\"}}";
                    break;
            }

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