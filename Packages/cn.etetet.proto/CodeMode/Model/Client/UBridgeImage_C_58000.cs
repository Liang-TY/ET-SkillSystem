using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== Image ====================
    [MemoryPackable]
    [Message(UBridgeImage.ImageGetRequest)]
    [ResponseType(nameof(ImageGetResponse))]
    public partial class ImageGetRequest : MessageObject, IRequest
    {
        public static ImageGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ImageGetRequest>(isFromPool);
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
    [Message(UBridgeImage.ImageGetResponse)]
    public partial class ImageGetResponse : MessageObject, IResponse
    {
        public static ImageGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ImageGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string Sprite { get; set; }

        [MemoryPackOrder(93)]
        public double ColorR { get; set; }

        [MemoryPackOrder(94)]
        public double ColorG { get; set; }

        [MemoryPackOrder(95)]
        public double ColorB { get; set; }

        [MemoryPackOrder(96)]
        public double ColorA { get; set; }

        [MemoryPackOrder(97)]
        public int ImageType { get; set; }

        [MemoryPackOrder(98)]
        public double FillAmount { get; set; }

        [MemoryPackOrder(99)]
        public int FillMethod { get; set; }

        [MemoryPackOrder(100)]
        public bool RaycastTarget { get; set; }

        [MemoryPackOrder(101)]
        public bool PreserveAspect { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Sprite = default;
            this.ColorR = default;
            this.ColorG = default;
            this.ColorB = default;
            this.ColorA = default;
            this.ImageType = default;
            this.FillAmount = default;
            this.FillMethod = default;
            this.RaycastTarget = default;
            this.PreserveAspect = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeImage.ImageSetRequest)]
    [ResponseType(nameof(ImageSetResponse))]
    public partial class ImageSetRequest : MessageObject, IRequest
    {
        public static ImageSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ImageSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string Sprite { get; set; }

        [MemoryPackOrder(92)]
        public double ColorR { get; set; }

        [MemoryPackOrder(93)]
        public double ColorG { get; set; }

        [MemoryPackOrder(94)]
        public double ColorB { get; set; }

        [MemoryPackOrder(95)]
        public double ColorA { get; set; }

        [MemoryPackOrder(96)]
        public int ImageType { get; set; }

        [MemoryPackOrder(97)]
        public double FillAmount { get; set; }

        [MemoryPackOrder(98)]
        public int FillMethod { get; set; }

        [MemoryPackOrder(99)]
        public bool RaycastTarget { get; set; }

        [MemoryPackOrder(100)]
        public bool PreserveAspect { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.Sprite = default;
            this.ColorR = default;
            this.ColorG = default;
            this.ColorB = default;
            this.ColorA = default;
            this.ImageType = default;
            this.FillAmount = default;
            this.FillMethod = default;
            this.RaycastTarget = default;
            this.PreserveAspect = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeImage.ImageSetResponse)]
    public partial class ImageSetResponse : MessageObject, IResponse
    {
        public static ImageSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ImageSetResponse>(isFromPool);
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

    public static class UBridgeImage
    {
        public const ushort ImageGetRequest = 58001;
        public const ushort ImageGetResponse = 58002;
        public const ushort ImageSetRequest = 58003;
        public const ushort ImageSetResponse = 58004;
    }
}