using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    // ==================== Inspector ====================
    [MemoryPackable]
    [Message(UBridgeInsp.BridgePropertyInfo)]
    public partial class BridgePropertyInfo : MessageObject
    {
        public static BridgePropertyInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgePropertyInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }

        [MemoryPackOrder(1)]
        public string DisplayName { get; set; }

        [MemoryPackOrder(2)]
        public string Type { get; set; }

        [MemoryPackOrder(3)]
        public string StringValue { get; set; }

        [MemoryPackOrder(4)]
        public int IntValue { get; set; }

        [MemoryPackOrder(5)]
        public float FloatValue { get; set; }

        [MemoryPackOrder(6)]
        public bool BoolValue { get; set; }

        [MemoryPackOrder(7)]
        public BridgeVector2 Vector2Value { get; set; }

        [MemoryPackOrder(8)]
        public BridgeVector3 Vector3Value { get; set; }

        [MemoryPackOrder(9)]
        public string ObjectReferencePath { get; set; }

        [MemoryPackOrder(10)]
        public string ObjectReferenceType { get; set; }

        [MemoryPackOrder(11)]
        public bool IsArray { get; set; }

        [MemoryPackOrder(12)]
        public bool IsEditable { get; set; }

        [MemoryPackOrder(13)]
        public string PropertyPath { get; set; }

        [MemoryPackOrder(14)]
        public bool IsExpanded { get; set; }

        [MemoryPackOrder(15)]
        public bool HasChildren { get; set; }

        [MemoryPackOrder(16)]
        public int Depth { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.DisplayName = default;
            this.Type = default;
            this.StringValue = default;
            this.IntValue = default;
            this.FloatValue = default;
            this.BoolValue = default;
            this.Vector2Value = default;
            this.Vector3Value = default;
            this.ObjectReferencePath = default;
            this.ObjectReferenceType = default;
            this.IsArray = default;
            this.IsEditable = default;
            this.PropertyPath = default;
            this.IsExpanded = default;
            this.HasChildren = default;
            this.Depth = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetComponentsRequest)]
    [ResponseType(nameof(InspectorGetComponentsResponse))]
    public partial class InspectorGetComponentsRequest : MessageObject, IRequest
    {
        public static InspectorGetComponentsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetComponentsResponse)]
    public partial class InspectorGetComponentsResponse : MessageObject, IResponse
    {
        public static InspectorGetComponentsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public List<BridgeComponentInfo> Components { get; set; } = new();

        [MemoryPackOrder(96)]
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
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Components.Clear();
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetPropertiesRequest)]
    [ResponseType(nameof(InspectorGetPropertiesResponse))]
    public partial class InspectorGetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public bool IncludeChildren { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.IncludeChildren = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetPropertiesResponse)]
    public partial class InspectorGetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetPropertyRequest)]
    [ResponseType(nameof(InspectorGetPropertyResponse))]
    public partial class InspectorGetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string PropertyName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.PropertyName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorGetPropertyResponse)]
    public partial class InspectorGetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(96)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(97)]
        public BridgePropertyInfo Property { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Property = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorFindPropertyRequest)]
    [ResponseType(nameof(InspectorFindPropertyResponse))]
    public partial class InspectorFindPropertyRequest : MessageObject, IRequest
    {
        public static InspectorFindPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string Keyword { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.Keyword = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorFindPropertyResponse)]
    public partial class InspectorFindPropertyResponse : MessageObject, IResponse
    {
        public static InspectorFindPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public string Keyword { get; set; }

        [MemoryPackOrder(96)]
        public int Count { get; set; }

        [MemoryPackOrder(97)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(98)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.ComponentName = default;
            this.Keyword = default;
            this.Count = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorSetPropertyRequest)]
    [ResponseType(nameof(InspectorSetPropertyResponse))]
    public partial class InspectorSetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public string PropertyName { get; set; }

        [MemoryPackOrder(98)]
        public BridgePropertyInfo Value { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.PropertyName = default;
            this.Value = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorSetPropertyResponse)]
    public partial class InspectorSetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public bool Changed { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Changed = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorSetPropertiesRequest)]
    [ResponseType(nameof(InspectorSetPropertiesResponse))]
    public partial class InspectorSetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        [MemoryPackOrder(97)]
        public List<BridgePropertyInfo> Values { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;
            this.Values.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorSetPropertiesResponse)]
    public partial class InspectorSetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string TargetName { get; set; }

        [MemoryPackOrder(93)]
        public string TargetType { get; set; }

        [MemoryPackOrder(94)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(95)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(96)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(97)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(98)]
        public bool Changed { get; set; }

        [MemoryPackOrder(99)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TargetName = default;
            this.TargetType = default;
            this.GameObjectName = default;
            this.ComponentName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.Changed = default;
            this.Properties.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorAddComponentRequest)]
    [ResponseType(nameof(InspectorAddComponentResponse))]
    public partial class InspectorAddComponentRequest : MessageObject, IRequest
    {
        public static InspectorAddComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string TypeName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.TypeName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorAddComponentResponse)]
    public partial class InspectorAddComponentResponse : MessageObject, IResponse
    {
        public static InspectorAddComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public BridgeComponentInfo AddedComponent { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.AddedComponent = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorRemoveComponentRequest)]
    [ResponseType(nameof(InspectorRemoveComponentResponse))]
    public partial class InspectorRemoveComponentRequest : MessageObject, IRequest
    {
        public static InspectorRemoveComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public string Path { get; set; }

        [MemoryPackOrder(91)]
        public int InstanceId { get; set; }

        [MemoryPackOrder(92)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(93)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(94)]
        public string ComponentName { get; set; }

        [MemoryPackOrder(95)]
        public int ComponentIndex { get; set; }

        [MemoryPackOrder(96)]
        public int ComponentInstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.InstanceId = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.ComponentName = default;
            this.ComponentIndex = default;
            this.ComponentInstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(UBridgeInsp.InspectorRemoveComponentResponse)]
    public partial class InspectorRemoveComponentResponse : MessageObject, IResponse
    {
        public static InspectorRemoveComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(92)]
        public string GameObjectName { get; set; }

        [MemoryPackOrder(93)]
        public string AssetPath { get; set; }

        [MemoryPackOrder(94)]
        public string ObjectPath { get; set; }

        [MemoryPackOrder(95)]
        public BridgeComponentInfo RemovedComponent { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.GameObjectName = default;
            this.AssetPath = default;
            this.ObjectPath = default;
            this.RemovedComponent = default;

            ObjectPool.Recycle(this);
        }
    }

    public static class UBridgeInsp
    {
        public const ushort BridgePropertyInfo = 51001;
        public const ushort InspectorGetComponentsRequest = 51002;
        public const ushort InspectorGetComponentsResponse = 51003;
        public const ushort InspectorGetPropertiesRequest = 51004;
        public const ushort InspectorGetPropertiesResponse = 51005;
        public const ushort InspectorGetPropertyRequest = 51006;
        public const ushort InspectorGetPropertyResponse = 51007;
        public const ushort InspectorFindPropertyRequest = 51008;
        public const ushort InspectorFindPropertyResponse = 51009;
        public const ushort InspectorSetPropertyRequest = 51010;
        public const ushort InspectorSetPropertyResponse = 51011;
        public const ushort InspectorSetPropertiesRequest = 51012;
        public const ushort InspectorSetPropertiesResponse = 51013;
        public const ushort InspectorAddComponentRequest = 51014;
        public const ushort InspectorAddComponentResponse = 51015;
        public const ushort InspectorRemoveComponentRequest = 51016;
        public const ushort InspectorRemoveComponentResponse = 51017;
    }
}