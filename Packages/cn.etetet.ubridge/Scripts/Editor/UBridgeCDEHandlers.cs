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
        /// <summary>命名前缀 → 目标组件类型</summary>
        [StaticField]
        private static readonly Dictionary<string, System.Type> s_PrefixToComponentType = new()
        {
            {"Btn",    typeof(Button)},
            {"Txt",    typeof(Text)},
            {"Img",    typeof(Image)},
            {"RawImg", typeof(RawImage)},
            {"Input",  typeof(InputField)},
            {"Tog",    typeof(Toggle)},
            {"Sld",    typeof(Slider)},
            {"Scroll", typeof(ScrollRect)},
            {"Drop",   typeof(Dropdown)},
            {"Bar",    typeof(Scrollbar)},
            {"Panel",  typeof(Image)},
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

                // 1) 按下划线拆分首段 → 命中前缀映射表 → 精确 GetComponent
                Component component = null;
                var parts = controlName.Split('_');
                if (parts.Length > 0 && s_PrefixToComponentType.TryGetValue(parts[0], out var targetType))
                {
                    component = child.GetComponent(targetType) as Component;
                }

                // 2) 未命中或取到空 → 默认规则：跳过 Transform/CanvasRenderer，取第一个
                if (!component || component is Transform)
                {
                    var components = child.GetComponents<Component>();
                    component = components.FirstOrDefault(c => c is not Transform && c is not CanvasRenderer)
                             ?? components.FirstOrDefault(c => c is not Transform);
                }

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
                var triggerType = r?.EventTriggerType ?? "Click";
                var bindType = GetBindComponentType(paramTypes, uiEvent.IsTaskEvent, triggerType);
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

        private static Type GetBindComponentType(List<EUIEventParamType> paramTypes, bool isTaskEvent, string triggerType)
        {
            bool hasParams = paramTypes != null && paramTypes.Count > 0;
            EUIEventParamType firstParam = hasParams ? paramTypes[0] : EUIEventParamType.Bool; // Bool as sentinel for "no params"

            // ClickDown / ClickUp only have no-param sync variants
            if (triggerType == "ClickDown")
                return typeof(UIEventBindClickDown);
            if (triggerType == "ClickUp")
                return typeof(UIEventBindClickUp);

            // Click (default) — sync vs async
            if (isTaskEvent)
            {
                return firstParam switch
                {
                    EUIEventParamType.Int => typeof(UITaskEventBindClickInt),
                    EUIEventParamType.String => typeof(UITaskEventBindClickString),
                    EUIEventParamType.Object or EUIEventParamType.ParamVo => typeof(UITaskEventBindClickPointerEventData),
                    _ => typeof(UITaskEventBindClick),
                };
            }
            else
            {
                return firstParam switch
                {
                    EUIEventParamType.Int => typeof(UIEventBindClickInt),
                    EUIEventParamType.String => typeof(UIEventBindClickString),
                    EUIEventParamType.Object or EUIEventParamType.ParamVo => typeof(UIEventBindClickPointerEventData),
                    _ => typeof(UIEventBindClick),
                };
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

    /// <summary>
    /// YIUIClearBindings: 清空 CDE Table 的 C 表或 E 表
    /// --type C  → 清 ComponentTable
    /// --type E  → 清 EventTable
    /// --type All → 全部清空
    /// </summary>
    public static class UBridgeYIUIClearBindingsHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIClearBindingsRequest>(p);
            var resp = YIUIClearBindingsResponse.Create();
            var (root, cde, error) = CDEHelper.LoadPrefab(r?.PrefabPath);
            if (error != null)
            {
                resp.Error = 3; resp.Message = error;
                return UBridgeJsonHelper.ToJson(resp);
            }

            try
            {
                string target = r?.Target?.ToUpper() ?? "";
                int clearedC = 0, clearedE = 0;

                // C 表：反射清 m_AllBindPair → AutoCheck 重建空字典
                if (target == "C" || target == "ALL")
                {
                    if (cde.ComponentTable != null)
                    {
                        var pairField = typeof(UIBindComponentTable).GetField(
                            "m_AllBindPair", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var list = pairField?.GetValue(cde.ComponentTable) as System.Collections.IList;
                        clearedC = list?.Count ?? 0;
                        list?.Clear();
                        cde.ComponentTable.AutoCheck();
                    }
                }

                // E 表：反射清 m_EventDic
                if (target == "E" || target == "ALL")
                {
                    if (cde.EventTable != null)
                    {
                        var dicField = typeof(UIBindEventTable).GetField(
                            "m_EventDic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var dic = dicField?.GetValue(cde.EventTable) as System.Collections.IDictionary;
                        clearedE = dic?.Count ?? 0;
                        dic?.Clear();
                    }
                }

                resp.Message = target switch
                {
                    "C" => $"ComponentTable cleared ({clearedC} entries)",
                    "E" => $"EventTable cleared ({clearedE} entries)",
                    "ALL" => $"Cleared: ComponentTable ({clearedC}) + EventTable ({clearedE})",
                    _ => $"Unknown target '{r?.Target}'. Use: C, E, All"
                };
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    /// <summary>
    /// YIUIRemoveControl: 按名称删除预制体中的子控件
    /// </summary>
    public static class UBridgeYIUIRemoveControlHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<YIUIRemoveControlRequest>(p);
            var resp = YIUIRemoveControlResponse.Create();
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
                    resp.Error = 3; resp.Message = $"Control not found: {controlName}";
                    return UBridgeJsonHelper.ToJson(resp);
                }

                UnityEngine.Object.DestroyImmediate(child.gameObject);
                resp.Message = $"Removed: {controlName}";
            }
            finally { CDEHelper.SaveAndUnload(r.PrefabPath, root); }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
