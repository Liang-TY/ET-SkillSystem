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
            int parentId = 0;
            bool active = true;
            float minX = 0, minY = 0, maxX = 1, maxY = 1;
            float posX = 0, posY = 0;
            float pivotX = 0.5f, pivotY = 0.5f;
            float rotX = 0, rotY = 0, rotZ = 0;
            float scaleX = 1, scaleY = 1, scaleZ = 1;
            float rectWidth = 100, rectHeight = 100;
            int paddingL = 0, paddingR = 0, paddingT = 0, paddingB = 0;
            float spacing = 0;
            float spacingX = 0, spacingY = 0;
            int alignment = 0;
            bool reverse = false;
            bool controlWidth = false, controlHeight = false;
            bool expandWidth = false, expandHeight = false;
            float cellSizeX = 0, cellSizeY = 0;
            int constraint = 0;
            int constraintCount = 0;
            int startCorner = 0;
            int startAxis = 0;
            int hFit = 0, vFit = 0;
            float minW = 0, minH = 0, prefW = 0, prefH = 0, flexW = 0, flexH = 0;
            bool ignoreLayout = false;
            int layoutPriority = 0;
            string sprite = "";
            int imageType = 0;
            float fillAmount = 0;
            int fillMethod = 0;
            bool preserveAspect = false;
            bool raycastTarget = true;
            string text = "";
            int fontSize = 14;
            int fontStyle = 0;
            bool bestFit = false;
            float colorR = 1, colorG = 1, colorB = 1, colorA = 1;
            string controlName = "";
            string paramTypeList = "";
            string targetName = "";
            string triggerType = "Click";

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
                    case "--parentId" when i + 1 < args.Length: parentId = int.Parse(args[++i]); break;
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
                    case "--paddingL" when i + 1 < args.Length: paddingL = int.Parse(args[++i]); break;
                    case "--paddingR" when i + 1 < args.Length: paddingR = int.Parse(args[++i]); break;
                    case "--paddingT" when i + 1 < args.Length: paddingT = int.Parse(args[++i]); break;
                    case "--paddingB" when i + 1 < args.Length: paddingB = int.Parse(args[++i]); break;
                    case "--spacing" when i + 1 < args.Length: spacing = float.Parse(args[++i]); break;
                    case "--spacingX" when i + 1 < args.Length: spacingX = float.Parse(args[++i]); break;
                    case "--spacingY" when i + 1 < args.Length: spacingY = float.Parse(args[++i]); break;
                    case "--alignment" when i + 1 < args.Length: alignment = int.Parse(args[++i]); break;
                    case "--reverse" when i + 1 < args.Length: reverse = bool.Parse(args[++i]); break;
                    case "--controlWidth" when i + 1 < args.Length: controlWidth = bool.Parse(args[++i]); break;
                    case "--controlHeight" when i + 1 < args.Length: controlHeight = bool.Parse(args[++i]); break;
                    case "--expandWidth" when i + 1 < args.Length: expandWidth = bool.Parse(args[++i]); break;
                    case "--expandHeight" when i + 1 < args.Length: expandHeight = bool.Parse(args[++i]); break;
                    case "--cellSizeX" when i + 1 < args.Length: cellSizeX = float.Parse(args[++i]); break;
                    case "--cellSizeY" when i + 1 < args.Length: cellSizeY = float.Parse(args[++i]); break;
                    case "--constraint" when i + 1 < args.Length: constraint = int.Parse(args[++i]); break;
                    case "--constraintCount" when i + 1 < args.Length: constraintCount = int.Parse(args[++i]); break;
                    case "--startCorner" when i + 1 < args.Length: startCorner = int.Parse(args[++i]); break;
                    case "--startAxis" when i + 1 < args.Length: startAxis = int.Parse(args[++i]); break;
                    case "--hFit" when i + 1 < args.Length: hFit = int.Parse(args[++i]); break;
                    case "--vFit" when i + 1 < args.Length: vFit = int.Parse(args[++i]); break;
                    case "--minW" when i + 1 < args.Length: minW = float.Parse(args[++i]); break;
                    case "--minH" when i + 1 < args.Length: minH = float.Parse(args[++i]); break;
                    case "--prefW" when i + 1 < args.Length: prefW = float.Parse(args[++i]); break;
                    case "--prefH" when i + 1 < args.Length: prefH = float.Parse(args[++i]); break;
                    case "--flexW" when i + 1 < args.Length: flexW = float.Parse(args[++i]); break;
                    case "--flexH" when i + 1 < args.Length: flexH = float.Parse(args[++i]); break;
                    case "--ignoreLayout" when i + 1 < args.Length: ignoreLayout = bool.Parse(args[++i]); break;
                    case "--layoutPriority" when i + 1 < args.Length: layoutPriority = int.Parse(args[++i]); break;
                    case "--sprite" when i + 1 < args.Length: sprite = args[++i]; break;
                    case "--imageType" when i + 1 < args.Length: imageType = int.Parse(args[++i]); break;
                    case "--fillAmount" when i + 1 < args.Length: fillAmount = float.Parse(args[++i]); break;
                    case "--fillMethod" when i + 1 < args.Length: fillMethod = int.Parse(args[++i]); break;
                    case "--preserveAspect" when i + 1 < args.Length: preserveAspect = bool.Parse(args[++i]); break;
                    case "--raycastTarget" when i + 1 < args.Length: raycastTarget = bool.Parse(args[++i]); break;
                    case "--text" when i + 1 < args.Length: text = args[++i]; break;
                    case "--fontSize" when i + 1 < args.Length: fontSize = int.Parse(args[++i]); break;
                    case "--fontStyle" when i + 1 < args.Length: fontStyle = int.Parse(args[++i]); break;
                    case "--bestFit" when i + 1 < args.Length: bestFit = bool.Parse(args[++i]); break;
                    case "--colorR" when i + 1 < args.Length: colorR = float.Parse(args[++i]); break;
                    case "--colorG" when i + 1 < args.Length: colorG = float.Parse(args[++i]); break;
                    case "--colorB" when i + 1 < args.Length: colorB = float.Parse(args[++i]); break;
                    case "--colorA" when i + 1 < args.Length: colorA = float.Parse(args[++i]); break;
                    case "--controlName" when i + 1 < args.Length: controlName = args[++i]; break;
                    case "--paramTypes" when i + 1 < args.Length: paramTypeList = args[++i]; break;
                    case "--targetName" when i + 1 < args.Length: targetName = args[++i]; break;
                    case "--triggerType" when i + 1 < args.Length: triggerType = args[++i]; break;
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
                    payloadJson = $"{{\"_t\":\"ET.InspectorAddComponentRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"TypeName\":\"{type}\"}}";
                    break;
                case "InspectorRemoveComponent":
                    payloadJson = $"{{\"_t\":\"ET.InspectorRemoveComponentRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"ComponentName\":\"{type}\"}}";
                    break;
                // GameObject
                case "GameObjectCreate":
                    payloadJson = $"{{\"_t\":\"ET.GameObjectCreateRequest\",\"RpcId\":1,\"Name\":\"{name}\"}}";
                    break;
                case "GameObjectDestroy":
                    payloadJson = $"{{\"_t\":\"ET.GameObjectDestroyRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "GameObjectFind":
                    payloadJson = $"{{\"_t\":\"ET.GameObjectFindRequest\",\"RpcId\":1,\"Name\":\"{name}\",\"MaxResults\":20}}";
                    break;
                // Control
                case "AddControl":
                    payloadJson = $"{{\"_t\":\"ET.AddControlRequest\",\"RpcId\":1,\"ParentId\":{parentId},\"Name\":\"{name}\",\"Type\":\"{type}\"}}";
                    break;
                // YIUI
                case "YIUICreatePanel":
                    payloadJson = $"{{\"_t\":\"ET.YIUICreatePanelRequest\",\"RpcId\":1,\"Path\":\"{path}\",\"Name\":\"{name}\"}}";
                    break;
                // CDE Table
                case "YIUIGetBindings":
                case "YIUIGetEvents":
                    payloadJson = $"{{\"_t\":\"ET.{command}Request\",\"RpcId\":1,\"PrefabPath\":\"{path}\"}}";
                    break;
                case "YIUIBindComponent":
                    payloadJson = $"{{\"_t\":\"ET.YIUIBindComponentRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"ControlName\":\"{controlName}\",\"BindName\":\"{name}\"}}";
                    break;
                case "YIUIBindEvent":
                    payloadJson = $"{{\"_t\":\"ET.YIUIBindEventRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"EventName\":\"{name}\",\"EventType\":\"{type}\",\"ParamTypes\":\"{paramTypeList}\"}}";
                    break;
                case "YIUIAttachEvent":
                    payloadJson = $"{{\"_t\":\"ET.YIUIAttachEventRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"TargetName\":\"{targetName}\",\"EventName\":\"{name}\",\"EventTriggerType\":\"{triggerType}\"}}";
                    break;
                case "YIUIGenerateCode":
                    payloadJson = $"{{\"_t\":\"ET.YIUIGenerateCodeRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"PackageName\":\"{name}\"}}";
                    break;
                case "YIUIClearBindings":
                    payloadJson = $"{{\"_t\":\"ET.YIUIClearBindingsRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"Target\":\"{type}\"}}";
                    break;
                case "YIUIRemoveControl":
                    payloadJson = $"{{\"_t\":\"ET.YIUIRemoveControlRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\",\"ControlName\":\"{name}\"}}";
                    break;
                case "PrefabLoadForEdit":
                    payloadJson = $"{{\"_t\":\"ET.PrefabLoadForEditRequest\",\"RpcId\":1,\"PrefabPath\":\"{path}\"}}";
                    break;
                case "PrefabSaveModified":
                    payloadJson = $"{{\"_t\":\"ET.PrefabSaveModifiedRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"PrefabPath\":\"{path}\"}}";
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
                // LayoutGroup
                case "LayoutGet":
                    payloadJson = $"{{\"_t\":\"ET.LayoutGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "LayoutSet":
                    payloadJson = $"{{\"_t\":\"ET.LayoutSetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"PaddingLeft\":{paddingL},\"PaddingRight\":{paddingR},\"PaddingTop\":{paddingT},\"PaddingBottom\":{paddingB},\"Spacing\":{spacing},\"SpacingX\":{spacingX},\"SpacingY\":{spacingY},\"ChildAlignment\":{alignment},\"ReverseArrangement\":{reverse.ToString().ToLower()},\"ControlChildWidth\":{controlWidth.ToString().ToLower()},\"ControlChildHeight\":{controlHeight.ToString().ToLower()},\"ChildForceExpandWidth\":{expandWidth.ToString().ToLower()},\"ChildForceExpandHeight\":{expandHeight.ToString().ToLower()},\"CellSizeX\":{cellSizeX},\"CellSizeY\":{cellSizeY},\"Constraint\":{constraint},\"ConstraintCount\":{constraintCount},\"StartCorner\":{startCorner},\"StartAxis\":{startAxis}}}";
                    break;
                // ContentSizeFitter
                case "FitterGet":
                    payloadJson = $"{{\"_t\":\"ET.FitterGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "FitterSet":
                    payloadJson = $"{{\"_t\":\"ET.FitterSetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"HorizontalFit\":{hFit},\"VerticalFit\":{vFit}}}";
                    break;
                // LayoutElement
                case "ElementGet":
                    payloadJson = $"{{\"_t\":\"ET.ElementGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "ElementSet":
                    payloadJson = $"{{\"_t\":\"ET.ElementSetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"MinWidth\":{minW},\"MinHeight\":{minH},\"PreferredWidth\":{prefW},\"PreferredHeight\":{prefH},\"FlexibleWidth\":{flexW},\"FlexibleHeight\":{flexH},\"IgnoreLayout\":{ignoreLayout.ToString().ToLower()},\"LayoutPriority\":{layoutPriority}}}";
                    break;
                // Image
                case "ImageGet":
                    payloadJson = $"{{\"_t\":\"ET.ImageGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "ImageSet":
                    payloadJson = $"{{\"_t\":\"ET.ImageSetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"Sprite\":\"{sprite}\",\"ColorR\":{colorR},\"ColorG\":{colorG},\"ColorB\":{colorB},\"ColorA\":{colorA},\"ImageType\":{imageType},\"FillAmount\":{fillAmount},\"FillMethod\":{fillMethod},\"RaycastTarget\":{raycastTarget.ToString().ToLower()},\"PreserveAspect\":{preserveAspect.ToString().ToLower()}}}";
                    break;
                // Text
                case "TextGet":
                    payloadJson = $"{{\"_t\":\"ET.TextGetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId}}}";
                    break;
                case "TextSet":
                    payloadJson = $"{{\"_t\":\"ET.TextSetRequest\",\"RpcId\":1,\"InstanceId\":{instanceId},\"Text\":\"{text}\",\"FontSize\":{fontSize},\"FontStyle\":{fontStyle},\"Alignment\":{alignment},\"ColorR\":{colorR},\"ColorG\":{colorG},\"ColorB\":{colorB},\"ColorA\":{colorA},\"BestFit\":{bestFit.ToString().ToLower()},\"RaycastTarget\":{raycastTarget.ToString().ToLower()}}}";
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