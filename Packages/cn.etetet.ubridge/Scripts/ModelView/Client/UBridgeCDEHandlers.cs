using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET
{
    /// <summary>
    /// CDE Table Handler 基类：加载/保存 prefab 的公共逻辑
    /// </summary>
    internal static class CDEHelper
    {
        public static (GameObject root, UIBindCDETable cde, string error) LoadPrefab(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (null, null, "PrefabPath required");
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset)
                return (null, null, $"Prefab not found: {path}");
            var root = PrefabUtility.LoadPrefabContents(path);
            var cde = root?.GetComponent<UIBindCDETable>();
            if (!cde)
                return (root, null, "CDE Table not found on prefab root");
            // 确保子表已初始化
            if (!cde.ComponentTable)
                cde.ComponentTable = root.AddComponent<UIBindComponentTable>();
            if (!cde.EventTable)
                cde.EventTable = root.AddComponent<UIBindEventTable>();
            return (root, cde, null);
        }

        public static void SaveAndUnload(string path, GameObject root)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.Refresh();
        }

        public static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
            foreach (Transform child in parent)
            {
                var found = FindChildRecursive(child, name);
                if (found) return found;
            }
            return null;
        }
    }

    /// <summary>
    /// YIUIGetBindings: 读取所有组件绑定
    /// </summary>
    public static class UBridgeYIUIGetBindingsHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIGetBindingsRequest>(p);
            var resp = YIUIGetBindingsResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                var bindings = new List<YIUIBindingInfo>();
                foreach (var kv in cde.ComponentTable.AllBindDic)
                {
                    var info = YIUIBindingInfo.Create();
                    info.Name = kv.Key;
                    info.ComponentType = kv.Value?.GetType().Name ?? "null";
                    info.ComponentName = kv.Value?.name ?? "null";
                    bindings.Add(info);
                }
                resp.Bindings.AddRange(bindings);
                resp.Count = bindings.Count;
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIGetEvents: 读取所有事件定义
    /// </summary>
    public static class UBridgeYIUIGetEventsHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIGetEventsRequest>(p);
            var resp = YIUIGetEventsResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                var events = new List<YIUIEventItem>();
                foreach (var kv in cde.EventTable.EventDic)
                {
                    var evt = kv.Value;
                    var paramStrs = evt.AllEventParamType?.Select(t => t.ToString()).ToList() ?? new List<string>();
                    var item = YIUIEventItem.Create();
                    item.EventName = kv.Key;
                    item.EventType = evt.IsTaskEvent ? "Async" : "Sync";
                    item.ParamTypes = string.Join(",", paramStrs);
                    events.Add(item);
                }
                resp.Events.AddRange(events);
                resp.Count = events.Count;
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIBindComponent: 绑定子控件到 CDE Table
    /// </summary>
    public static class UBridgeYIUIBindComponentHandler
    {
        [StaticField]
        private static readonly HashSet<string> s_UITypes = new()
        {
            typeof(Button).Name,
            typeof(Text).Name,
            typeof(Image).Name,
            typeof(RawImage).Name,
            typeof(Toggle).Name,
            typeof(Slider).Name,
            typeof(Scrollbar).Name,
        };

        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIBindComponentRequest>(p);
            var resp = YIUIBindComponentResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                string controlName = r?.ControlName ?? "";
                if (string.IsNullOrWhiteSpace(controlName))
                {
                    resp.Error = 3; resp.Message = "ControlName required";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                var child = CDEHelper.FindChildRecursive(root.transform, controlName);
                if (!child)
                {
                    resp.Error = 3; resp.Message = $"Child not found: {controlName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                // 优先匹配 Unity UI 组件（通过 typeof 获取名称，避免硬编码字符串）
                var components = child.GetComponents<Component>();
                var component = components.FirstOrDefault(c => s_UITypes.Contains(c.GetType().Name))
                             ?? components.FirstOrDefault(c => c is not Transform);
                if (!component || component is Transform)
                {
                    resp.Error = 3; resp.Message = $"No bindable component found on: {controlName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                string bindName = !string.IsNullOrWhiteSpace(r.BindName) ? r.BindName : "";
                cde.ComponentTable.EditorAddComponent(component, bindName);
                resp.Message = $"Bound {component.GetType().Name} as '{bindName}'";
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIBindEvent: 在 CDE EventTable 中创建事件定义
    /// </summary>
    public static class UBridgeYIUIBindEventHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIBindEventRequest>(p);
            var resp = YIUIBindEventResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                string eventName = r?.EventName ?? "";
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    resp.Error = 3; resp.Message = "EventName required";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                var eventType = r?.EventType?.ToLower() == "sync"
                    ? UIBindEventTable.EUITaskEventType.Sync
                    : UIBindEventTable.EUITaskEventType.Async;

                // 解析参数类型
                var paramTypeList = new List<EUIEventParamType>();
                if (!string.IsNullOrWhiteSpace(r?.ParamTypes))
                {
                    foreach (var pt in r.ParamTypes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (Enum.TryParse<EUIEventParamType>(pt.Trim(), true, out var ept))
                            paramTypeList.Add(ept);
                    }
                }

                var uiEvent = cde.EventTable.EditorAddEvent(eventType, eventName, paramTypeList);
                if (uiEvent == null)
                {
                    resp.Error = 3; resp.Message = $"Failed to create event: {eventName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                resp.Message = $"Created event '{eventName}' ({r?.EventType ?? "Async"}, params: {r?.ParamTypes ?? ""})";
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIAttachEvent: 将事件挂载到指定控件
    /// </summary>
    public static class UBridgeYIUIAttachEventHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIAttachEventRequest>(p);
            var resp = YIUIAttachEventResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                string targetName = r?.TargetName ?? "";
                string eventName = r?.EventName ?? "";
                if (string.IsNullOrWhiteSpace(targetName) || string.IsNullOrWhiteSpace(eventName))
                {
                    resp.Error = 3; resp.Message = "TargetName and EventName required";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                // 查找事件定义
                if (!cde.EventTable.EventDic.TryGetValue(eventName, out var uiEvent))
                {
                    resp.Error = 3; resp.Message = $"Event not found: {eventName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                // 查找目标控件
                var target = CDEHelper.FindChildRecursive(root.transform, targetName);
                if (!target)
                {
                    resp.Error = 3; resp.Message = $"Target not found: {targetName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                // 根据事件参数类型确定 UIEventBind 组件类型
                var paramTypes = uiEvent.AllEventParamType;
                var bindType = GetBindComponentType(paramTypes, uiEvent.IsTaskEvent);
                if (bindType == null)
                {
                    resp.Error = 3; resp.Message = $"Cannot determine bind component for event: {eventName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                // 检查是否已经挂载过相同事件 → 移除旧的
                var field = bindType.GetField("m_EventName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    foreach (var c in target.gameObject.GetComponents<UIEventBind>())
                    {
                        if ((string)field.GetValue(c) == eventName)
                        {
                            UnityEngine.Object.DestroyImmediate(c);
                            break;
                        }
                    }
                }

                var bindComp = target.gameObject.AddComponent(bindType);
                if (field != null)
                    field.SetValue(bindComp, eventName);

                resp.Message = $"Attached {bindType.Name} to '{targetName}' with event '{eventName}'";
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }

        private static Type GetBindComponentType(List<EUIEventParamType> paramTypes, bool isTaskEvent)
        {
            // YIUI 的 UIEventBind 类型映射（简化版，按第一个参数类型匹配）
            if (paramTypes == null || paramTypes.Count == 0)
            {
                return typeof(UIEventBindClick);
            }

            switch (paramTypes[0])
            {
                case EUIEventParamType.Int:
                    return typeof(UIEventBindClickInt);
                case EUIEventParamType.String:
                    return typeof(UIEventBindClickString);
                case EUIEventParamType.Object:
                case EUIEventParamType.ParamVo:
                    return typeof(UIEventBindClickPointerEventData);
                default:
                    // Bool, Float 及其他无精确匹配 → 用基础 Click 类型
                    return typeof(UIEventBindClick);
            }
        }
    }

    /// <summary>
    /// YIUIGenerateCode: 根据 CDE Table 生成代码文件
    /// 反射调用 UICreateModule.CreatePackages
    /// </summary>
    public static class UBridgeYIUIGenerateCodeHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIGenerateCodeRequest>(p);
            var resp = YIUIGenerateCodeResponse.Create();

            string path = r?.PrefabPath ?? "";
            if (string.IsNullOrWhiteSpace(path))
            {
                resp.Error = 3; resp.Message = "PrefabPath required";
                return UBridgeJsonHelper.ToJson(resp);
            }

            // 必须用 AssetDatabase 加载（非 LoadPrefabContents），
            // 因为 UICreateModule 会检查 IsPartOfPrefabAsset
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefabAsset)
            {
                resp.Error = 3; resp.Message = $"Prefab not found: {path}";
                return UBridgeJsonHelper.ToJson(resp);
            }
            var cde = prefabAsset.GetComponent<UIBindCDETable>();
            if (!cde)
            {
                resp.Error = 3; resp.Message = "CDE Table not found on prefab root";
                return UBridgeJsonHelper.ToJson(resp);
            }

            string packageName = r?.PackageName ?? "";
            try
            {
                var moduleType = System.Reflection.Assembly.Load("ET.YIUIFramework.Editor")
                    ?.GetType("YIUIFramework.Editor.UICreateModule");
                var method = moduleType?.GetMethod("CreatePackages",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null)
                {
                    resp.Error = 3; resp.Message = "UICreateModule.CreatePackages not found";
                    return UBridgeJsonHelper.ToJson(resp);
                }
                method.Invoke(null, new object[] { cde, true, false, packageName });
                resp.Message = $"Code generated: package='{packageName}'";
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                resp.Error = 3; resp.Message = $"GenerateCode failed: {ex.InnerException?.Message ?? ex.Message}";
            }
            catch (System.Exception ex)
            {
                resp.Error = 3; resp.Message = $"GenerateCode error: {ex.Message}";
            }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
