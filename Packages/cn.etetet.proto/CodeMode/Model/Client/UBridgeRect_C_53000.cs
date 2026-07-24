using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== RectTransform ====================
    [MemoryPackable]
    [Message(UBridgeRect.RectGetRequest)]
    [ResponseType(nameof(RectGetResponse))]
    public partial class RectGetRequest : MessageObject, IRequest
    {
        public static RectGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectGetRequest>(isFromPool);
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
    [Message(UBridgeRect.RectGetResponse)]
    public partial class RectGetResponse : MessageObject, IResponse
    {
        public static RectGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public float AnchorMinX { get; set; }

        [MemoryPackOrder(93)]
        public float AnchorMinY { get; set; }

        [MemoryPackOrder(94)]
        public float AnchorMaxX { get; set; }

        [MemoryPackOrder(95)]
        public float AnchorMaxY { get; set; }

        [MemoryPackOrder(96)]
        public float SizeDeltaX { get; set; }

        [MemoryPackOrder(97)]
        public float SizeDeltaY { get; set; }

        [MemoryPackOrder(98)]
        public float AnchoredPosX { get; set; }

        [MemoryPackOrder(99)]
        public float AnchoredPosY { get; set; }

        [MemoryPackOrder(100)]
        public float PivotX { get; set; }

        [MemoryPackOrder(101)]
        public float PivotY { get; set; }

        [MemoryPackOrder(102)]
        public float LocalRotX { get; set; }

        [MemoryPackOrder(103)]
        public float LocalRotY { get; set; }

        [MemoryPackOrder(104)]
        public float LocalRotZ { get; set; }

        [MemoryPackOrder(105)]
        public float LocalScaleX { get; set; }

        [MemoryPackOrder(106)]
        public float LocalScaleY { get; set; }

        [MemoryPackOrder(107)]
        public float LocalScaleZ { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AnchorMinX = default;
            this.AnchorMinY = default;
            this.AnchorMaxX = default;
            this.AnchorMaxY = default;
            this.SizeDeltaX = default;
            this.SizeDeltaY = default;
            this.AnchoredPosX = default;
            this.AnchoredPosY = default;
            this.PivotX = default;
            this.PivotY = default;
            this.LocalRotX = default;
            this.LocalRotY = default;
            this.LocalRotZ = default;
            this.LocalScaleX = default;
            this.LocalScaleY = default;
            this.LocalScaleZ = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetAnchorRequest)]
    [ResponseType(nameof(RectSetAnchorResponse))]
    public partial class RectSetAnchorRequest : MessageObject, IRequest
    {
        public static RectSetAnchorRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetAnchorRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float MinX { get; set; }

        [MemoryPackOrder(92)]
        public float MinY { get; set; }

        [MemoryPackOrder(93)]
        public float MaxX { get; set; }

        [MemoryPackOrder(94)]
        public float MaxY { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.MinX = default;
            this.MinY = default;
            this.MaxX = default;
            this.MaxY = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetAnchorResponse)]
    public partial class RectSetAnchorResponse : MessageObject, IResponse
    {
        public static RectSetAnchorResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetAnchorResponse>(isFromPool);
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
    [Message(UBridgeRect.RectSetSizeRequest)]
    [ResponseType(nameof(RectSetSizeResponse))]
    public partial class RectSetSizeRequest : MessageObject, IRequest
    {
        public static RectSetSizeRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetSizeRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float RectWidth { get; set; }

        [MemoryPackOrder(92)]
        public float RectHeight { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.RectWidth = default;
            this.RectHeight = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetSizeResponse)]
    public partial class RectSetSizeResponse : MessageObject, IResponse
    {
        public static RectSetSizeResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetSizeResponse>(isFromPool);
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
    [Message(UBridgeRect.RectSetPosRequest)]
    [ResponseType(nameof(RectSetPosResponse))]
    public partial class RectSetPosRequest : MessageObject, IRequest
    {
        public static RectSetPosRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetPosRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float X { get; set; }

        [MemoryPackOrder(92)]
        public float Y { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.X = default;
            this.Y = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetPosResponse)]
    public partial class RectSetPosResponse : MessageObject, IResponse
    {
        public static RectSetPosResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetPosResponse>(isFromPool);
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
    [Message(UBridgeRect.RectSetPivotRequest)]
    [ResponseType(nameof(RectSetPivotResponse))]
    public partial class RectSetPivotRequest : MessageObject, IRequest
    {
        public static RectSetPivotRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetPivotRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float X { get; set; }

        [MemoryPackOrder(92)]
        public float Y { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.X = default;
            this.Y = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetPivotResponse)]
    public partial class RectSetPivotResponse : MessageObject, IResponse
    {
        public static RectSetPivotResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetPivotResponse>(isFromPool);
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
    [Message(UBridgeRect.RectSetRotationRequest)]
    [ResponseType(nameof(RectSetRotationResponse))]
    public partial class RectSetRotationRequest : MessageObject, IRequest
    {
        public static RectSetRotationRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetRotationRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float X { get; set; }

        [MemoryPackOrder(92)]
        public float Y { get; set; }

        [MemoryPackOrder(93)]
        public float Z { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.X = default;
            this.Y = default;
            this.Z = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetRotationResponse)]
    public partial class RectSetRotationResponse : MessageObject, IResponse
    {
        public static RectSetRotationResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetRotationResponse>(isFromPool);
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
    [Message(UBridgeRect.RectSetScaleRequest)]
    [ResponseType(nameof(RectSetScaleResponse))]
    public partial class RectSetScaleRequest : MessageObject, IRequest
    {
        public static RectSetScaleRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetScaleRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public float X { get; set; }

        [MemoryPackOrder(92)]
        public float Y { get; set; }

        [MemoryPackOrder(93)]
        public float Z { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.X = default;
            this.Y = default;
            this.Z = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeRect.RectSetScaleResponse)]
    public partial class RectSetScaleResponse : MessageObject, IResponse
    {
        public static RectSetScaleResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RectSetScaleResponse>(isFromPool);
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

    public static class UBridgeRect
    {
        public const ushort RectGetRequest = 53001;
        public const ushort RectGetResponse = 53002;
        public const ushort RectSetAnchorRequest = 53003;
        public const ushort RectSetAnchorResponse = 53004;
        public const ushort RectSetSizeRequest = 53005;
        public const ushort RectSetSizeResponse = 53006;
        public const ushort RectSetPosRequest = 53007;
        public const ushort RectSetPosResponse = 53008;
        public const ushort RectSetPivotRequest = 53009;
        public const ushort RectSetPivotResponse = 53010;
        public const ushort RectSetRotationRequest = 53011;
        public const ushort RectSetRotationResponse = 53012;
        public const ushort RectSetScaleRequest = 53013;
        public const ushort RectSetScaleResponse = 53014;
    }
}