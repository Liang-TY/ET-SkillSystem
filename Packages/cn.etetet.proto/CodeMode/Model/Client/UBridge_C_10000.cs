using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(UBridge.BridgeConsoleLog)]
    public partial class BridgeConsoleLog : MessageObject
    {
        public static BridgeConsoleLog Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeConsoleLog>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string LogType { get; set; }

        [MemoryPackOrder(1)]
        public string Message { get; set; }

        [MemoryPackOrder(2)]
        public string StackTrace { get; set; }

        [MemoryPackOrder(3)]
        public string Time { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.LogType = default;
            this.Message = default;
            this.StackTrace = default;
            this.Time = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ConsoleGetLogsRequest)]
    [ResponseType(nameof(ConsoleGetLogsResponse))]
    public partial class ConsoleGetLogsRequest : MessageObject, IRequest
    {
        public static ConsoleGetLogsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ConsoleGetLogsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Count { get; set; }

        [MemoryPackOrder(91)]
        public string LogType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Count = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ConsoleGetLogsResponse)]
    public partial class ConsoleGetLogsResponse : MessageObject, IResponse
    {
        public static ConsoleGetLogsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ConsoleGetLogsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeConsoleLog> Logs { get; set; } = new();

        [MemoryPackOrder(93)]
        public int Count { get; set; }

        [MemoryPackOrder(94)]
        public int TotalCount { get; set; }

        [MemoryPackOrder(95)]
        public string LogType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Logs.Clear();
            this.Count = default;
            this.TotalCount = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeScreenshotInfo)]
    public partial class BridgeScreenshotInfo : MessageObject
    {
        public static BridgeScreenshotInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeScreenshotInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Path { get; set; }

        [MemoryPackOrder(1)]
        public string FileName { get; set; }

        [MemoryPackOrder(2)]
        public int Width { get; set; }

        [MemoryPackOrder(3)]
        public int Height { get; set; }

        [MemoryPackOrder(4)]
        public long FileSize { get; set; }

        [MemoryPackOrder(5)]
        public string MediaType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Path = default;
            this.FileName = default;
            this.Width = default;
            this.Height = default;
            this.FileSize = default;
            this.MediaType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ScreenshotCaptureRequest)]
    [ResponseType(nameof(ScreenshotCaptureResponse))]
    public partial class ScreenshotCaptureRequest : MessageObject, IRequest
    {
        public static ScreenshotCaptureRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ScreenshotCaptureRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Target { get; set; }

        [MemoryPackOrder(91)]
        public string Format { get; set; }

        [MemoryPackOrder(92)]
        public int Quality { get; set; }

        [MemoryPackOrder(93)]
        public bool AllowEditMode { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Target = default;
            this.Format = default;
            this.Quality = default;
            this.AllowEditMode = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ScreenshotCaptureResponse)]
    public partial class ScreenshotCaptureResponse : MessageObject, IResponse
    {
        public static ScreenshotCaptureResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ScreenshotCaptureResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool Captured { get; set; }

        [MemoryPackOrder(93)]
        public string Target { get; set; }

        [MemoryPackOrder(94)]
        public BridgeScreenshotInfo Screenshot { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Captured = default;
            this.Target = default;
            this.Screenshot = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.Ping)]
    [ResponseType(nameof(PingResponse))]
    public partial class Ping : MessageObject, IRequest
    {
        public static Ping Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Ping>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PingResponse)]
    public partial class PingResponse : MessageObject, IResponse
    {
        public static PingResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PingResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public long Time { get; set; }

        [MemoryPackOrder(93)]
        public bool IsCompiling { get; set; }

        [MemoryPackOrder(94)]
        public bool IsPlaying { get; set; }

        [MemoryPackOrder(95)]
        public bool IsPlayingOrWillChangePlaymode { get; set; }

        [MemoryPackOrder(96)]
        public string CodeMode { get; set; }

        [MemoryPackOrder(97)]
        public string UnityVersion { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Time = default;
            this.IsCompiling = default;
            this.IsPlaying = default;
            this.IsPlayingOrWillChangePlaymode = default;
            this.CodeMode = default;
            this.UnityVersion = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.MenuItemExecuteRequest)]
    [ResponseType(nameof(MenuItemExecuteResponse))]
    public partial class MenuItemExecuteRequest : MessageObject, IRequest
    {
        public static MenuItemExecuteRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MenuItemExecuteRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string MenuPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MenuPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.MenuItemExecuteResponse)]
    public partial class MenuItemExecuteResponse : MessageObject, IResponse
    {
        public static MenuItemExecuteResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MenuItemExecuteResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string MenuPath { get; set; }

        [MemoryPackOrder(93)]
        public bool Executed { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MenuPath = default;
            this.Executed = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeVector2)]
    public partial class BridgeVector2 : MessageObject
    {
        public static BridgeVector2 Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeVector2>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }

        [MemoryPackOrder(1)]
        public float Y { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeVector3)]
    public partial class BridgeVector3 : MessageObject
    {
        public static BridgeVector3 Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeVector3>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }

        [MemoryPackOrder(1)]
        public float Y { get; set; }

        [MemoryPackOrder(2)]
        public float Z { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;
            this.Z = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeQuaternion)]
    public partial class BridgeQuaternion : MessageObject
    {
        public static BridgeQuaternion Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeQuaternion>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }

        [MemoryPackOrder(1)]
        public float Y { get; set; }

        [MemoryPackOrder(2)]
        public float Z { get; set; }

        [MemoryPackOrder(3)]
        public float W { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;
            this.Z = default;
            this.W = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeTransformInfo)]
    public partial class BridgeTransformInfo : MessageObject
    {
        public static BridgeTransformInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeTransformInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public BridgeVector3 Position { get; set; }

        [MemoryPackOrder(1)]
        public BridgeVector3 RotationEuler { get; set; }

        [MemoryPackOrder(2)]
        public BridgeQuaternion Rotation { get; set; }

        [MemoryPackOrder(3)]
        public BridgeVector3 LocalScale { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Position = default;
            this.RotationEuler = default;
            this.Rotation = default;
            this.LocalScale = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeObjectInfo)]
    public partial class BridgeObjectInfo : MessageObject
    {
        public static BridgeObjectInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeObjectInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(1)]
        public string Name { get; set; }

        [MemoryPackOrder(2)]
        public string Tag { get; set; }

        [MemoryPackOrder(3)]
        public int Layer { get; set; }

        [MemoryPackOrder(4)]
        public bool ActiveSelf { get; set; }

        [MemoryPackOrder(5)]
        public bool ActiveInHierarchy { get; set; }

        [MemoryPackOrder(6)]
        public BridgeTransformInfo Transform { get; set; }

        [MemoryPackOrder(7)]
        public string Path { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.InstanceId = default;
            this.Name = default;
            this.Tag = default;
            this.Layer = default;
            this.ActiveSelf = default;
            this.ActiveInHierarchy = default;
            this.Transform = default;
            this.Path = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeComponentInfo)]
    public partial class BridgeComponentInfo : MessageObject
    {
        public static BridgeComponentInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeComponentInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Type { get; set; }

        [MemoryPackOrder(1)]
        public string Data { get; set; }

        [MemoryPackOrder(2)]
        public List<BridgeComponentInfo> Children { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Type = default;
            this.Data = default;
            this.Children.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeAssetInfo)]
    public partial class BridgeAssetInfo : MessageObject
    {
        public static BridgeAssetInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeAssetInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Path { get; set; }

        [MemoryPackOrder(1)]
        public string Guid { get; set; }

        [MemoryPackOrder(2)]
        public string Name { get; set; }

        [MemoryPackOrder(3)]
        public string Type { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Path = default;
            this.Guid = default;
            this.Name = default;
            this.Type = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeSceneNode)]
    public partial class BridgeSceneNode : MessageObject
    {
        public static BridgeSceneNode Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeSceneNode>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public BridgeObjectInfo Object { get; set; }

        [MemoryPackOrder(1)]
        public List<BridgeSceneNode> Children { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Object = default;
            this.Children.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneGetHierarchyRequest)]
    [ResponseType(nameof(SceneGetHierarchyResponse))]
    public partial class SceneGetHierarchyRequest : MessageObject, IRequest
    {
        public static SceneGetHierarchyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetHierarchyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public bool IncludeComponents { get; set; }

        [MemoryPackOrder(91)]
        public int MaxDepth { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.IncludeComponents = default;
            this.MaxDepth = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneGetHierarchyResponse)]
    public partial class SceneGetHierarchyResponse : MessageObject, IResponse
    {
        public static SceneGetHierarchyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetHierarchyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string SceneName { get; set; }

        [MemoryPackOrder(93)]
        public string ScenePath { get; set; }

        [MemoryPackOrder(94)]
        public List<BridgeSceneNode> RootNodes { get; set; } = new();

        [MemoryPackOrder(95)]
        public int NodeCount { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.RootNodes.Clear();
            this.NodeCount = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneGetActiveRequest)]
    [ResponseType(nameof(SceneGetActiveResponse))]
    public partial class SceneGetActiveRequest : MessageObject, IRequest
    {
        public static SceneGetActiveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetActiveRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneGetActiveResponse)]
    public partial class SceneGetActiveResponse : MessageObject, IResponse
    {
        public static SceneGetActiveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetActiveResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string SceneName { get; set; }

        [MemoryPackOrder(93)]
        public string ScenePath { get; set; }

        [MemoryPackOrder(94)]
        public int BuildIndex { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.BuildIndex = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneLoadRequest)]
    [ResponseType(nameof(SceneLoadResponse))]
    public partial class SceneLoadRequest : MessageObject, IRequest
    {
        public static SceneLoadRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneLoadRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string ScenePath { get; set; }

        [MemoryPackOrder(91)]
        public int BuildIndex { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ScenePath = default;
            this.BuildIndex = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneLoadResponse)]
    public partial class SceneLoadResponse : MessageObject, IResponse
    {
        public static SceneLoadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneLoadResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string ScenePath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ScenePath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneSaveRequest)]
    [ResponseType(nameof(SceneSaveResponse))]
    public partial class SceneSaveRequest : MessageObject, IRequest
    {
        public static SceneSaveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneSaveRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string ScenePath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ScenePath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneSaveResponse)]
    public partial class SceneSaveResponse : MessageObject, IResponse
    {
        public static SceneSaveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneSaveResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string ScenePath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ScenePath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneNewRequest)]
    [ResponseType(nameof(SceneNewResponse))]
    public partial class SceneNewRequest : MessageObject, IRequest
    {
        public static SceneNewRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneNewRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string SceneName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.SceneName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SceneNewResponse)]
    public partial class SceneNewResponse : MessageObject, IResponse
    {
        public static SceneNewResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneNewResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string SceneName { get; set; }

        [MemoryPackOrder(93)]
        public string ScenePath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionGetRequest)]
    [ResponseType(nameof(SelectionGetResponse))]
    public partial class SelectionGetRequest : MessageObject, IRequest
    {
        public static SelectionGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionGetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public bool IncludeComponents { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.IncludeComponents = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionGetResponse)]
    public partial class SelectionGetResponse : MessageObject, IResponse
    {
        public static SelectionGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(93)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        [MemoryPackOrder(94)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Objects.Clear();
            this.Assets.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionSetRequest)]
    [ResponseType(nameof(SelectionSetResponse))]
    public partial class SelectionSetRequest : MessageObject, IRequest
    {
        public static SelectionSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public List<int> InstanceIds { get; set; } = new();

        [MemoryPackOrder(91)]
        public List<string> AssetPaths { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceIds.Clear();
            this.AssetPaths.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionSetResponse)]
    public partial class SelectionSetResponse : MessageObject, IResponse
    {
        public static SelectionSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionSetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionAddRequest)]
    [ResponseType(nameof(SelectionAddResponse))]
    public partial class SelectionAddRequest : MessageObject, IRequest
    {
        public static SelectionAddRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionAddRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public List<int> InstanceIds { get; set; } = new();

        [MemoryPackOrder(91)]
        public List<string> AssetPaths { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceIds.Clear();
            this.AssetPaths.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionAddResponse)]
    public partial class SelectionAddResponse : MessageObject, IResponse
    {
        public static SelectionAddResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionAddResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionRemoveRequest)]
    [ResponseType(nameof(SelectionRemoveResponse))]
    public partial class SelectionRemoveRequest : MessageObject, IRequest
    {
        public static SelectionRemoveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionRemoveRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public List<int> InstanceIds { get; set; } = new();

        [MemoryPackOrder(91)]
        public List<string> AssetPaths { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceIds.Clear();
            this.AssetPaths.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionRemoveResponse)]
    public partial class SelectionRemoveResponse : MessageObject, IResponse
    {
        public static SelectionRemoveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionRemoveResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionClearRequest)]
    [ResponseType(nameof(SelectionClearResponse))]
    public partial class SelectionClearRequest : MessageObject, IRequest
    {
        public static SelectionClearRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionClearRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.SelectionClearResponse)]
    public partial class SelectionClearResponse : MessageObject, IResponse
    {
        public static SelectionClearResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionClearResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetSearchRequest)]
    [ResponseType(nameof(AssetSearchResponse))]
    public partial class AssetSearchRequest : MessageObject, IRequest
    {
        public static AssetSearchRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetSearchRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Filter { get; set; }

        [MemoryPackOrder(91)]
        public string Type { get; set; }

        [MemoryPackOrder(92)]
        public int MaxResults { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Filter = default;
            this.Type = default;
            this.MaxResults = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetSearchResponse)]
    public partial class AssetSearchResponse : MessageObject, IResponse
    {
        public static AssetSearchResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetSearchResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        [MemoryPackOrder(93)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Assets.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetFindRequest)]
    [ResponseType(nameof(AssetFindResponse))]
    public partial class AssetFindRequest : MessageObject, IRequest
    {
        public static AssetFindRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetFindRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(91)]
        public string Guid { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;
            this.Guid = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetFindResponse)]
    public partial class AssetFindResponse : MessageObject, IResponse
    {
        public static AssetFindResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetFindResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeAssetInfo Asset { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetGetPathRequest)]
    [ResponseType(nameof(AssetGetPathResponse))]
    public partial class AssetGetPathRequest : MessageObject, IRequest
    {
        public static AssetGetPathRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetGetPathRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Guid { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Guid = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetGetPathResponse)]
    public partial class AssetGetPathResponse : MessageObject, IResponse
    {
        public static AssetGetPathResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetGetPathResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public BridgeAssetInfo Asset { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetLoadRequest)]
    [ResponseType(nameof(AssetLoadResponse))]
    public partial class AssetLoadRequest : MessageObject, IRequest
    {
        public static AssetLoadRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetLoadRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string AssetPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetLoadResponse)]
    public partial class AssetLoadResponse : MessageObject, IResponse
    {
        public static AssetLoadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetLoadResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public BridgeAssetInfo Asset { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetReadTextRequest)]
    [ResponseType(nameof(AssetReadTextResponse))]
    public partial class AssetReadTextRequest : MessageObject, IRequest
    {
        public static AssetReadTextRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetReadTextRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(91)]
        public int StartLine { get; set; }

        [MemoryPackOrder(92)]
        public int MaxLines { get; set; }

        [MemoryPackOrder(93)]
        public int MaxChars { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;
            this.StartLine = default;
            this.MaxLines = default;
            this.MaxChars = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetReadTextResponse)]
    public partial class AssetReadTextResponse : MessageObject, IResponse
    {
        public static AssetReadTextResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetReadTextResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public int TotalLines { get; set; }

        [MemoryPackOrder(94)]
        public int ReturnedLineStart { get; set; }

        [MemoryPackOrder(95)]
        public int ReturnedLineEnd { get; set; }

        [MemoryPackOrder(96)]
        public int ReturnedLineCount { get; set; }

        [MemoryPackOrder(97)]
        public bool Truncated { get; set; }

        [MemoryPackOrder(98)]
        public string Content { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;
            this.TotalLines = default;
            this.ReturnedLineStart = default;
            this.ReturnedLineEnd = default;
            this.ReturnedLineCount = default;
            this.Truncated = default;
            this.Content = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectCreateRequest)]
    [ResponseType(nameof(GameObjectCreateResponse))]
    public partial class GameObjectCreateRequest : MessageObject, IRequest
    {
        public static GameObjectCreateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectCreateRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Name { get; set; }

        [MemoryPackOrder(91)]
        public string Tag { get; set; }

        [MemoryPackOrder(92)]
        public int Layer { get; set; }

        [MemoryPackOrder(93)]
        public BridgeVector3 Position { get; set; }

        [MemoryPackOrder(94)]
        public BridgeQuaternion Rotation { get; set; }

        [MemoryPackOrder(95)]
        public BridgeVector3 Scale { get; set; }

        [MemoryPackOrder(96)]
        public string ParentPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Name = default;
            this.Tag = default;
            this.Layer = default;
            this.Position = default;
            this.Rotation = default;
            this.Scale = default;
            this.ParentPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectCreateResponse)]
    public partial class GameObjectCreateResponse : MessageObject, IResponse
    {
        public static GameObjectCreateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectCreateResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectDestroyRequest)]
    [ResponseType(nameof(GameObjectDestroyResponse))]
    public partial class GameObjectDestroyRequest : MessageObject, IRequest
    {
        public static GameObjectDestroyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDestroyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string Path { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Path = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectDestroyResponse)]
    public partial class GameObjectDestroyResponse : MessageObject, IResponse
    {
        public static GameObjectDestroyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDestroyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool Destroyed { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Destroyed = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectFindRequest)]
    [ResponseType(nameof(GameObjectFindResponse))]
    public partial class GameObjectFindRequest : MessageObject, IRequest
    {
        public static GameObjectFindRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectFindRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Name { get; set; }

        [MemoryPackOrder(91)]
        public string Tag { get; set; }

        [MemoryPackOrder(92)]
        public string ComponentType { get; set; }

        [MemoryPackOrder(93)]
        public int MaxResults { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Name = default;
            this.Tag = default;
            this.ComponentType = default;
            this.MaxResults = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectFindResponse)]
    public partial class GameObjectFindResponse : MessageObject, IResponse
    {
        public static GameObjectFindResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectFindResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(93)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Objects.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectGetInfoRequest)]
    [ResponseType(nameof(GameObjectGetInfoResponse))]
    public partial class GameObjectGetInfoRequest : MessageObject, IRequest
    {
        public static GameObjectGetInfoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectGetInfoRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string Path { get; set; }

        [MemoryPackOrder(92)]
        public bool IncludeComponents { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Path = default;
            this.IncludeComponents = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectGetInfoResponse)]
    public partial class GameObjectGetInfoResponse : MessageObject, IResponse
    {
        public static GameObjectGetInfoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectGetInfoResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectRenameRequest)]
    [ResponseType(nameof(GameObjectRenameResponse))]
    public partial class GameObjectRenameRequest : MessageObject, IRequest
    {
        public static GameObjectRenameRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectRenameRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string NewName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.NewName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectRenameResponse)]
    public partial class GameObjectRenameResponse : MessageObject, IResponse
    {
        public static GameObjectRenameResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectRenameResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectDuplicateRequest)]
    [ResponseType(nameof(GameObjectDuplicateResponse))]
    public partial class GameObjectDuplicateRequest : MessageObject, IRequest
    {
        public static GameObjectDuplicateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDuplicateRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string NewName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.NewName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectDuplicateResponse)]
    public partial class GameObjectDuplicateResponse : MessageObject, IResponse
    {
        public static GameObjectDuplicateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDuplicateResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectSetActiveRequest)]
    [ResponseType(nameof(GameObjectSetActiveResponse))]
    public partial class GameObjectSetActiveRequest : MessageObject, IRequest
    {
        public static GameObjectSetActiveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectSetActiveRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public bool Active { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Active = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.GameObjectSetActiveResponse)]
    public partial class GameObjectSetActiveResponse : MessageObject, IResponse
    {
        public static GameObjectSetActiveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectSetActiveResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformGetRequest)]
    [ResponseType(nameof(TransformGetResponse))]
    public partial class TransformGetRequest : MessageObject, IRequest
    {
        public static TransformGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformGetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string Path { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Path = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformGetResponse)]
    public partial class TransformGetResponse : MessageObject, IResponse
    {
        public static TransformGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetPositionRequest)]
    [ResponseType(nameof(TransformSetPositionResponse))]
    public partial class TransformSetPositionRequest : MessageObject, IRequest
    {
        public static TransformSetPositionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetPositionRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public BridgeVector3 Position { get; set; }

        [MemoryPackOrder(92)]
        public bool Local { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Position = default;
            this.Local = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetPositionResponse)]
    public partial class TransformSetPositionResponse : MessageObject, IResponse
    {
        public static TransformSetPositionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetPositionResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetRotationRequest)]
    [ResponseType(nameof(TransformSetRotationResponse))]
    public partial class TransformSetRotationRequest : MessageObject, IRequest
    {
        public static TransformSetRotationRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetRotationRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public BridgeQuaternion Rotation { get; set; }

        [MemoryPackOrder(92)]
        public bool Local { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Rotation = default;
            this.Local = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetRotationResponse)]
    public partial class TransformSetRotationResponse : MessageObject, IResponse
    {
        public static TransformSetRotationResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetRotationResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetScaleRequest)]
    [ResponseType(nameof(TransformSetScaleResponse))]
    public partial class TransformSetScaleRequest : MessageObject, IRequest
    {
        public static TransformSetScaleRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetScaleRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public BridgeVector3 Scale { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Scale = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetScaleResponse)]
    public partial class TransformSetScaleResponse : MessageObject, IResponse
    {
        public static TransformSetScaleResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetScaleResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetParentRequest)]
    [ResponseType(nameof(TransformSetParentResponse))]
    public partial class TransformSetParentRequest : MessageObject, IRequest
    {
        public static TransformSetParentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetParentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public int ParentInstanceId { get; set; }

        [MemoryPackOrder(92)]
        public bool WorldPositionStays { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.ParentInstanceId = default;
            this.WorldPositionStays = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetParentResponse)]
    public partial class TransformSetParentResponse : MessageObject, IResponse
    {
        public static TransformSetParentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetParentResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetSiblingIndexRequest)]
    [ResponseType(nameof(TransformSetSiblingIndexResponse))]
    public partial class TransformSetSiblingIndexRequest : MessageObject, IRequest
    {
        public static TransformSetSiblingIndexRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetSiblingIndexRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public int SiblingIndex { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.SiblingIndex = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformSetSiblingIndexResponse)]
    public partial class TransformSetSiblingIndexResponse : MessageObject, IResponse
    {
        public static TransformSetSiblingIndexResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetSiblingIndexResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformLookAtRequest)]
    [ResponseType(nameof(TransformLookAtResponse))]
    public partial class TransformLookAtRequest : MessageObject, IRequest
    {
        public static TransformLookAtRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformLookAtRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public BridgeVector3 Target { get; set; }

        [MemoryPackOrder(92)]
        public BridgeVector3 WorldUp { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Target = default;
            this.WorldUp = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformLookAtResponse)]
    public partial class TransformLookAtResponse : MessageObject, IResponse
    {
        public static TransformLookAtResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformLookAtResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformResetRequest)]
    [ResponseType(nameof(TransformResetResponse))]
    public partial class TransformResetRequest : MessageObject, IRequest
    {
        public static TransformResetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformResetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.TransformResetResponse)]
    public partial class TransformResetResponse : MessageObject, IResponse
    {
        public static TransformResetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformResetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeTransformInfo Transform { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Transform = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== Prefab ====================
    [MemoryPackable]
    [Message(UBridge.PrefabInstantiateRequest)]
    [ResponseType(nameof(PrefabInstantiateResponse))]
    public partial class PrefabInstantiateRequest : MessageObject, IRequest
    {
        public static PrefabInstantiateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabInstantiateRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public BridgeVector3 Position { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.Position = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabInstantiateResponse)]
    public partial class PrefabInstantiateResponse : MessageObject, IResponse
    {
        public static PrefabInstantiateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabInstantiateResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(93)]
        public BridgeObjectInfo Instance { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PrefabPath = default;
            this.Instance = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabSaveRequest)]
    [ResponseType(nameof(PrefabSaveResponse))]
    public partial class PrefabSaveRequest : MessageObject, IRequest
    {
        public static PrefabSaveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string GameObjectPath { get; set; }

        [MemoryPackOrder(91)]
        public string SavePath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.GameObjectPath = default;
            this.SavePath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabSaveResponse)]
    public partial class PrefabSaveResponse : MessageObject, IResponse
    {
        public static PrefabSaveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(94)]
        public bool Saved { get; set; }

        [MemoryPackOrder(95)]
        public BridgeAssetInfo Asset { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.PrefabPath = default;
            this.Saved = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabApplyRequest)]
    [ResponseType(nameof(PrefabApplyResponse))]
    public partial class PrefabApplyRequest : MessageObject, IRequest
    {
        public static PrefabApplyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabApplyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string GameObjectPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.GameObjectPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabApplyResponse)]
    public partial class PrefabApplyResponse : MessageObject, IResponse
    {
        public static PrefabApplyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabApplyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(94)]
        public bool Applied { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.PrefabPath = default;
            this.Applied = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabUnpackRequest)]
    [ResponseType(nameof(PrefabUnpackResponse))]
    public partial class PrefabUnpackRequest : MessageObject, IRequest
    {
        public static PrefabUnpackRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabUnpackRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string GameObjectPath { get; set; }

        [MemoryPackOrder(91)]
        public bool Completely { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.GameObjectPath = default;
            this.Completely = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabUnpackResponse)]
    public partial class PrefabUnpackResponse : MessageObject, IResponse
    {
        public static PrefabUnpackResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabUnpackResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public bool Unpacked { get; set; }

        [MemoryPackOrder(94)]
        public bool Completely { get; set; }

        [MemoryPackOrder(95)]
        public BridgeObjectInfo Object { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.Unpacked = default;
            this.Completely = default;
            this.Object = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabGetInfoRequest)]
    [ResponseType(nameof(PrefabGetInfoResponse))]
    public partial class PrefabGetInfoRequest : MessageObject, IRequest
    {
        public static PrefabGetInfoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetInfoRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string GameObjectPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.GameObjectPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabGetInfoResponse)]
    public partial class PrefabGetInfoResponse : MessageObject, IResponse
    {
        public static PrefabGetInfoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetInfoResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string Name { get; set; }

        [MemoryPackOrder(93)]
        public bool IsPrefabAsset { get; set; }

        [MemoryPackOrder(94)]
        public bool IsPrefabInstance { get; set; }

        [MemoryPackOrder(95)]
        public string PrefabAssetPath { get; set; }

        [MemoryPackOrder(96)]
        public string PrefabType { get; set; }

        [MemoryPackOrder(97)]
        public string PrefabStatus { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Name = default;
            this.IsPrefabAsset = default;
            this.IsPrefabInstance = default;
            this.PrefabAssetPath = default;
            this.PrefabType = default;
            this.PrefabStatus = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabGetHierarchyRequest)]
    [ResponseType(nameof(PrefabGetHierarchyResponse))]
    public partial class PrefabGetHierarchyRequest : MessageObject, IRequest
    {
        public static PrefabGetHierarchyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetHierarchyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public int Depth { get; set; }

        [MemoryPackOrder(92)]
        public bool IncludeInactive { get; set; }

        [MemoryPackOrder(93)]
        public bool IncludeComponents { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.Depth = default;
            this.IncludeInactive = default;
            this.IncludeComponents = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.PrefabGetHierarchyResponse)]
    public partial class PrefabGetHierarchyResponse : MessageObject, IResponse
    {
        public static PrefabGetHierarchyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetHierarchyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(93)]
        public string PrefabName { get; set; }

        [MemoryPackOrder(94)]
        public int RootCount { get; set; }

        [MemoryPackOrder(95)]
        public bool Truncated { get; set; }

        [MemoryPackOrder(96)]
        public List<BridgeSceneNode> Roots { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PrefabPath = default;
            this.PrefabName = default;
            this.RootCount = default;
            this.Truncated = default;
            this.Roots.Clear();

            ObjectPool.Recycle(this);
        }
    }

    // ==================== Inspector ====================
    [MemoryPackable]
    [Message(UBridge.BridgePropertyInfo)]
    public partial class BridgePropertyInfo : MessageObject
    {
        public static BridgePropertyInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgePropertyInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]
        public string DisplayName { get; set; }

        [MemoryPackOrder(2)]
        public string Type { get; set; }

        [MemoryPackOrder(3)]
        public string StringValue { get; set; }

        [MemoryPackOrder(4)]
        public int IntValue { get; set; }

        [MemoryPackOrder(5)]
        public float FloatValue { get; set; }

        [MemoryPackOrder(6)]
        public bool BoolValue { get; set; }

        [MemoryPackOrder(7)]
        public BridgeVector2 Vector2Value { get; set; }

        [MemoryPackOrder(8)]
        public BridgeVector3 Vector3Value { get; set; }

        [MemoryPackOrder(9)]
        public string ObjectReferencePath { get; set; }

        [MemoryPackOrder(10)]
        public string ObjectReferenceType { get; set; }

        [MemoryPackOrder(11)]
        public bool IsArray { get; set; }

        [MemoryPackOrder(12)]
        public bool IsEditable { get; set; }

        [MemoryPackOrder(13)]
        public string PropertyPath { get; set; }

        [MemoryPackOrder(14)]
        public bool IsExpanded { get; set; }

        [MemoryPackOrder(15)]
        public bool HasChildren { get; set; }

        [MemoryPackOrder(16)]
        public int Depth { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.DisplayName = default;
            this.Type = default;
            this.StringValue = default;
            this.IntValue = default;
            this.FloatValue = default;
            this.BoolValue = default;
            this.Vector2Value = default;
            this.Vector3Value = default;
            this.ObjectReferencePath = default;
            this.ObjectReferenceType = default;
            this.IsArray = default;
            this.IsEditable = default;
            this.PropertyPath = default;
            this.IsExpanded = default;
            this.HasChildren = default;
            this.Depth = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetComponentsRequest)]
    [ResponseType(nameof(InspectorGetComponentsResponse))]
    public partial class InspectorGetComponentsRequest : MessageObject, IRequest
    {
        public static InspectorGetComponentsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetComponentsResponse)]
    public partial class InspectorGetComponentsResponse : MessageObject, IResponse
    {
        public static InspectorGetComponentsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public List<BridgeComponentInfo> Components { get; set; } = new();

        [MemoryPackOrder(96)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Components.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetPropertiesRequest)]
    [ResponseType(nameof(InspectorGetPropertiesResponse))]
    public partial class InspectorGetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public bool IncludeChildren { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.IncludeChildren = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetPropertiesResponse)]
    public partial class InspectorGetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetPropertyRequest)]
    [ResponseType(nameof(InspectorGetPropertyResponse))]
    public partial class InspectorGetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string PropertyName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.PropertyName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorGetPropertyResponse)]
    public partial class InspectorGetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(96)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(97)]
        public BridgePropertyInfo Property { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Property = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorFindPropertyRequest)]
    [ResponseType(nameof(InspectorFindPropertyResponse))]
    public partial class InspectorFindPropertyRequest : MessageObject, IRequest
    {
        public static InspectorFindPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string Keyword { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.Keyword = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorFindPropertyResponse)]
    public partial class InspectorFindPropertyResponse : MessageObject, IResponse
    {
        public static InspectorFindPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public string Keyword { get; set; }

        [MemoryPackOrder(96)]
        public int Count { get; set; }

        [MemoryPackOrder(97)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(98)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.ComponentName = default;
            this.Keyword = default;
            this.Count = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorSetPropertyRequest)]
    [ResponseType(nameof(InspectorSetPropertyResponse))]
    public partial class InspectorSetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string PropertyName { get; set; }

        [MemoryPackOrder(98)]
        public BridgePropertyInfo Value { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.PropertyName = default;
            this.Value = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorSetPropertyResponse)]
    public partial class InspectorSetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public bool Changed { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Changed = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorSetPropertiesRequest)]
    [ResponseType(nameof(InspectorSetPropertiesResponse))]
    public partial class InspectorSetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public List<BridgePropertyInfo> Values { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.Values.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorSetPropertiesResponse)]
    public partial class InspectorSetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public bool Changed { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Changed = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorAddComponentRequest)]
    [ResponseType(nameof(InspectorAddComponentResponse))]
    public partial class InspectorAddComponentRequest : MessageObject, IRequest
    {
        public static InspectorAddComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string TypeName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.TypeName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorAddComponentResponse)]
    public partial class InspectorAddComponentResponse : MessageObject, IResponse
    {
        public static InspectorAddComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public BridgeComponentInfo AddedComponent { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.AddedComponent = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorRemoveComponentRequest)]
    [ResponseType(nameof(InspectorRemoveComponentResponse))]
    public partial class InspectorRemoveComponentRequest : MessageObject, IRequest
    {
        public static InspectorRemoveComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.InspectorRemoveComponentResponse)]
    public partial class InspectorRemoveComponentResponse : MessageObject, IResponse
    {
        public static InspectorRemoveComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public BridgeComponentInfo RemovedComponent { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.RemovedComponent = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== Editor 控制 ====================
    [MemoryPackable]
    [Message(UBridge.Reload)]
    [ResponseType(nameof(ReloadResponse))]
    public partial class Reload : MessageObject, IRequest
    {
        public static Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Reload>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ReloadResponse)]
    public partial class ReloadResponse : MessageObject, IResponse
    {
        public static ReloadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ReloadResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool Reloaded { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Reloaded = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorUndoRequest)]
    [ResponseType(nameof(EditorUndoResponse))]
    public partial class EditorUndoRequest : MessageObject, IRequest
    {
        public static EditorUndoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorUndoRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorUndoResponse)]
    public partial class EditorUndoResponse : MessageObject, IResponse
    {
        public static EditorUndoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorUndoResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorRedoRequest)]
    [ResponseType(nameof(EditorRedoResponse))]
    public partial class EditorRedoRequest : MessageObject, IRequest
    {
        public static EditorRedoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorRedoRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorRedoResponse)]
    public partial class EditorRedoResponse : MessageObject, IResponse
    {
        public static EditorRedoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorRedoResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int Count { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorPauseRequest)]
    [ResponseType(nameof(EditorPauseResponse))]
    public partial class EditorPauseRequest : MessageObject, IRequest
    {
        public static EditorPauseRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorPauseRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public bool Toggle { get; set; }

        [MemoryPackOrder(91)]
        public bool Pause { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Toggle = default;
            this.Pause = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorPauseResponse)]
    public partial class EditorPauseResponse : MessageObject, IResponse
    {
        public static EditorPauseResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorPauseResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool IsPaused { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPaused = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorGetStateRequest)]
    [ResponseType(nameof(EditorGetStateResponse))]
    public partial class EditorGetStateRequest : MessageObject, IRequest
    {
        public static EditorGetStateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorGetStateRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EditorGetStateResponse)]
    public partial class EditorGetStateResponse : MessageObject, IResponse
    {
        public static EditorGetStateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorGetStateResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool IsPlaying { get; set; }

        [MemoryPackOrder(93)]
        public bool IsPaused { get; set; }

        [MemoryPackOrder(94)]
        public bool IsCompiling { get; set; }

        [MemoryPackOrder(95)]
        public bool IsUpdating { get; set; }

        [MemoryPackOrder(96)]
        public string ApplicationPath { get; set; }

        [MemoryPackOrder(97)]
        public string ApplicationContentsPath { get; set; }

        [MemoryPackOrder(98)]
        public bool EnterPlayModeOptionsEnabled { get; set; }

        [MemoryPackOrder(99)]
        public string EnterPlayModeOptions { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPlaying = default;
            this.IsPaused = default;
            this.IsCompiling = default;
            this.IsUpdating = default;
            this.ApplicationPath = default;
            this.ApplicationContentsPath = default;
            this.EnterPlayModeOptionsEnabled = default;
            this.EnterPlayModeOptions = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== 延迟命令（生命周期） ====================
    [MemoryPackable]
    [Message(UBridge.Compile)]
    [ResponseType(nameof(CompileResponse))]
    public partial class Compile : MessageObject, IRequest
    {
        public static Compile Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Compile>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.CompileResponse)]
    public partial class CompileResponse : MessageObject, IResponse
    {
        public static CompileResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<CompileResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public long DurationMs { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.DurationMs = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.Refresh)]
    [ResponseType(nameof(RefreshResponse))]
    public partial class Refresh : MessageObject, IRequest
    {
        public static Refresh Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Refresh>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public bool ForceUpdate { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ForceUpdate = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.RefreshResponse)]
    public partial class RefreshResponse : MessageObject, IResponse
    {
        public static RefreshResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RefreshResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.RegenProject)]
    [ResponseType(nameof(RegenProjectResponse))]
    public partial class RegenProject : MessageObject, IRequest
    {
        public static RegenProject Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RegenProject>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.RegenProjectResponse)]
    public partial class RegenProjectResponse : MessageObject, IResponse
    {
        public static RegenProjectResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RegenProjectResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EnterPlay)]
    [ResponseType(nameof(EnterPlayResponse))]
    public partial class EnterPlay : MessageObject, IRequest
    {
        public static EnterPlay Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EnterPlay>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.EnterPlayResponse)]
    public partial class EnterPlayResponse : MessageObject, IResponse
    {
        public static EnterPlayResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EnterPlayResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool IsPlaying { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPlaying = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ExitPlay)]
    [ResponseType(nameof(ExitPlayResponse))]
    public partial class ExitPlay : MessageObject, IRequest
    {
        public static ExitPlay Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ExitPlay>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.ExitPlayResponse)]
    public partial class ExitPlayResponse : MessageObject, IResponse
    {
        public static ExitPlayResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ExitPlayResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool IsPlaying { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPlaying = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== 系统 ====================
    [MemoryPackable]
    [Message(UBridge.HostState)]
    [ResponseType(nameof(HostStateResponse))]
    public partial class HostState : MessageObject, IRequest
    {
        public static HostState Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<HostState>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.HostStateResponse)]
    public partial class HostStateResponse : MessageObject, IResponse
    {
        public static HostStateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<HostStateResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool IsCompiling { get; set; }

        [MemoryPackOrder(93)]
        public bool IsPlaying { get; set; }

        [MemoryPackOrder(94)]
        public bool IsPlayingOrWillChangePlaymode { get; set; }

        [MemoryPackOrder(95)]
        public string CodeMode { get; set; }

        [MemoryPackOrder(96)]
        public string UnityVersion { get; set; }

        [MemoryPackOrder(97)]
        public string AvailableCommands { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsCompiling = default;
            this.IsPlaying = default;
            this.IsPlayingOrWillChangePlaymode = default;
            this.CodeMode = default;
            this.UnityVersion = default;
            this.AvailableCommands = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BridgeBatchStepResult)]
    public partial class BridgeBatchStepResult : MessageObject
    {
        public static BridgeBatchStepResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeBatchStepResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]
        public string Command { get; set; }

        [MemoryPackOrder(2)]
        public int Error { get; set; }

        [MemoryPackOrder(3)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.Command = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BatchExecuteRequest)]
    [ResponseType(nameof(BatchExecuteResponse))]
    public partial class BatchExecuteRequest : MessageObject, IRequest
    {
        public static BatchExecuteRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BatchExecuteRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public List<string> Commands { get; set; } = new();

        [MemoryPackOrder(91)]
        public bool StopOnError { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Commands.Clear();
            this.StopOnError = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.BatchExecuteResponse)]
    public partial class BatchExecuteResponse : MessageObject, IResponse
    {
        public static BatchExecuteResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BatchExecuteResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeBatchStepResult> Results { get; set; } = new();

        [MemoryPackOrder(93)]
        public int Count { get; set; }

        [MemoryPackOrder(94)]
        public int Failed { get; set; }

        [MemoryPackOrder(95)]
        public bool Completed { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Results.Clear();
            this.Count = default;
            this.Failed = default;
            this.Completed = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== Asset 延迟命令 ====================
    [MemoryPackable]
    [Message(UBridge.AssetImportRequest)]
    [ResponseType(nameof(AssetImportResponse))]
    public partial class AssetImportRequest : MessageObject, IRequest
    {
        public static AssetImportRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetImportRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string AssetPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetImportResponse)]
    public partial class AssetImportResponse : MessageObject, IResponse
    {
        public static AssetImportResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetImportResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetRefreshRequest)]
    [ResponseType(nameof(AssetRefreshResponse))]
    public partial class AssetRefreshRequest : MessageObject, IRequest
    {
        public static AssetRefreshRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetRefreshRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public bool ForceUpdate { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ForceUpdate = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridge.AssetRefreshResponse)]
    public partial class AssetRefreshResponse : MessageObject, IResponse
    {
        public static AssetRefreshResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetRefreshResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool Refreshed { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Refreshed = default;

            ObjectPool.Recycle(this);
        }
    }

    public static class UBridge
    {
        public const ushort BridgeConsoleLog = 10001;
        public const ushort ConsoleGetLogsRequest = 10002;
        public const ushort ConsoleGetLogsResponse = 10003;
        public const ushort BridgeScreenshotInfo = 10004;
        public const ushort ScreenshotCaptureRequest = 10005;
        public const ushort ScreenshotCaptureResponse = 10006;
        public const ushort Ping = 10007;
        public const ushort PingResponse = 10008;
        public const ushort MenuItemExecuteRequest = 10009;
        public const ushort MenuItemExecuteResponse = 10010;
        public const ushort BridgeVector2 = 10011;
        public const ushort BridgeVector3 = 10012;
        public const ushort BridgeQuaternion = 10013;
        public const ushort BridgeTransformInfo = 10014;
        public const ushort BridgeObjectInfo = 10015;
        public const ushort BridgeComponentInfo = 10016;
        public const ushort BridgeAssetInfo = 10017;
        public const ushort BridgeSceneNode = 10018;
        public const ushort SceneGetHierarchyRequest = 10019;
        public const ushort SceneGetHierarchyResponse = 10020;
        public const ushort SceneGetActiveRequest = 10021;
        public const ushort SceneGetActiveResponse = 10022;
        public const ushort SceneLoadRequest = 10023;
        public const ushort SceneLoadResponse = 10024;
        public const ushort SceneSaveRequest = 10025;
        public const ushort SceneSaveResponse = 10026;
        public const ushort SceneNewRequest = 10027;
        public const ushort SceneNewResponse = 10028;
        public const ushort SelectionGetRequest = 10029;
        public const ushort SelectionGetResponse = 10030;
        public const ushort SelectionSetRequest = 10031;
        public const ushort SelectionSetResponse = 10032;
        public const ushort SelectionAddRequest = 10033;
        public const ushort SelectionAddResponse = 10034;
        public const ushort SelectionRemoveRequest = 10035;
        public const ushort SelectionRemoveResponse = 10036;
        public const ushort SelectionClearRequest = 10037;
        public const ushort SelectionClearResponse = 10038;
        public const ushort AssetSearchRequest = 10039;
        public const ushort AssetSearchResponse = 10040;
        public const ushort AssetFindRequest = 10041;
        public const ushort AssetFindResponse = 10042;
        public const ushort AssetGetPathRequest = 10043;
        public const ushort AssetGetPathResponse = 10044;
        public const ushort AssetLoadRequest = 10045;
        public const ushort AssetLoadResponse = 10046;
        public const ushort AssetReadTextRequest = 10047;
        public const ushort AssetReadTextResponse = 10048;
        public const ushort GameObjectCreateRequest = 10049;
        public const ushort GameObjectCreateResponse = 10050;
        public const ushort GameObjectDestroyRequest = 10051;
        public const ushort GameObjectDestroyResponse = 10052;
        public const ushort GameObjectFindRequest = 10053;
        public const ushort GameObjectFindResponse = 10054;
        public const ushort GameObjectGetInfoRequest = 10055;
        public const ushort GameObjectGetInfoResponse = 10056;
        public const ushort GameObjectRenameRequest = 10057;
        public const ushort GameObjectRenameResponse = 10058;
        public const ushort GameObjectDuplicateRequest = 10059;
        public const ushort GameObjectDuplicateResponse = 10060;
        public const ushort GameObjectSetActiveRequest = 10061;
        public const ushort GameObjectSetActiveResponse = 10062;
        public const ushort TransformGetRequest = 10063;
        public const ushort TransformGetResponse = 10064;
        public const ushort TransformSetPositionRequest = 10065;
        public const ushort TransformSetPositionResponse = 10066;
        public const ushort TransformSetRotationRequest = 10067;
        public const ushort TransformSetRotationResponse = 10068;
        public const ushort TransformSetScaleRequest = 10069;
        public const ushort TransformSetScaleResponse = 10070;
        public const ushort TransformSetParentRequest = 10071;
        public const ushort TransformSetParentResponse = 10072;
        public const ushort TransformSetSiblingIndexRequest = 10073;
        public const ushort TransformSetSiblingIndexResponse = 10074;
        public const ushort TransformLookAtRequest = 10075;
        public const ushort TransformLookAtResponse = 10076;
        public const ushort TransformResetRequest = 10077;
        public const ushort TransformResetResponse = 10078;
        public const ushort PrefabInstantiateRequest = 10079;
        public const ushort PrefabInstantiateResponse = 10080;
        public const ushort PrefabSaveRequest = 10081;
        public const ushort PrefabSaveResponse = 10082;
        public const ushort PrefabApplyRequest = 10083;
        public const ushort PrefabApplyResponse = 10084;
        public const ushort PrefabUnpackRequest = 10085;
        public const ushort PrefabUnpackResponse = 10086;
        public const ushort PrefabGetInfoRequest = 10087;
        public const ushort PrefabGetInfoResponse = 10088;
        public const ushort PrefabGetHierarchyRequest = 10089;
        public const ushort PrefabGetHierarchyResponse = 10090;
        public const ushort BridgePropertyInfo = 10091;
        public const ushort InspectorGetComponentsRequest = 10092;
        public const ushort InspectorGetComponentsResponse = 10093;
        public const ushort InspectorGetPropertiesRequest = 10094;
        public const ushort InspectorGetPropertiesResponse = 10095;
        public const ushort InspectorGetPropertyRequest = 10096;
        public const ushort InspectorGetPropertyResponse = 10097;
        public const ushort InspectorFindPropertyRequest = 10098;
        public const ushort InspectorFindPropertyResponse = 10099;
        public const ushort InspectorSetPropertyRequest = 10100;
        public const ushort InspectorSetPropertyResponse = 10101;
        public const ushort InspectorSetPropertiesRequest = 10102;
        public const ushort InspectorSetPropertiesResponse = 10103;
        public const ushort InspectorAddComponentRequest = 10104;
        public const ushort InspectorAddComponentResponse = 10105;
        public const ushort InspectorRemoveComponentRequest = 10106;
        public const ushort InspectorRemoveComponentResponse = 10107;
        public const ushort Reload = 10108;
        public const ushort ReloadResponse = 10109;
        public const ushort EditorUndoRequest = 10110;
        public const ushort EditorUndoResponse = 10111;
        public const ushort EditorRedoRequest = 10112;
        public const ushort EditorRedoResponse = 10113;
        public const ushort EditorPauseRequest = 10114;
        public const ushort EditorPauseResponse = 10115;
        public const ushort EditorGetStateRequest = 10116;
        public const ushort EditorGetStateResponse = 10117;
        public const ushort Compile = 10118;
        public const ushort CompileResponse = 10119;
        public const ushort Refresh = 10120;
        public const ushort RefreshResponse = 10121;
        public const ushort RegenProject = 10122;
        public const ushort RegenProjectResponse = 10123;
        public const ushort EnterPlay = 10124;
        public const ushort EnterPlayResponse = 10125;
        public const ushort ExitPlay = 10126;
        public const ushort ExitPlayResponse = 10127;
        public const ushort HostState = 10128;
        public const ushort HostStateResponse = 10129;
        public const ushort BridgeBatchStepResult = 10130;
        public const ushort BatchExecuteRequest = 10131;
        public const ushort BatchExecuteResponse = 10132;
        public const ushort AssetImportRequest = 10133;
        public const ushort AssetImportResponse = 10134;
        public const ushort AssetRefreshRequest = 10135;
        public const ushort AssetRefreshResponse = 10136;
    }
}