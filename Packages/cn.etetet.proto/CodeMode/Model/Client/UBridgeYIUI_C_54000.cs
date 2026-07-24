using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== YIUI Create Panel ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreatePanelRequest)]
    [ResponseType(nameof(YIUICreatePanelResponse))]
    public partial class YIUICreatePanelRequest : MessageObject, IRequest
    {
        public static YIUICreatePanelRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreatePanelRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public string Name { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.Name = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreatePanelResponse)]
    public partial class YIUICreatePanelResponse : MessageObject, IResponse
    {
        public static YIUICreatePanelResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreatePanelResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string PrefabPath { get; set; }

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

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.PrefabLoadForEditRequest)]
    [ResponseType(nameof(PrefabLoadForEditResponse))]
    public partial class PrefabLoadForEditRequest : MessageObject, IRequest
    {
        public static PrefabLoadForEditRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabLoadForEditRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.PrefabLoadForEditResponse)]
    public partial class PrefabLoadForEditResponse : MessageObject, IResponse
    {
        public static PrefabLoadForEditResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabLoadForEditResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int RootInstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.RootInstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.PrefabSaveModifiedRequest)]
    [ResponseType(nameof(PrefabSaveModifiedResponse))]
    public partial class PrefabSaveModifiedRequest : MessageObject, IRequest
    {
        public static PrefabSaveModifiedRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveModifiedRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(91)]
        public string PrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.InstanceId = default;
            this.PrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.PrefabSaveModifiedResponse)]
    public partial class PrefabSaveModifiedResponse : MessageObject, IResponse
    {
        public static PrefabSaveModifiedResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveModifiedResponse>(isFromPool);
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

    // ==================== YIUI Add Control ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIAddControlRequest)]
    [ResponseType(nameof(YIUIAddControlResponse))]
    public partial class YIUIAddControlRequest : MessageObject, IRequest
    {
        public static YIUIAddControlRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIAddControlRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int ParentId { get; set; }

        [MemoryPackOrder(91)]
        public string Name { get; set; }

        [MemoryPackOrder(92)]
        public string Type { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ParentId = default;
            this.Name = default;
            this.Type = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIAddControlResponse)]
    public partial class YIUIAddControlResponse : MessageObject, IResponse
    {
        public static YIUIAddControlResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIAddControlResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int InstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== AddControl (Standard Unity UI) ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.AddControlRequest)]
    [ResponseType(nameof(AddControlResponse))]
    public partial class AddControlRequest : MessageObject, IRequest
    {
        public static AddControlRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AddControlRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int ParentId { get; set; }

        [MemoryPackOrder(91)]
        public string Name { get; set; }

        [MemoryPackOrder(92)]
        public string Type { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ParentId = default;
            this.Name = default;
            this.Type = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.AddControlResponse)]
    public partial class AddControlResponse : MessageObject, IResponse
    {
        public static AddControlResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AddControlResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public int InstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    // ==================== YIUI CDE Table ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIBindingInfo)]
    public partial class YIUIBindingInfo : MessageObject
    {
        public static YIUIBindingInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIBindingInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]
        public string ComponentType { get; set; }

        [MemoryPackOrder(2)]
        public string ComponentName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.ComponentType = default;
            this.ComponentName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGetBindingsRequest)]
    [ResponseType(nameof(YIUIGetBindingsResponse))]
    public partial class YIUIGetBindingsRequest : MessageObject, IRequest
    {
        public static YIUIGetBindingsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGetBindingsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGetBindingsResponse)]
    public partial class YIUIGetBindingsResponse : MessageObject, IResponse
    {
        public static YIUIGetBindingsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGetBindingsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<YIUIBindingInfo> Bindings { get; set; } = new();

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
            this.Bindings.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIEventItem)]
    public partial class YIUIEventItem : MessageObject
    {
        public static YIUIEventItem Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIEventItem>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string EventName { get; set; }

        [MemoryPackOrder(1)]
        public string EventType { get; set; }

        [MemoryPackOrder(2)]
        public string ParamTypes { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.EventName = default;
            this.EventType = default;
            this.ParamTypes = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGetEventsRequest)]
    [ResponseType(nameof(YIUIGetEventsResponse))]
    public partial class YIUIGetEventsRequest : MessageObject, IRequest
    {
        public static YIUIGetEventsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGetEventsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGetEventsResponse)]
    public partial class YIUIGetEventsResponse : MessageObject, IResponse
    {
        public static YIUIGetEventsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGetEventsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public List<YIUIEventItem> Events { get; set; } = new();

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
            this.Events.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIBindComponentRequest)]
    [ResponseType(nameof(YIUIBindComponentResponse))]
    public partial class YIUIBindComponentRequest : MessageObject, IRequest
    {
        public static YIUIBindComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIBindComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string ControlName { get; set; }

        [MemoryPackOrder(92)]
        public string BindName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.ControlName = default;
            this.BindName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIBindComponentResponse)]
    public partial class YIUIBindComponentResponse : MessageObject, IResponse
    {
        public static YIUIBindComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIBindComponentResponse>(isFromPool);
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
    [Message(UBridgeYIUI.YIUIBindEventRequest)]
    [ResponseType(nameof(YIUIBindEventResponse))]
    public partial class YIUIBindEventRequest : MessageObject, IRequest
    {
        public static YIUIBindEventRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIBindEventRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string EventName { get; set; }

        [MemoryPackOrder(92)]
        public string EventType { get; set; }

        [MemoryPackOrder(93)]
        public string ParamTypes { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.EventName = default;
            this.EventType = default;
            this.ParamTypes = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIBindEventResponse)]
    public partial class YIUIBindEventResponse : MessageObject, IResponse
    {
        public static YIUIBindEventResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIBindEventResponse>(isFromPool);
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
    [Message(UBridgeYIUI.YIUIAttachEventRequest)]
    [ResponseType(nameof(YIUIAttachEventResponse))]
    public partial class YIUIAttachEventRequest : MessageObject, IRequest
    {
        public static YIUIAttachEventRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIAttachEventRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string TargetName { get; set; }

        [MemoryPackOrder(92)]
        public string EventName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.TargetName = default;
            this.EventName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIAttachEventResponse)]
    public partial class YIUIAttachEventResponse : MessageObject, IResponse
    {
        public static YIUIAttachEventResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIAttachEventResponse>(isFromPool);
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

    // ==================== YIUI Generate Code ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGenerateCodeRequest)]
    [ResponseType(nameof(YIUIGenerateCodeResponse))]
    public partial class YIUIGenerateCodeRequest : MessageObject, IRequest
    {
        public static YIUIGenerateCodeRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGenerateCodeRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string PackageName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.PackageName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIGenerateCodeResponse)]
    public partial class YIUIGenerateCodeResponse : MessageObject, IResponse
    {
        public static YIUIGenerateCodeResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIGenerateCodeResponse>(isFromPool);
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

    // ==================== YIUI Clear Bindings ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIClearBindingsRequest)]
    [ResponseType(nameof(YIUIClearBindingsResponse))]
    public partial class YIUIClearBindingsRequest : MessageObject, IRequest
    {
        public static YIUIClearBindingsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIClearBindingsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string Target { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.Target = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIClearBindingsResponse)]
    public partial class YIUIClearBindingsResponse : MessageObject, IResponse
    {
        public static YIUIClearBindingsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIClearBindingsResponse>(isFromPool);
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

    // ==================== YIUI Remove Control ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIRemoveControlRequest)]
    [ResponseType(nameof(YIUIRemoveControlResponse))]
    public partial class YIUIRemoveControlRequest : MessageObject, IRequest
    {
        public static YIUIRemoveControlRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIRemoveControlRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string ControlName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.ControlName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUIRemoveControlResponse)]
    public partial class YIUIRemoveControlResponse : MessageObject, IResponse
    {
        public static YIUIRemoveControlResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUIRemoveControlResponse>(isFromPool);
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
    [Message(UBridgeYIUI.YIUICreateCommonRequest)]
    [ResponseType(nameof(YIUICreateCommonResponse))]
    public partial class YIUICreateCommonRequest : MessageObject, IRequest
    {
        public static YIUICreateCommonRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateCommonRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public string Name { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.Name = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateCommonResponse)]
    public partial class YIUICreateCommonResponse : MessageObject, IResponse
    {
        public static YIUICreateCommonResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateCommonResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string PrefabPath { get; set; }

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

            ObjectPool.Recycle(this);
        }
    }

    // ==================== YIUI Create View ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateViewRequest)]
    [ResponseType(nameof(YIUICreateViewResponse))]
    public partial class YIUICreateViewRequest : MessageObject, IRequest
    {
        public static YIUICreateViewRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateViewRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public string Name { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.Name = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateViewResponse)]
    public partial class YIUICreateViewResponse : MessageObject, IResponse
    {
        public static YIUICreateViewResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateViewResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string PrefabPath { get; set; }

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

            ObjectPool.Recycle(this);
        }
    }

    // ==================== YIUI Create AllView ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateAllViewRequest)]
    [ResponseType(nameof(YIUICreateAllViewResponse))]
    public partial class YIUICreateAllViewRequest : MessageObject, IRequest
    {
        public static YIUICreateAllViewRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateAllViewRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateAllViewResponse)]
    public partial class YIUICreateAllViewResponse : MessageObject, IResponse
    {
        public static YIUICreateAllViewResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateAllViewResponse>(isFromPool);
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

    // ==================== YIUI Create UIView in Panel ====================
    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateUIViewRequest)]
    [ResponseType(nameof(YIUICreateUIViewResponse))]
    public partial class YIUICreateUIViewRequest : MessageObject, IRequest
    {
        public static YIUICreateUIViewRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateUIViewRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string PrefabPath { get; set; }

        [MemoryPackOrder(91)]
        public string ViewPrefabPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PrefabPath = default;
            this.ViewPrefabPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeYIUI.YIUICreateUIViewResponse)]
    public partial class YIUICreateUIViewResponse : MessageObject, IResponse
    {
        public static YIUICreateUIViewResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<YIUICreateUIViewResponse>(isFromPool);
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

    public static class UBridgeYIUI
    {
        public const ushort YIUICreatePanelRequest = 54001;
        public const ushort YIUICreatePanelResponse = 54002;
        public const ushort PrefabLoadForEditRequest = 54003;
        public const ushort PrefabLoadForEditResponse = 54004;
        public const ushort PrefabSaveModifiedRequest = 54005;
        public const ushort PrefabSaveModifiedResponse = 54006;
        public const ushort YIUIAddControlRequest = 54007;
        public const ushort YIUIAddControlResponse = 54008;
        public const ushort AddControlRequest = 54009;
        public const ushort AddControlResponse = 54010;
        public const ushort YIUIBindingInfo = 54011;
        public const ushort YIUIGetBindingsRequest = 54012;
        public const ushort YIUIGetBindingsResponse = 54013;
        public const ushort YIUIEventItem = 54014;
        public const ushort YIUIGetEventsRequest = 54015;
        public const ushort YIUIGetEventsResponse = 54016;
        public const ushort YIUIBindComponentRequest = 54017;
        public const ushort YIUIBindComponentResponse = 54018;
        public const ushort YIUIBindEventRequest = 54019;
        public const ushort YIUIBindEventResponse = 54020;
        public const ushort YIUIAttachEventRequest = 54021;
        public const ushort YIUIAttachEventResponse = 54022;
        public const ushort YIUIGenerateCodeRequest = 54023;
        public const ushort YIUIGenerateCodeResponse = 54024;
        public const ushort YIUIClearBindingsRequest = 54025;
        public const ushort YIUIClearBindingsResponse = 54026;
        public const ushort YIUIRemoveControlRequest = 54027;
        public const ushort YIUIRemoveControlResponse = 54028;
        public const ushort YIUICreateCommonRequest = 54029;
        public const ushort YIUICreateCommonResponse = 54030;
        public const ushort YIUICreateViewRequest = 54031;
        public const ushort YIUICreateViewResponse = 54032;
        public const ushort YIUICreateAllViewRequest = 54033;
        public const ushort YIUICreateAllViewResponse = 54034;
        public const ushort YIUICreateUIViewRequest = 54035;
        public const ushort YIUICreateUIViewResponse = 54036;
    }
}