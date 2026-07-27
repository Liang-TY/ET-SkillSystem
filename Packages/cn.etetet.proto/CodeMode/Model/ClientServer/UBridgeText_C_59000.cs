using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== Text ====================
    [MemoryPackable]
    [Message(UBridgeText.TextGetRequest)]
    [ResponseType(nameof(TextGetResponse))]
    public partial class TextGetRequest : MessageObject, IRequest
    {
        public static TextGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TextGetRequest>(isFromPool);
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
    [Message(UBridgeText.TextGetResponse)]
    public partial class TextGetResponse : MessageObject, IResponse
    {
        public static TextGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TextGetResponse>(isFromPool);
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
        public int FontSize { get; set; }

        [MemoryPackOrder(94)]
        public int FontStyle { get; set; }

        [MemoryPackOrder(95)]
        public int Alignment { get; set; }

        [MemoryPackOrder(96)]
        public double ColorR { get; set; }

        [MemoryPackOrder(97)]
        public double ColorG { get; set; }

        [MemoryPackOrder(98)]
        public double ColorB { get; set; }

        [MemoryPackOrder(99)]
        public double ColorA { get; set; }

        [MemoryPackOrder(100)]
        public bool BestFit { get; set; }

        [MemoryPackOrder(101)]
        public bool RaycastTarget { get; set; }

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
            this.FontSize = default;
            this.FontStyle = default;
            this.Alignment = default;
            this.ColorR = default;
            this.ColorG = default;
            this.ColorB = default;
            this.ColorA = default;
            this.BestFit = default;
            this.RaycastTarget = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeText.TextSetRequest)]
    [ResponseType(nameof(TextSetResponse))]
    public partial class TextSetRequest : MessageObject, IRequest
    {
        public static TextSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TextSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string Text { get; set; }

        [MemoryPackOrder(92)]
        public int FontSize { get; set; }

        [MemoryPackOrder(93)]
        public int FontStyle { get; set; }

        [MemoryPackOrder(94)]
        public int Alignment { get; set; }

        [MemoryPackOrder(95)]
        public double ColorR { get; set; }

        [MemoryPackOrder(96)]
        public double ColorG { get; set; }

        [MemoryPackOrder(97)]
        public double ColorB { get; set; }

        [MemoryPackOrder(98)]
        public double ColorA { get; set; }

        [MemoryPackOrder(99)]
        public bool BestFit { get; set; }

        [MemoryPackOrder(100)]
        public bool RaycastTarget { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Text = default;
            this.FontSize = default;
            this.FontStyle = default;
            this.Alignment = default;
            this.ColorR = default;
            this.ColorG = default;
            this.ColorB = default;
            this.ColorA = default;
            this.BestFit = default;
            this.RaycastTarget = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeText.TextSetResponse)]
    public partial class TextSetResponse : MessageObject, IResponse
    {
        public static TextSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TextSetResponse>(isFromPool);
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

    public static class UBridgeText
    {
        public const ushort TextGetRequest = 59001;
        public const ushort TextGetResponse = 59002;
        public const ushort TextSetRequest = 59003;
        public const ushort TextSetResponse = 59004;
    }
}