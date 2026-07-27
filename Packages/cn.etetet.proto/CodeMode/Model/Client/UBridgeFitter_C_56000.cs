using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== ContentSizeFitter ====================
    [MemoryPackable]
    [Message(UBridgeFitter.FitterGetRequest)]
    [ResponseType(nameof(FitterGetResponse))]
    public partial class FitterGetRequest : MessageObject, IRequest
    {
        public static FitterGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<FitterGetRequest>(isFromPool);
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
    [Message(UBridgeFitter.FitterGetResponse)]
    public partial class FitterGetResponse : MessageObject, IResponse
    {
        public static FitterGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<FitterGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int HorizontalFit { get; set; }

        [MemoryPackOrder(93)]
        public int VerticalFit { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.HorizontalFit = default;
            this.VerticalFit = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeFitter.FitterSetRequest)]
    [ResponseType(nameof(FitterSetResponse))]
    public partial class FitterSetRequest : MessageObject, IRequest
    {
        public static FitterSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<FitterSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public int HorizontalFit { get; set; }

        [MemoryPackOrder(92)]
        public int VerticalFit { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.HorizontalFit = default;
            this.VerticalFit = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeFitter.FitterSetResponse)]
    public partial class FitterSetResponse : MessageObject, IResponse
    {
        public static FitterSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<FitterSetResponse>(isFromPool);
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

    public static class UBridgeFitter
    {
        public const ushort FitterGetRequest = 56001;
        public const ushort FitterGetResponse = 56002;
        public const ushort FitterSetRequest = 56003;
        public const ushort FitterSetResponse = 56004;
    }
}