using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== Editor 控制 ====================
    [MemoryPackable]
    [Message(UBridgeEdit.Reload)]
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
    [Message(UBridgeEdit.ReloadResponse)]
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
    [Message(UBridgeEdit.EditorUndoRequest)]
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
    [Message(UBridgeEdit.EditorUndoResponse)]
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
    [Message(UBridgeEdit.EditorRedoRequest)]
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
    [Message(UBridgeEdit.EditorRedoResponse)]
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
    [Message(UBridgeEdit.EditorPauseRequest)]
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
    [Message(UBridgeEdit.EditorPauseResponse)]
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
    [Message(UBridgeEdit.EditorGetStateRequest)]
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
    [Message(UBridgeEdit.EditorGetStateResponse)]
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
    [Message(UBridgeEdit.Compile)]
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
    [Message(UBridgeEdit.CompileResponse)]
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
    [Message(UBridgeEdit.Refresh)]
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
    [Message(UBridgeEdit.RefreshResponse)]
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
    [Message(UBridgeEdit.RegenProject)]
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
    [Message(UBridgeEdit.RegenProjectResponse)]
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
    [Message(UBridgeEdit.EnterPlay)]
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
    [Message(UBridgeEdit.EnterPlayResponse)]
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
    [Message(UBridgeEdit.ExitPlay)]
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
    [Message(UBridgeEdit.ExitPlayResponse)]
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
    [Message(UBridgeEdit.HostState)]
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
    [Message(UBridgeEdit.HostStateResponse)]
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
    [Message(UBridgeEdit.BridgeBatchStepResult)]
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
    [Message(UBridgeEdit.BatchExecuteRequest)]
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
    [Message(UBridgeEdit.BatchExecuteResponse)]
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
    [Message(UBridgeEdit.AssetImportRequest)]
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
    [Message(UBridgeEdit.AssetImportResponse)]
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
    [Message(UBridgeEdit.AssetRefreshRequest)]
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
    [Message(UBridgeEdit.AssetRefreshResponse)]
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

    // ==================== 剩余命令 ====================
    [MemoryPackable]
    [Message(UBridgeEdit.BridgeGameViewResolution)]
    public partial class BridgeGameViewResolution : MessageObject
    {
        public static BridgeGameViewResolution Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeGameViewResolution>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int Width { get; set; }

        [MemoryPackOrder(1)]
        public int Height { get; set; }

        [MemoryPackOrder(2)]
        public string Label { get; set; }

        [MemoryPackOrder(3)]
        public bool IsCurrent { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Width = default;
            this.Height = default;
            this.Label = default;
            this.IsCurrent = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.BridgeTestResult)]
    public partial class BridgeTestResult : MessageObject
    {
        public static BridgeTestResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeTestResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]
        public string FullName { get; set; }

        [MemoryPackOrder(2)]
        public bool Passed { get; set; }

        [MemoryPackOrder(3)]
        public int Error { get; set; }

        [MemoryPackOrder(4)]
        public string Message { get; set; }

        [MemoryPackOrder(5)]
        public long DurationMs { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.FullName = default;
            this.Passed = default;
            this.Error = default;
            this.Message = default;
            this.DurationMs = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.TestEcho)]
    [ResponseType(nameof(TestEchoResponse))]
    public partial class TestEcho : MessageObject, IRequest
    {
        public static TestEcho Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TestEcho>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Text { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Text = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.TestEchoResponse)]
    public partial class TestEchoResponse : MessageObject, IResponse
    {
        public static TestEchoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TestEchoResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string Text { get; set; }

        [MemoryPackOrder(93)]
        public long HandledAt { get; set; }

        [MemoryPackOrder(94)]
        public string Handler { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Text = default;
            this.HandledAt = default;
            this.Handler = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.EditorLogRequest)]
    [ResponseType(nameof(EditorLogResponse))]
    public partial class EditorLogRequest : MessageObject, IRequest
    {
        public static EditorLogRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorLogRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Message { get; set; }

        [MemoryPackOrder(91)]
        public string LogType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Message = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.EditorLogResponse)]
    public partial class EditorLogResponse : MessageObject, IResponse
    {
        public static EditorLogResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorLogResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public bool Logged { get; set; }

        [MemoryPackOrder(93)]
        public string LogType { get; set; }

        [MemoryPackOrder(94)]
        public string LoggedMessage { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Logged = default;
            this.LogType = default;
            this.LoggedMessage = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.GameViewGetResolutionRequest)]
    [ResponseType(nameof(GameViewGetResolutionResponse))]
    public partial class GameViewGetResolutionRequest : MessageObject, IRequest
    {
        public static GameViewGetResolutionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewGetResolutionRequest>(isFromPool);
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
    [Message(UBridgeEdit.GameViewGetResolutionResponse)]
    public partial class GameViewGetResolutionResponse : MessageObject, IResponse
    {
        public static GameViewGetResolutionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewGetResolutionResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeGameViewResolution Resolution { get; set; }

        [MemoryPackOrder(93)]
        public int SelectedIndex { get; set; }

        [MemoryPackOrder(94)]
        public string SizeType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolution = default;
            this.SelectedIndex = default;
            this.SizeType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.GameViewListResolutionsRequest)]
    [ResponseType(nameof(GameViewListResolutionsResponse))]
    public partial class GameViewListResolutionsRequest : MessageObject, IRequest
    {
        public static GameViewListResolutionsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewListResolutionsRequest>(isFromPool);
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
    [Message(UBridgeEdit.GameViewListResolutionsResponse)]
    public partial class GameViewListResolutionsResponse : MessageObject, IResponse
    {
        public static GameViewListResolutionsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewListResolutionsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<BridgeGameViewResolution> Resolutions { get; set; } = new();

        [MemoryPackOrder(93)]
        public int Count { get; set; }

        [MemoryPackOrder(94)]
        public int CurrentIndex { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolutions.Clear();
            this.Count = default;
            this.CurrentIndex = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.GameViewSetResolutionRequest)]
    [ResponseType(nameof(GameViewSetResolutionResponse))]
    public partial class GameViewSetResolutionRequest : MessageObject, IRequest
    {
        public static GameViewSetResolutionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewSetResolutionRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Width { get; set; }

        [MemoryPackOrder(91)]
        public int Height { get; set; }

        [MemoryPackOrder(92)]
        public string Label { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Width = default;
            this.Height = default;
            this.Label = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeEdit.GameViewSetResolutionResponse)]
    public partial class GameViewSetResolutionResponse : MessageObject, IResponse
    {
        public static GameViewSetResolutionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewSetResolutionResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public BridgeGameViewResolution Resolution { get; set; }

        [MemoryPackOrder(93)]
        public int SelectedIndex { get; set; }

        [MemoryPackOrder(94)]
        public bool WasAdded { get; set; }

        [MemoryPackOrder(95)]
        public string SizeType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolution = default;
            this.SelectedIndex = default;
            this.WasAdded = default;
            this.SizeType = default;

            ObjectPool.Recycle(this);
        }
    }

    public static class UBridgeEdit
    {
        public const ushort Reload = 52001;
        public const ushort ReloadResponse = 52002;
        public const ushort EditorUndoRequest = 52003;
        public const ushort EditorUndoResponse = 52004;
        public const ushort EditorRedoRequest = 52005;
        public const ushort EditorRedoResponse = 52006;
        public const ushort EditorPauseRequest = 52007;
        public const ushort EditorPauseResponse = 52008;
        public const ushort EditorGetStateRequest = 52009;
        public const ushort EditorGetStateResponse = 52010;
        public const ushort Compile = 52011;
        public const ushort CompileResponse = 52012;
        public const ushort Refresh = 52013;
        public const ushort RefreshResponse = 52014;
        public const ushort RegenProject = 52015;
        public const ushort RegenProjectResponse = 52016;
        public const ushort EnterPlay = 52017;
        public const ushort EnterPlayResponse = 52018;
        public const ushort ExitPlay = 52019;
        public const ushort ExitPlayResponse = 52020;
        public const ushort HostState = 52021;
        public const ushort HostStateResponse = 52022;
        public const ushort BridgeBatchStepResult = 52023;
        public const ushort BatchExecuteRequest = 52024;
        public const ushort BatchExecuteResponse = 52025;
        public const ushort AssetImportRequest = 52026;
        public const ushort AssetImportResponse = 52027;
        public const ushort AssetRefreshRequest = 52028;
        public const ushort AssetRefreshResponse = 52029;
        public const ushort BridgeGameViewResolution = 52030;
        public const ushort BridgeTestResult = 52031;
        public const ushort TestEcho = 52032;
        public const ushort TestEchoResponse = 52033;
        public const ushort EditorLogRequest = 52034;
        public const ushort EditorLogResponse = 52035;
        public const ushort GameViewGetResolutionRequest = 52036;
        public const ushort GameViewGetResolutionResponse = 52037;
        public const ushort GameViewListResolutionsRequest = 52038;
        public const ushort GameViewListResolutionsResponse = 52039;
        public const ushort GameViewSetResolutionRequest = 52040;
        public const ushort GameViewSetResolutionResponse = 52041;
    }
}