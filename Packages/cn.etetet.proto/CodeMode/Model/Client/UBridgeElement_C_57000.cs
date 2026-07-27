using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== LayoutElement ====================
    [MemoryPackable]
    [Message(UBridgeElement.ElementGetRequest)]
    [ResponseType(nameof(ElementGetResponse))]
    public partial class ElementGetRequest : MessageObject, IRequest
    {
        public static ElementGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ElementGetRequest>(isFromPool);
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
    [Message(UBridgeElement.ElementGetResponse)]
    public partial class ElementGetResponse : MessageObject, IResponse
    {
        public static ElementGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ElementGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public double MinWidth { get; set; }

        [MemoryPackOrder(93)]
        public double MinHeight { get; set; }

        [MemoryPackOrder(94)]
        public double PreferredWidth { get; set; }

        [MemoryPackOrder(95)]
        public double PreferredHeight { get; set; }

        [MemoryPackOrder(96)]
        public double FlexibleWidth { get; set; }

        [MemoryPackOrder(97)]
        public double FlexibleHeight { get; set; }

        [MemoryPackOrder(98)]
        public bool IgnoreLayout { get; set; }

        [MemoryPackOrder(99)]
        public int LayoutPriority { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MinWidth = default;
            this.MinHeight = default;
            this.PreferredWidth = default;
            this.PreferredHeight = default;
            this.FlexibleWidth = default;
            this.FlexibleHeight = default;
            this.IgnoreLayout = default;
            this.LayoutPriority = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeElement.ElementSetRequest)]
    [ResponseType(nameof(ElementSetResponse))]
    public partial class ElementSetRequest : MessageObject, IRequest
    {
        public static ElementSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ElementSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public double MinWidth { get; set; }

        [MemoryPackOrder(92)]
        public double MinHeight { get; set; }

        [MemoryPackOrder(93)]
        public double PreferredWidth { get; set; }

        [MemoryPackOrder(94)]
        public double PreferredHeight { get; set; }

        [MemoryPackOrder(95)]
        public double FlexibleWidth { get; set; }

        [MemoryPackOrder(96)]
        public double FlexibleHeight { get; set; }

        [MemoryPackOrder(97)]
        public bool IgnoreLayout { get; set; }

        [MemoryPackOrder(98)]
        public int LayoutPriority { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.MinWidth = default;
            this.MinHeight = default;
            this.PreferredWidth = default;
            this.PreferredHeight = default;
            this.FlexibleWidth = default;
            this.FlexibleHeight = default;
            this.IgnoreLayout = default;
            this.LayoutPriority = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeElement.ElementSetResponse)]
    public partial class ElementSetResponse : MessageObject, IResponse
    {
        public static ElementSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ElementSetResponse>(isFromPool);
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

    public static class UBridgeElement
    {
        public const ushort ElementGetRequest = 57001;
        public const ushort ElementGetResponse = 57002;
        public const ushort ElementSetRequest = 57003;
        public const ushort ElementSetResponse = 57004;
    }
}