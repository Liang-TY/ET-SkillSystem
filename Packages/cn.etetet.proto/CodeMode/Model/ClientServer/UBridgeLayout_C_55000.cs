using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== LayoutGroup ====================
    [MemoryPackable]
    [Message(UBridgeLayout.LayoutGetRequest)]
    [ResponseType(nameof(LayoutGetResponse))]
    public partial class LayoutGetRequest : MessageObject, IRequest
    {
        public static LayoutGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LayoutGetRequest>(isFromPool);
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
    [Message(UBridgeLayout.LayoutGetResponse)]
    public partial class LayoutGetResponse : MessageObject, IResponse
    {
        public static LayoutGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LayoutGetResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string Type { get; set; }

        [MemoryPackOrder(93)]
        public int PaddingLeft { get; set; }

        [MemoryPackOrder(94)]
        public int PaddingRight { get; set; }

        [MemoryPackOrder(95)]
        public int PaddingTop { get; set; }

        [MemoryPackOrder(96)]
        public int PaddingBottom { get; set; }

        [MemoryPackOrder(97)]
        public double Spacing { get; set; }

        [MemoryPackOrder(98)]
        public double SpacingX { get; set; }

        [MemoryPackOrder(99)]
        public double SpacingY { get; set; }

        [MemoryPackOrder(100)]
        public int ChildAlignment { get; set; }

        [MemoryPackOrder(101)]
        public bool ReverseArrangement { get; set; }

        [MemoryPackOrder(102)]
        public bool ControlChildWidth { get; set; }

        [MemoryPackOrder(103)]
        public bool ControlChildHeight { get; set; }

        [MemoryPackOrder(104)]
        public bool ChildForceExpandWidth { get; set; }

        [MemoryPackOrder(105)]
        public bool ChildForceExpandHeight { get; set; }

        [MemoryPackOrder(106)]
        public double CellSizeX { get; set; }

        [MemoryPackOrder(107)]
        public double CellSizeY { get; set; }

        [MemoryPackOrder(108)]
        public int Constraint { get; set; }

        [MemoryPackOrder(109)]
        public int ConstraintCount { get; set; }

        [MemoryPackOrder(110)]
        public int StartCorner { get; set; }

        [MemoryPackOrder(111)]
        public int StartAxis { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Type = default;
            this.PaddingLeft = default;
            this.PaddingRight = default;
            this.PaddingTop = default;
            this.PaddingBottom = default;
            this.Spacing = default;
            this.SpacingX = default;
            this.SpacingY = default;
            this.ChildAlignment = default;
            this.ReverseArrangement = default;
            this.ControlChildWidth = default;
            this.ControlChildHeight = default;
            this.ChildForceExpandWidth = default;
            this.ChildForceExpandHeight = default;
            this.CellSizeX = default;
            this.CellSizeY = default;
            this.Constraint = default;
            this.ConstraintCount = default;
            this.StartCorner = default;
            this.StartAxis = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeLayout.LayoutSetRequest)]
    [ResponseType(nameof(LayoutSetResponse))]
    public partial class LayoutSetRequest : MessageObject, IRequest
    {
        public static LayoutSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LayoutSetRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public int PaddingLeft { get; set; }

        [MemoryPackOrder(92)]
        public int PaddingRight { get; set; }

        [MemoryPackOrder(93)]
        public int PaddingTop { get; set; }

        [MemoryPackOrder(94)]
        public int PaddingBottom { get; set; }

        [MemoryPackOrder(95)]
        public double Spacing { get; set; }

        [MemoryPackOrder(96)]
        public double SpacingX { get; set; }

        [MemoryPackOrder(97)]
        public double SpacingY { get; set; }

        [MemoryPackOrder(98)]
        public int ChildAlignment { get; set; }

        [MemoryPackOrder(99)]
        public bool ReverseArrangement { get; set; }

        [MemoryPackOrder(100)]
        public bool ControlChildWidth { get; set; }

        [MemoryPackOrder(101)]
        public bool ControlChildHeight { get; set; }

        [MemoryPackOrder(102)]
        public bool ChildForceExpandWidth { get; set; }

        [MemoryPackOrder(103)]
        public bool ChildForceExpandHeight { get; set; }

        [MemoryPackOrder(104)]
        public double CellSizeX { get; set; }

        [MemoryPackOrder(105)]
        public double CellSizeY { get; set; }

        [MemoryPackOrder(106)]
        public int Constraint { get; set; }

        [MemoryPackOrder(107)]
        public int ConstraintCount { get; set; }

        [MemoryPackOrder(108)]
        public int StartCorner { get; set; }

        [MemoryPackOrder(109)]
        public int StartAxis { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.PaddingLeft = default;
            this.PaddingRight = default;
            this.PaddingTop = default;
            this.PaddingBottom = default;
            this.Spacing = default;
            this.SpacingX = default;
            this.SpacingY = default;
            this.ChildAlignment = default;
            this.ReverseArrangement = default;
            this.ControlChildWidth = default;
            this.ControlChildHeight = default;
            this.ChildForceExpandWidth = default;
            this.ChildForceExpandHeight = default;
            this.CellSizeX = default;
            this.CellSizeY = default;
            this.Constraint = default;
            this.ConstraintCount = default;
            this.StartCorner = default;
            this.StartAxis = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeLayout.LayoutSetResponse)]
    public partial class LayoutSetResponse : MessageObject, IResponse
    {
        public static LayoutSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LayoutSetResponse>(isFromPool);
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

    public static class UBridgeLayout
    {
        public const ushort LayoutGetRequest = 55001;
        public const ushort LayoutGetResponse = 55002;
        public const ushort LayoutSetRequest = 55003;
        public const ushort LayoutSetResponse = 55004;
    }
}