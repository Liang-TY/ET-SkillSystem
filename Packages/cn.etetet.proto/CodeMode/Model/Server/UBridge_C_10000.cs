using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // 单条控制台日志
    [MemoryPackable]
    [Message(UBridge.BridgeConsoleLog)]
    public partial class BridgeConsoleLog : MessageObject
    {
        public static BridgeConsoleLog Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeConsoleLog>(isFromPool);
        }

        /// <summary>
        /// Error/Warning/Log/Exception
        /// </summary>
        [MemoryPackOrder(0)]
        public string LogType { get; set; }

        /// <summary>
        /// 日志正文
        /// </summary>
        [MemoryPackOrder(1)]
        public string Message { get; set; }

        /// <summary>
        /// 堆栈跟踪
        /// </summary>
        [MemoryPackOrder(2)]
        public string StackTrace { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
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

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 要获取的条数，0=全部
        /// </summary>
        [MemoryPackOrder(1)]
        public int Count { get; set; }

        /// <summary>
        /// 类型过滤: Error/Warning/Log/All
        /// </summary>
        [MemoryPackOrder(2)]
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

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<BridgeConsoleLog> Logs { get; set; } = new();

        /// <summary>
        /// 实际返回条数
        /// </summary>
        [MemoryPackOrder(4)]
        public int Count { get; set; }

        /// <summary>
        /// 控制台日志总数
        /// </summary>
        [MemoryPackOrder(5)]
        public int TotalCount { get; set; }

        /// <summary>
        /// 实际过滤类型
        /// </summary>
        [MemoryPackOrder(6)]
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

    // 截图文件信息
    [MemoryPackable]
    [Message(UBridge.BridgeScreenshotInfo)]
    public partial class BridgeScreenshotInfo : MessageObject
    {
        public static BridgeScreenshotInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeScreenshotInfo>(isFromPool);
        }

        /// <summary>
        /// 完整文件路径
        /// </summary>
        [MemoryPackOrder(0)]
        public string Path { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        [MemoryPackOrder(1)]
        public string FileName { get; set; }

        /// <summary>
        /// 图像宽度
        /// </summary>
        [MemoryPackOrder(2)]
        public int Width { get; set; }

        /// <summary>
        /// 图像高度
        /// </summary>
        [MemoryPackOrder(3)]
        public int Height { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [MemoryPackOrder(4)]
        public long FileSize { get; set; }

        /// <summary>
        /// MIME 类型 "image/png" 或 "image/jpeg"
        /// </summary>
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

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 截图目标，"game" 或 "gameview"
        /// </summary>
        [MemoryPackOrder(1)]
        public string Target { get; set; }

        /// <summary>
        /// 输出格式，"png"、"jpg"、"jpeg"
        /// </summary>
        [MemoryPackOrder(2)]
        public string Format { get; set; }

        /// <summary>
        /// JPEG 质量 1-100，默认 85
        /// </summary>
        [MemoryPackOrder(3)]
        public int Quality { get; set; }

        /// <summary>
        /// 是否允许 EditMode 截图
        /// </summary>
        [MemoryPackOrder(4)]
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

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [MemoryPackOrder(3)]
        public bool Captured { get; set; }

        /// <summary>
        /// 实际目标
        /// </summary>
        [MemoryPackOrder(4)]
        public string Target { get; set; }

        /// <summary>
        /// 截图文件信息
        /// </summary>
        [MemoryPackOrder(5)]
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

    public static class UBridge
    {
        public const ushort BridgeConsoleLog = 10001;
        public const ushort ConsoleGetLogsRequest = 10002;
        public const ushort ConsoleGetLogsResponse = 10003;
        public const ushort BridgeScreenshotInfo = 10004;
        public const ushort ScreenshotCaptureRequest = 10005;
        public const ushort ScreenshotCaptureResponse = 10006;
    }
}