using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>目标对象解析上下文：自动处理场景对象 vs Prefab 资产</summary>
    [EnableClass]
    class InspectorContext : IDisposable
    {
        public GameObject GameObject;
        public string AssetPath;
        public string ObjectPath;
        public bool IsSceneObject;
        private GameObject m_PrefabContentsRoot;
        public void Dispose() { if (m_PrefabContentsRoot) PrefabUtility.UnloadPrefabContents(m_PrefabContentsRoot); }
        public static InspectorContext Resolve(string path, int instanceId, string assetPath, string objectPath)
        {
            var ctx = new InspectorContext();
            // 1. 通过 InstanceId 查找场景对象
            if (instanceId != 0) { var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject; if (go && go.scene.IsValid()) { ctx.GameObject = go; ctx.IsSceneObject = true; return ctx; } }
            // 2. 通过路径查找场景对象
            if (!string.IsNullOrEmpty(path)) { var go = GameObject.Find(path); if (go) { ctx.GameObject = go; ctx.IsSceneObject = true; return ctx; } }
            // 3. 加载 Prefab 资产
            if (!string.IsNullOrEmpty(assetPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab) { ctx.m_PrefabContentsRoot = PrefabUtility.LoadPrefabContents(assetPath); ctx.GameObject = ctx.m_PrefabContentsRoot; ctx.AssetPath = assetPath; return ctx; }
            }
            // 4. 使用当前选中
            var sel = Selection.activeGameObject;
            if (sel) { ctx.GameObject = sel; ctx.IsSceneObject = sel.scene.IsValid(); return ctx; }
            return ctx;
        }
    }

    /// <summary>Inspector 命令共享工具</summary>
    static class InspectorHelper
    {
        public static BridgeComponentInfo MakeComponentInfo(Component comp, int index)
        {
            var c = BridgeComponentInfo.Create();
            c.Type = comp.GetType().Name; c.Data = "";
            return c;
        }
        public static BridgePropertyInfo MakePropertyInfo(SerializedProperty prop)
        {
            var p = BridgePropertyInfo.Create();
            p.Name = prop.name; p.DisplayName = prop.displayName; p.Type = prop.type;
            p.PropertyPath = prop.propertyPath; p.IsArray = prop.isArray;
            p.IsEditable = prop.editable; p.IsExpanded = prop.isExpanded;
            p.HasChildren = prop.hasChildren; p.Depth = prop.depth;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: p.IntValue = prop.intValue; break;
                case SerializedPropertyType.Float: p.FloatValue = prop.floatValue; break;
                case SerializedPropertyType.Boolean: p.BoolValue = prop.boolValue; break;
                case SerializedPropertyType.String: p.StringValue = prop.stringValue ?? ""; break;
                case SerializedPropertyType.Vector2: p.Vector2Value = V2(prop.vector2Value); break;
                case SerializedPropertyType.Vector3: p.Vector3Value = V3(prop.vector3Value); break;
                case SerializedPropertyType.ObjectReference:
                    if (prop.objectReferenceValue) { p.ObjectReferencePath = AssetDatabase.GetAssetPath(prop.objectReferenceValue); p.ObjectReferenceType = prop.objectReferenceValue.GetType().Name; }
                    break;
            }
            return p;
        }
        public static bool TrySetPropertyValue(SerializedProperty prop, BridgePropertyInfo val, out string err)
        {
            err = null;
            try
            {
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Integer: prop.intValue = val.IntValue; break;
                    case SerializedPropertyType.Float: prop.floatValue = val.FloatValue; break;
                    case SerializedPropertyType.Boolean: prop.boolValue = val.BoolValue; break;
                    case SerializedPropertyType.String: prop.stringValue = val.StringValue ?? ""; break;
                    case SerializedPropertyType.Vector2: prop.vector2Value = new Vector2(val.Vector2Value?.X ?? 0, val.Vector2Value?.Y ?? 0); break;
                    case SerializedPropertyType.Vector3: prop.vector3Value = new Vector3(val.Vector3Value?.X ?? 0, val.Vector3Value?.Y ?? 0, val.Vector3Value?.Z ?? 0); break;
                    case SerializedPropertyType.ObjectReference:
                        if (!string.IsNullOrEmpty(val.ObjectReferencePath))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(val.ObjectReferencePath);
                            if (obj) prop.objectReferenceValue = obj;
                            else { err = "Object not found: " + val.ObjectReferencePath; return false; }
                        }
                        break;
                    default: err = $"Unsupported type: {prop.propertyType}"; return false;
                }
                return true;
            }
            catch (Exception e) { err = e.Message; return false; }
        }
        public static Component ResolveComponent(GameObject go, string name, int index, int instId)
        {
            var comps = go.GetComponents<Component>();
            if (instId != 0) { foreach (var c in comps) if (c.GetInstanceID() == instId) return c; }
            if (!string.IsNullOrEmpty(name)) { foreach (var c in comps) if (c.GetType().Name == name) return c; }
            if (index >= 0 && index < comps.Length) return comps[index];
            return comps.Length > 0 ? comps[0] : null;
        }
        static BridgeVector2 V2(Vector2 v) { var r = BridgeVector2.Create(); r.X = v.x; r.Y = v.y; return r; }
        static BridgeVector3 V3(Vector3 v) { var r = BridgeVector3.Create(); r.X = v.x; r.Y = v.y; r.Z = v.z; return r; }
    }

    // ================== 8 Inspector Handlers ==================

    public static class UBridgeInspectorGetComponentsHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorGetComponentsRequest>(p);
            var resp = InspectorGetComponentsResponse.Create();
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comps = ctx.GameObject.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++) if (comps[i]) resp.Components.Add(InspectorHelper.MakeComponentInfo(comps[i], i));
            resp.GameObjectName = ctx.GameObject.name; resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath; resp.Count = resp.Components.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorGetPropertiesHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorGetPropertiesRequest>(p);
            var resp = InspectorGetPropertiesResponse.Create();
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            var target = (comp as UnityEngine.Object) ?? (UnityEngine.Object)ctx.GameObject;
            var so = new SerializedObject(target);
            var it = so.GetIterator(); bool enter = true;
            while (it.NextVisible(enter)) { enter = r?.IncludeChildren ?? false; resp.Properties.Add(InspectorHelper.MakePropertyInfo(it)); }
            resp.TargetName = target.name; resp.TargetType = target.GetType().Name;
            resp.GameObjectName = ctx.GameObject.name; resp.ComponentName = comp?.GetType().Name ?? ""; resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorGetPropertyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorGetPropertyRequest>(p);
            var resp = InspectorGetPropertyResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.PropertyName)) { resp.Error = 3; resp.Message = "PropertyName required"; return UBridgeJsonHelper.ToJson(resp); }
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            var target = (comp as UnityEngine.Object) ?? (UnityEngine.Object)ctx.GameObject;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(r.PropertyName);
            if (prop == null) { resp.Error = 3; resp.Message = "Property not found: " + r.PropertyName; return UBridgeJsonHelper.ToJson(resp); }
            resp.TargetName = target.name; resp.TargetType = target.GetType().Name;
            resp.ComponentName = comp?.GetType().Name ?? ""; resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            resp.Property = InspectorHelper.MakePropertyInfo(prop);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorFindPropertyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorFindPropertyRequest>(p);
            var resp = InspectorFindPropertyResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.Keyword)) { resp.Error = 3; resp.Message = "Keyword required"; return UBridgeJsonHelper.ToJson(resp); }
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            var target = (comp as UnityEngine.Object) ?? (UnityEngine.Object)ctx.GameObject;
            var so = new SerializedObject(target);
            var it = so.GetIterator(); bool enter = true; string kw = r.Keyword;
            while (it.NextVisible(enter))
            {
                enter = true;
                if (it.propertyPath.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    it.name.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    it.displayName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    resp.Properties.Add(InspectorHelper.MakePropertyInfo(it));
            }
            resp.TargetName = target.name; resp.TargetType = target.GetType().Name;
            resp.ComponentName = comp?.GetType().Name ?? ""; resp.Keyword = kw; resp.Count = resp.Properties.Count;
            resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorSetPropertyHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorSetPropertyRequest>(p);
            var resp = InspectorSetPropertyResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.PropertyName) || r?.Value == null) { resp.Error = 3; resp.Message = "PropertyName+Value required"; return UBridgeJsonHelper.ToJson(resp); }
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            var target = (comp as UnityEngine.Object) ?? (UnityEngine.Object)ctx.GameObject;
            var so = new SerializedObject(target); so.Update();
            var prop = so.FindProperty(r.PropertyName);
            if (prop == null) { resp.Error = 3; resp.Message = "Property not found: " + r.PropertyName; return UBridgeJsonHelper.ToJson(resp); }
            if (!prop.editable) { resp.Error = 3; resp.Message = "Not editable: " + r.PropertyName; return UBridgeJsonHelper.ToJson(resp); }
            if (ctx.IsSceneObject) Undo.RecordObject(target, "Set Property");
            if (!InspectorHelper.TrySetPropertyValue(prop, r.Value, out string err)) { resp.Error = 3; resp.Message = err; return UBridgeJsonHelper.ToJson(resp); }
            resp.Changed = so.ApplyModifiedProperties();
            if (resp.Changed && !ctx.IsSceneObject) EditorUtility.SetDirty(target);
            resp.TargetName = target.name; resp.TargetType = target.GetType().Name;
            resp.GameObjectName = ctx.GameObject.name; resp.ComponentName = comp?.GetType().Name ?? "";
            resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            resp.Properties.Add(InspectorHelper.MakePropertyInfo(prop));
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorSetPropertiesHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorSetPropertiesRequest>(p);
            var resp = InspectorSetPropertiesResponse.Create();
            if (r?.Values == null || r.Values.Count == 0) { resp.Error = 3; resp.Message = "Values required"; return UBridgeJsonHelper.ToJson(resp); }
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            var target = (comp as UnityEngine.Object) ?? (UnityEngine.Object)ctx.GameObject;
            var so = new SerializedObject(target); so.Update();
            if (ctx.IsSceneObject) Undo.RecordObject(target, "Set Properties");
            foreach (var val in r.Values)
            {
                var propPath = !string.IsNullOrWhiteSpace(val.PropertyPath) ? val.PropertyPath : val.Name;
                var prop = so.FindProperty(propPath);
                if (prop == null || !prop.editable) { resp.Error = 3; resp.Message = "Property error: " + propPath; return UBridgeJsonHelper.ToJson(resp); }
                if (!InspectorHelper.TrySetPropertyValue(prop, val, out string err)) { resp.Error = 3; resp.Message = err; return UBridgeJsonHelper.ToJson(resp); }
                resp.Properties.Add(InspectorHelper.MakePropertyInfo(prop));
            }
            resp.Changed = so.ApplyModifiedProperties();
            if (resp.Changed && !ctx.IsSceneObject) EditorUtility.SetDirty(target);
            resp.TargetName = target.name; resp.TargetType = target.GetType().Name;
            resp.GameObjectName = ctx.GameObject.name; resp.ComponentName = comp?.GetType().Name ?? "";
            resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorAddComponentHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorAddComponentRequest>(p);
            var resp = InspectorAddComponentResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.TypeName)) { resp.Error = 3; resp.Message = "TypeName required"; return UBridgeJsonHelper.ToJson(resp); }
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var compType = Type.GetType(r.TypeName) ?? Type.GetType(r.TypeName + ", UnityEngine") ?? Type.GetType("UnityEngine." + r.TypeName + ", UnityEngine");
            if (compType == null || !typeof(Component).IsAssignableFrom(compType)) { resp.Error = 3; resp.Message = "Component type not found: " + r.TypeName; return UBridgeJsonHelper.ToJson(resp); }
            Component added = ctx.IsSceneObject ? Undo.AddComponent(ctx.GameObject, compType) : ctx.GameObject.AddComponent(compType);
            if (!ctx.IsSceneObject) EditorUtility.SetDirty(ctx.GameObject);
            resp.GameObjectName = ctx.GameObject.name; resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            resp.AddedComponent = InspectorHelper.MakeComponentInfo(added, 0);
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeInspectorRemoveComponentHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<InspectorRemoveComponentRequest>(p);
            var resp = InspectorRemoveComponentResponse.Create();
            using var ctx = InspectorContext.Resolve(r?.Path, r?.InstanceId ?? 0, r?.AssetPath, r?.ObjectPath);
            if (!ctx.GameObject) { resp.Error = 3; resp.Message = "Target not found"; return UBridgeJsonHelper.ToJson(resp); }
            var comp = InspectorHelper.ResolveComponent(ctx.GameObject, r?.ComponentName, r?.ComponentIndex ?? 0, r?.ComponentInstanceId ?? 0);
            if (!comp) { resp.Error = 3; resp.Message = "Component not found"; return UBridgeJsonHelper.ToJson(resp); }
            if (comp is Transform) { resp.Error = 3; resp.Message = "Cannot remove Transform"; return UBridgeJsonHelper.ToJson(resp); }
            resp.GameObjectName = ctx.GameObject.name; resp.AssetPath = ctx.AssetPath; resp.ObjectPath = ctx.ObjectPath;
            resp.RemovedComponent = InspectorHelper.MakeComponentInfo(comp, 0);
            if (ctx.IsSceneObject) Undo.DestroyObjectImmediate(comp);
            else { UnityEngine.Object.DestroyImmediate(comp, true); EditorUtility.SetDirty(ctx.GameObject); }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}