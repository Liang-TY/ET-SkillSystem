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

    public static class UBridge
    {
        public const ushort BridgeConsoleLog = 10001;
        public const ushort ConsoleGetLogsRequest = 10002;
        public const ushort ConsoleGetLogsResponse = 10003;
    }
}