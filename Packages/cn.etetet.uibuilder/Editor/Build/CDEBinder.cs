using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET.UIBuilder
{
    /// <summary>
    /// CDE 表绑定（迁移自 ubridge UBridgeCDEHandlers）：
    /// C 表：u_Com* 节点 → ComponentTable.EditorAddComponent
    /// E 表：events → EventTable.EditorAddEvent + 目标节点挂 UIEventBind/UITaskEventBind 组件
    /// 全部在内存树上完成后才落盘（事务性由 BuildPipeline 保证）。
    /// </summary>
    public static class CDEBinder
    {
        /// <summary>bind.component 名 → 类型（SpecSchema.TypeBindComponents 的值域）</summary>
        private static readonly Dictionary<string, Type> BindComponentTypes = new Dictionary<string, Type>
        {
            { "RectTransform", typeof(RectTransform) },
            { "Text", typeof(Text) },
            { "TextMeshProUGUI", typeof(TextMeshProUGUI) },
            { "Image", typeof(Image) },
            { "Button", typeof(Button) },
            { "InputField", typeof(InputField) },
            { "Toggle", typeof(Toggle) },
            { "Slider", typeof(Slider) },
            { "Dropdown", typeof(Dropdown) },
            { "ScrollRect", typeof(ScrollRect) },
            { "LoopVerticalScrollRect", typeof(LoopVerticalScrollRect) },
            { "LoopHorizontalScrollRect", typeof(LoopHorizontalScrollRect) },
        };

        /// <summary>类型默认绑定（缓存 SpecSchema 查询结果为 Type）</summary>
        private static readonly Dictionary<string, Type> TypeDefaultBind = BuildTypeDefaultBind();

        public static void Bind(GameObject root, UISpec spec, BuildResult result)
        {
            UIBindCDETable cde = root.GetComponent<UIBindCDETable>();
            if (cde == null)
            {
                result.Errors.Add("根节点缺少 UIBindCDETable（PanelAssembler 应已添加）");
                return;
            }

            // 确保子表存在（与 ubridge CDEHelper 行为一致）
            if (!cde.ComponentTable)
                cde.ComponentTable = root.AddComponent<UIBindComponentTable>();
            if (!cde.EventTable)
                cde.EventTable = root.AddComponent<UIBindEventTable>();

            var nameToTransform = new Dictionary<string, Transform>();
            CollectTransforms(root.transform, nameToTransform);

            // C 表
            foreach (NodeSpec node in WalkNodes(spec))
            {
                if (!node.IsBound)
                    continue;

                if (!nameToTransform.TryGetValue(node.Name, out Transform nodeTransform))
                {
                    result.Errors.Add($"C 表绑定失败：节点 '{node.Name}' 不存在于构建树");
                    continue;
                }

                string typeName = node.BindComponent
                    ?? (SpecSchema.TypeBindComponents.TryGetValue(node.Type, out string defaultName) ? defaultName : null);
                if (string.IsNullOrEmpty(typeName) || !BindComponentTypes.TryGetValue(typeName, out Type componentType))
                {
                    result.Errors.Add($"C 表绑定失败：'{node.Name}' 的绑定组件类型无法解析（{typeName ?? "无"}）");
                    continue;
                }

                Component component = nodeTransform.GetComponent(componentType);
                if (component == null)
                {
                    result.Errors.Add($"C 表绑定失败：'{node.Name}' 上没有组件 {componentType.Name}");
                    continue;
                }

                cde.ComponentTable.EditorAddComponent(component, node.Name);
            }

            // E 表 + 事件组件挂载
            foreach (EventSpec evt in spec.Events)
            {
                var paramTypes = new List<EUIEventParamType>();
                foreach (string param in evt.Params)
                {
                    if (Enum.TryParse(param, out EUIEventParamType parsed))
                        paramTypes.Add(parsed);
                }

                // ClickDown/ClickUp 只有 Sync 组件（无 Task 变体），强制走 Sync 通道
            bool forceSync = evt.Trigger == "ClickDown" || evt.Trigger == "ClickUp";
            var eventType = (evt.Sync || forceSync)
                    ? UIBindEventTable.EUITaskEventType.Sync
                    : UIBindEventTable.EUITaskEventType.Async;

                var uiEvent = cde.EventTable.EditorAddEvent(eventType, evt.Name, paramTypes);
                if (uiEvent == null)
                {
                    result.Errors.Add($"E 表事件创建失败：'{evt.Name}'");
                    continue;
                }

                if (!nameToTransform.TryGetValue(evt.Target, out Transform target))
                {
                    result.Errors.Add($"事件挂载失败：'{evt.Name}' 的 target '{evt.Target}' 不存在");
                    continue;
                }

                AttachEventBind(target.gameObject, uiEvent, evt);
            }
        }

        /// <summary>挂载事件绑定组件（迁移自 ubridge YIUIAttachEvent：含同事件去重 + m_EventName 反射写入）</summary>
        private static void AttachEventBind(GameObject target, UIEventBase uiEvent, EventSpec evt)
        {
            Type bindType = GetBindComponentType(uiEvent.AllEventParamType, uiEvent.IsTaskEvent, evt.Trigger);
            if (bindType == null)
                throw new InvalidOperationException($"无法确定事件 '{evt.Name}' 的绑定组件类型");

            FieldInfo eventNameField = bindType.GetField("m_EventName",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // 同节点上同事件的旧组件先移除
            if (eventNameField != null)
            {
                foreach (UIEventBind existing in target.GetComponents<UIEventBind>())
                {
                    if ((string)eventNameField.GetValue(existing) == evt.Name)
                    {
                        UnityEngine.Object.DestroyImmediate(existing);
                        break;
                    }
                }
            }

            Component bindComponent = target.AddComponent(bindType);
            if (eventNameField != null)
                eventNameField.SetValue(bindComponent, evt.Name);
        }

        /// <summary>根据参数/同步性/触发方式确定绑定组件类型（迁移自 ubridge）</summary>
        private static Type GetBindComponentType(List<EUIEventParamType> paramTypes, bool isTaskEvent, string triggerType)
        {
            if (triggerType == "ClickDown")
                return typeof(UIEventBindClickDown);
            if (triggerType == "ClickUp")
                return typeof(UIEventBindClickUp);


            bool hasParams = paramTypes != null && paramTypes.Count > 0;
            EUIEventParamType firstParam = hasParams ? paramTypes[0] : EUIEventParamType.Bool; // Bool 作为"无参"哨兵

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

            return firstParam switch
            {
                EUIEventParamType.Int => typeof(UIEventBindClickInt),
                EUIEventParamType.String => typeof(UIEventBindClickString),
                EUIEventParamType.Object or EUIEventParamType.ParamVo => typeof(UIEventBindClickPointerEventData),
                _ => typeof(UIEventBindClick),
            };
        }

        private static Dictionary<string, Type> BuildTypeDefaultBind()
        {
            var map = new Dictionary<string, Type>();
            foreach (KeyValuePair<string, string> kv in SpecSchema.TypeBindComponents)
            {
                if (kv.Value != null && BindComponentTypes.TryGetValue(kv.Value, out Type type))
                    map[kv.Key] = type;
            }

            return map;
        }

        private static void CollectTransforms(Transform parent, Dictionary<string, Transform> map)
        {
            foreach (Transform child in parent)
            {
                map[child.name] = child;
                CollectTransforms(child, map);
            }
        }

        private static IEnumerable<NodeSpec> WalkNodes(UISpec spec)
        {
            foreach (NodeSpec node in spec.Nodes)
            {
                yield return node;
                foreach (NodeSpec child in WalkChildren(node))
                    yield return child;
            }
        }

        private static IEnumerable<NodeSpec> WalkChildren(NodeSpec node)
        {
            foreach (NodeSpec child in node.Children)
            {
                yield return child;
                foreach (NodeSpec grandChild in WalkChildren(child))
                    yield return grandChild;
            }
        }
    }
}
