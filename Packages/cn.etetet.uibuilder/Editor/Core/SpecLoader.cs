using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ET.UIBuilder
{
    /// <summary>
    /// spec 加载与校验（lint 规则 §3.8.1-5 + §3.8.7；规则 6 绑定一致性属开发机离线 lint，不在此）。
    /// 流程：读文件 → YamlDotNet 泛型反序列化（对象树）→ 手写绑定 + 逐项校验（精确路径定位）。
    /// 错误全部收集后一次性返回，不遇错即弃。
    /// </summary>
    public static class SpecLoader
    {
        private static readonly Regex ColorRegex = new Regex("^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$", RegexOptions.Compiled);

        /// <summary>Unity 工程根（Assets 的上级）</summary>
        public static string ProjectRoot { get; } = Directory.GetParent(Application.dataPath)!.FullName;

        /// <summary>
        /// 加载并校验一个 spec。无论校验结果如何都尽量返回已填充的 spec（错误字段取默认值），
        /// 以便调用方在部分失败时仍能展示/调试。
        /// </summary>
        public static (UISpec spec, SpecValidationResult result) Load(string path)
        {
            var result = new SpecValidationResult();
            var spec = new UISpec();

            string fullPath = ToProjectPath(path);
            result.SpecPath = path;
            spec.SourcePath = fullPath;
            if (!File.Exists(fullPath))
            {
                result.Error("SPEC_FILE_NOT_FOUND", "", $"文件不存在: {fullPath}");
                return (spec, result);
            }

            object root;
            try
            {
                string text = File.ReadAllText(fullPath);
                // WithAttemptingUnquotedStringTypeDeserialization：
                // YamlDotNet 18 泛型反序列化到 object 时，未加引号的标量默认全是 string；
                // 开启后 10/-40/true 等恢复自然类型（int/double/bool），加引号的恒为 string。
                var deserializer = new DeserializerBuilder()
                    .WithAttemptingUnquotedStringTypeDeserialization()
                    .Build();
                root = deserializer.Deserialize(new StringReader(text));
            }
            catch (YamlException ex)
            {
                result.Error("SPEC_YAML_SYNTAX", "", $"YAML 语法错误: {UnwrapYamlMessage(ex)}");
                return (spec, result);
            }
            catch (Exception ex)
            {
                result.Error("SPEC_READ_FAIL", "", $"读取失败: {ex.Message}");
                return (spec, result);
            }

            if (root == null)
            {
                result.Error("SPEC_EMPTY", "", "spec 文件为空");
                return (spec, result);
            }

            Dictionary<string, object> rootDict = AsDict(root);
            if (rootDict == null)
            {
                result.Error("SPEC_ROOT_TYPE", "", $"根节点必须是映射(map)，实际是 {TypeName(root)}");
                return (spec, result);
            }

            foreach (string k in rootDict.Keys)
            {
                if (k != "panel" && k != "nodes" && k != "events")
                    result.Error("SPEC_TOP_UNKNOWN", k, $"未知顶层字段 '{k}'（合法: panel/nodes/events）");
            }

            if (rootDict.TryGetValue("panel", out object panelObj))
            {
                BindPanel(AsDict(panelObj), spec, result);
            }
            else
            {
                result.Error("SPEC_PANEL_MISSING", "panel", "缺少 panel 段");
            }

            if (rootDict.TryGetValue("nodes", out object nodesObj))
            {
                List<object> list = AsList(nodesObj);
                if (list == null)
                {
                    result.Error("SPEC_NODES_TYPE", "nodes", $"nodes 必须是列表，实际是 {TypeName(nodesObj)}");
                }
                else
                {
                    if (list.Count == 0)
                        result.Warn("SPEC_NODES_EMPTY", "nodes", "nodes 为空（面板只有骨架 + UIBlockBG）");
                    for (int i = 0; i < list.Count; i++)
                        spec.Nodes.Add(BindNode(list[i], $"nodes[{i}]", result));
                }
            }
            else
            {
                result.Error("SPEC_NODES_MISSING", "nodes", "缺少 nodes 段");
            }

            if (rootDict.TryGetValue("events", out object eventsObj))
            {
                List<object> list = AsList(eventsObj);
                if (list == null)
                {
                    result.Error("SPEC_EVENTS_TYPE", "events", $"events 必须是列表，实际是 {TypeName(eventsObj)}");
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                        spec.Events.Add(BindEvent(list[i], $"events[{i}]", result));
                }
            }

            // 跨节点检查（§3.8.3 / §3.8.4 / §3.8.7）
            CheckNaming(spec, result);
            CheckEventTargets(spec, result);
            CheckPlaceUnderLayout(spec, result);

            // 资源存在性（§3.8.5）
            CheckResourcePaths(spec, result);

            return (spec, result);
        }

        // ---------------- 绑定 ----------------

        private static void BindPanel(Dictionary<string, object> dict, UISpec spec, SpecValidationResult result)
        {
            if (dict == null)
            {
                result.Error("SPEC_PANEL_TYPE", "panel", "panel 必须是映射(map)");
                return;
            }

            foreach (string k in dict.Keys)
            {
                if (k != "name" && k != "pkg" && k != "layer" && k != "cacheseconds" && k != "blockbg"
                    && k != "stackoption" && k != "priority" && k != "prefabpath")
                    result.Error("SPEC_FIELD_UNKNOWN", $"panel.{k}", $"panel 段未知字段 '{k}'");
            }

            spec.Panel.Name = GetString(dict, "name", "panel.name", result, true);
            spec.Panel.Pkg = GetString(dict, "pkg", "panel.pkg", result, true);
            spec.Panel.Layer = GetEnum(dict, "layer", "panel.layer", SpecSchema.PanelLayers, result, "Panel");
            spec.Panel.CacheSeconds = GetInt(dict, "cacheseconds", "panel.cacheSeconds", result, 0);
            spec.Panel.BlockBg = GetBool(dict, "blockbg", "panel.blockBg", result, true);
            spec.Panel.StackOption = GetEnum(dict, "stackoption", "panel.stackOption", SpecSchema.StackOptions, result, "VisibleTween");
            spec.Panel.Priority = GetInt(dict, "priority", "panel.priority", result, 0);
            spec.Panel.PrefabPath = GetString(dict, "prefabpath", "panel.prefabPath", result);
        }

        private static NodeSpec BindNode(object obj, string path, SpecValidationResult result)
        {
            var node = new NodeSpec();
            Dictionary<string, object> dict = AsDict(obj);
            if (dict == null)
            {
                result.Error("SPEC_NODE_TYPE", path, $"节点必须是映射(map)，实际是 {TypeName(obj)}");
                return node;
            }

            foreach (string k in dict.Keys)
            {
                if (k != "name" && k != "type" && k != "bind" && k != "place" && k != "layout"
                    && k != "props" && k != "image" && k != "style" && k != "children")
                    result.Error("SPEC_FIELD_UNKNOWN", $"{path}.{k}", $"节点未知字段 '{k}'");
            }

            node.Name = GetString(dict, "name", $"{path}.name", result, true);

            // 命名规范（§3.8.4）：u_ 前缀保留给 u_Com
            if (!string.IsNullOrEmpty(node.Name) && node.Name.StartsWith("u_") && !node.IsBound)
                result.Error("SPEC_NAME_PREFIX", $"{path}.name", $"节点名 '{node.Name}'：u_ 前缀保留给 u_Com（C 表绑定节点）");

            node.Type = GetString(dict, "type", $"{path}.type", result, true);
            bool typeKnown = node.Type != null && SpecSchema.TypeProps.ContainsKey(node.Type);
            if (node.Type != null && !typeKnown)
                result.Error("SPEC_TYPE_UNKNOWN", $"{path}.type",
                    $"未知控件类型 '{node.Type}'（合法: {Join(SpecSchema.NodeTypes)}）");

            // bind.component
            if (dict.TryGetValue("bind", out object bindObj))
            {
                Dictionary<string, object> bindDict = AsDict(bindObj);
                if (bindDict == null)
                {
                    result.Error("SPEC_BIND_TYPE", $"{path}.bind", $"bind 必须是映射(map)，实际是 {TypeName(bindObj)}");
                }
                else
                {
                    foreach (string k in bindDict.Keys)
                    {
                        if (k != "component")
                            result.Error("SPEC_FIELD_UNKNOWN", $"{path}.bind.{k}", $"bind 段未知字段 '{k}'");
                    }

                    node.BindComponent = GetString(bindDict, "component", $"{path}.bind.component", result);
                }
            }

            if (typeKnown && SpecSchema.TypeBindComponents[node.Type] == null && string.IsNullOrEmpty(node.BindComponent))
                result.Error("SPEC_BIND_REQUIRED", $"{path}.bind.component",
                    $"type='{node.Type}' 无默认绑定组件，必须显式指定 bind.component");

            if (dict.TryGetValue("place", out object placeObj))
                node.Place = BindPlace(placeObj, $"{path}.place", result);

            if (dict.TryGetValue("layout", out object layoutObj))
                node.Layout = BindLayout(layoutObj, $"{path}.layout", result);

            if (dict.TryGetValue("props", out object propsObj))
                BindProps(propsObj, node, typeKnown, $"{path}.props", result);

            // 预留字段（§3.1 决策：v1 忽略）
            node.Image = GetString(dict, "image", $"{path}.image", result);
            if (!string.IsNullOrEmpty(node.Image))
                result.Warn("SPEC_IMAGE_RESERVED", $"{path}.image", "image 为贴图 pass 预留字段，v1 忽略");

            node.Style = GetString(dict, "style", $"{path}.style", result);
            if (!string.IsNullOrEmpty(node.Style))
                result.Warn("SPEC_STYLE_RESERVED", $"{path}.style", "style 为样式 token 预留字段，v1 忽略");

            if (dict.TryGetValue("children", out object childrenObj))
            {
                List<object> children = AsList(childrenObj);
                if (children == null)
                {
                    result.Error("SPEC_CHILDREN_TYPE", $"{path}.children", $"children 必须是列表，实际是 {TypeName(childrenObj)}");
                }
                else
                {
                    for (int i = 0; i < children.Count; i++)
                        node.Children.Add(BindNode(children[i], $"{path}.children[{i}]", result));
                }
            }

            return node;
        }

        private static void BindProps(object obj, NodeSpec node, bool typeKnown, string path, SpecValidationResult result)
        {
            Dictionary<string, object> dict = AsDict(obj);
            if (dict == null)
            {
                result.Error("SPEC_PROPS_TYPE", path, $"props 必须是映射(map)，实际是 {TypeName(obj)}");
                return;
            }

            if (!typeKnown)
                return; // 类型未知已在节点层报错，属性校验跳过（避免噪音）

            List<SpecSchema.PropDef> defs = SpecSchema.TypeProps[node.Type];
            var defMap = new Dictionary<string, SpecSchema.PropDef>();
            foreach (SpecSchema.PropDef def in defs) defMap[def.Name] = def;

            foreach (KeyValuePair<string, object> kv in dict)
            {
                if (!defMap.TryGetValue(kv.Key, out SpecSchema.PropDef def))
                {
                    result.Error("SPEC_PROP_UNKNOWN", $"{path}.{kv.Key}",
                        $"type='{node.Type}' 不存在属性 '{kv.Key}'（合法: {Join(defMap.Keys)}）");
                    continue;
                }

                ValidatePropValue(def, kv.Value, $"{path}.{kv.Key}", result);
                node.Props[kv.Key] = kv.Value;
            }

            foreach (SpecSchema.PropDef def in defs)
            {
                if (def.Required && !dict.ContainsKey(def.Name))
                    result.Error("SPEC_PROP_REQUIRED", $"{path}.{def.Name}",
                        $"type='{node.Type}' 的必填属性 '{def.Name}' 缺失");
            }
        }

        private static void ValidatePropValue(SpecSchema.PropDef def, object value, string path, SpecValidationResult result)
        {
            switch (def.Kind)
            {
                case SpecSchema.EPropKind.String:
                    if (!IsScalar(value))
                        result.Error("SPEC_PROP_TYPE", path, $"应为字符串，实际 {TypeName(value)}");
                    break;

                case SpecSchema.EPropKind.Number:
                    if (!IsNumber(value))
                        result.Error("SPEC_PROP_TYPE", path, $"应为数值，实际 {TypeName(value)} ('{Str(value)}')");
                    break;

                case SpecSchema.EPropKind.Bool:
                    if (value is not bool)
                        result.Error("SPEC_PROP_TYPE", path, $"应为布尔(true/false)，实际 {TypeName(value)} ('{Str(value)}')");
                    break;

                case SpecSchema.EPropKind.Color:
                    if (!ColorRegex.IsMatch(Str(value)))
                        result.Error("SPEC_PROP_TYPE", path, $"颜色格式应为 #RRGGBB 或 #RRGGBBAA，实际 '{Str(value)}'");
                    break;

                case SpecSchema.EPropKind.Enum:
                    if (Array.IndexOf(def.EnumValues, Str(value)) < 0)
                        result.Error("SPEC_PROP_ENUM", path,
                            $"枚举值 '{Str(value)}' 不合法（合法: {Join(def.EnumValues)}）");
                    break;

                case SpecSchema.EPropKind.StringList:
                    List<object> list = AsList(value);
                    if (list == null)
                    {
                        result.Error("SPEC_PROP_TYPE", path, $"应为字符串列表，实际 {TypeName(value)}");
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (!IsScalar(list[i]))
                                result.Error("SPEC_PROP_TYPE", $"{path}[{i}]", $"列表元素应为字符串，实际 {TypeName(list[i])}");
                        }
                    }

                    break;
            }
        }

        private static PlaceSpec BindPlace(object obj, string path, SpecValidationResult result)
        {
            var place = new PlaceSpec();
            Dictionary<string, object> dict = AsDict(obj);
            if (dict == null)
            {
                result.Error("SPEC_PLACE_TYPE", path, $"place 必须是映射(map)，实际是 {TypeName(obj)}");
                return place;
            }

            foreach (string k in dict.Keys)
            {
                if (k != "anchor" && k != "offset" && k != "margins" && k != "size" && k != "pivot" && k != "rotation" && k != "scale")
                    result.Error("SPEC_FIELD_UNKNOWN", $"{path}.{k}", $"place 段未知字段 '{k}'");
            }

            place.Anchor = GetEnum(dict, "anchor", $"{path}.anchor", SpecSchema.Anchors, result, "center");

            float[] offset = GetVecN(dict, "offset", $"{path}.offset", 2, result);
            if (offset != null) { place.OffsetX = offset[0]; place.OffsetY = offset[1]; }

            // margins 顺序 [左,上,右,下]
            float[] margins = GetVecN(dict, "margins", $"{path}.margins", 4, result);
            if (margins != null)
            {
                place.MarginLeft = margins[0]; place.MarginTop = margins[1];
                place.MarginRight = margins[2]; place.MarginBottom = margins[3];
            }

            float[] size = GetVecN(dict, "size", $"{path}.size", 2, result);
            if (size != null) { place.Width = size[0]; place.Height = size[1]; }

            float[] pivot = GetVecN(dict, "pivot", $"{path}.pivot", 2, result);
            if (pivot != null) { place.PivotX = pivot[0]; place.PivotY = pivot[1]; }

            place.Rotation = GetFloat(dict, "rotation", $"{path}.rotation", result, 0f);

            float[] scale = GetVecN(dict, "scale", $"{path}.scale", 2, result);
            if (scale != null) { place.ScaleX = scale[0]; place.ScaleY = scale[1]; }

            return place;
        }

        private static LayoutSpec BindLayout(object obj, string path, SpecValidationResult result)
        {
            var layout = new LayoutSpec();
            Dictionary<string, object> dict = AsDict(obj);
            if (dict == null)
            {
                result.Error("SPEC_LAYOUT_TYPE", path, $"layout 必须是映射(map)，实际是 {TypeName(obj)}");
                return layout;
            }

            foreach (string k in dict.Keys)
            {
                if (k != "type" && k != "spacing" && k != "padding" && k != "childalignment"
                    && k != "controlchildsize" && k != "childforceexpand"
                    && k != "cellsize" && k != "constraint" && k != "constraintcount")
                    result.Error("SPEC_FIELD_UNKNOWN", $"{path}.{k}", $"layout 段未知字段 '{k}'");
            }

            string type = GetString(dict, "type", $"{path}.type", result, true);
            layout.Type = type;
            bool typeKnown = type != null && Array.IndexOf(SpecSchema.LayoutTypes, type) >= 0;
            if (type != null && !typeKnown)
                result.Error("SPEC_LAYOUT_TYPE_INVALID", $"{path}.type",
                    $"布局类型 '{type}' 不合法（合法: {Join(SpecSchema.LayoutTypes)}）");

            // spacing: float 或 [x,y]
            if (dict.TryGetValue("spacing", out object spacingObj))
            {
                if (IsNumber(spacingObj))
                {
                    float s = ToFloat(spacingObj);
                    layout.SpacingX = s; layout.SpacingY = s;
                }
                else
                {
                    float[] v = GetVecN(dict, "spacing", $"{path}.spacing", 2, result);
                    if (v != null) { layout.SpacingX = v[0]; layout.SpacingY = v[1]; }
                }
            }

            // padding 顺序 [左,右,上,下]
            float[] padding = GetVecN(dict, "padding", $"{path}.padding", 4, result);
            if (padding != null)
            {
                layout.PaddingLeft = padding[0]; layout.PaddingRight = padding[1];
                layout.PaddingTop = padding[2]; layout.PaddingBottom = padding[3];
            }

            layout.ChildAlignment = GetEnum(dict, "childalignment", $"{path}.childAlignment", TextAnchorValues, result, "UpperLeft");
            layout.ControlChildSize = GetBool(dict, "controlchildsize", $"{path}.controlChildSize", result, true);
            layout.ChildForceExpand = GetBool(dict, "childforceexpand", $"{path}.childForceExpand", result, false);

            float[] cellSize = GetVecN(dict, "cellsize", $"{path}.cellSize", 2, result);
            if (cellSize != null) { layout.CellWidth = cellSize[0]; layout.CellHeight = cellSize[1]; }

            layout.Constraint = GetEnum(dict, "constraint", $"{path}.constraint", SpecSchema.Constraints, result, "Flexible");
            layout.ConstraintCount = GetInt(dict, "constraintcount", $"{path}.constraintCount", result, 1);

            if (typeKnown && type == "grid" && cellSize == null)
                result.Error("SPEC_GRID_CELLSIZE", $"{path}.cellSize", "grid 布局必须提供 cellSize: [w,h]");

            return layout;
        }

        private static EventSpec BindEvent(object obj, string path, SpecValidationResult result)
        {
            var evt = new EventSpec();
            Dictionary<string, object> dict = AsDict(obj);
            if (dict == null)
            {
                result.Error("SPEC_EVENT_TYPE", path, $"事件必须是映射(map)，实际是 {TypeName(obj)}");
                return evt;
            }

            foreach (string k in dict.Keys)
            {
                if (k != "name" && k != "sync" && k != "params" && k != "target" && k != "trigger")
                    result.Error("SPEC_FIELD_UNKNOWN", $"{path}.{k}", $"events 段未知字段 '{k}'");
            }

            evt.Name = GetString(dict, "name", $"{path}.name", result, true);
            evt.Sync = GetBool(dict, "sync", $"{path}.sync", result, false);
            evt.Target = GetString(dict, "target", $"{path}.target", result, true);
            evt.Trigger = GetEnum(dict, "trigger", $"{path}.trigger", SpecSchema.Triggers, result, "Click");

            if (!string.IsNullOrEmpty(evt.Name) && !evt.Name.StartsWith("u_Event"))
                result.Warn("SPEC_EVENT_NAMING", $"{path}.name", $"事件名建议以 'u_Event' 开头（当前 '{evt.Name}'）");

            if (dict.TryGetValue("params", out object paramsObj))
            {
                List<object> list = AsList(paramsObj);
                if (list == null)
                {
                    result.Error("SPEC_PARAMS_TYPE", $"{path}.params", $"params 必须是列表，实际是 {TypeName(paramsObj)}");
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        string p = Str(list[i]);
                        evt.Params.Add(p);
                        if (Array.IndexOf(SpecSchema.ParamTypes, p) < 0)
                            result.Error("SPEC_PARAM_ENUM", $"{path}.params[{i}]",
                                $"参数类型 '{p}' 不合法（合法: {Join(SpecSchema.ParamTypes)}）");
                    }
                }
            }

            return evt;
        }

        // ---------------- 跨节点检查 ----------------

        private static void CheckNaming(UISpec spec, SpecValidationResult result)
        {
            var seen = new Dictionary<string, string>();

            void Walk(List<NodeSpec> nodes, string prefix)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    NodeSpec n = nodes[i];
                    string nodePath = prefix == null ? $"nodes[{i}]" : $"{prefix}.children[{i}]";
                    if (!string.IsNullOrEmpty(n.Name))
                    {
                        if (seen.TryGetValue(n.Name, out string firstPath))
                            result.Error("SPEC_NAME_DUPLICATE", nodePath,
                                $"节点名 '{n.Name}' 重复（首次出现于 {firstPath}）");
                        else
                            seen[n.Name] = nodePath;
                    }

                    Walk(n.Children, nodePath);
                }
            }

            Walk(spec.Nodes, null);

            if (!string.IsNullOrEmpty(spec.Panel.Name) && seen.ContainsKey(spec.Panel.Name))
                result.Warn("SPEC_NAME_PANEL_COLLISION", "panel.name",
                    $"面板名与某节点名相同 '{spec.Panel.Name}'，建议避开");
        }

        private static void CheckPlaceUnderLayout(UISpec spec, SpecValidationResult result)
        {
            void Walk(List<NodeSpec> nodes, string prefix, bool parentHasLayout)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    NodeSpec n = nodes[i];
                    string nodePath = prefix == null ? $"nodes[{i}]" : $"{prefix}.children[{i}]";
                    if (parentHasLayout && n.Place != null)
                        result.Warn("SPEC_PLACE_OVERRIDE_BY_LAYOUT", nodePath,
                            "父节点带 layout 时，本节点的 place 会被 LayoutGroup 覆盖（§3.5/§3.8.7）");
                    Walk(n.Children, nodePath, n.Layout != null);
                }
            }

            Walk(spec.Nodes, null, false);
        }

        private static void CheckEventTargets(UISpec spec, SpecValidationResult result)
        {
            var names = new HashSet<string>();

            void Walk(List<NodeSpec> nodes)
            {
                foreach (NodeSpec n in nodes)
                {
                    if (!string.IsNullOrEmpty(n.Name)) names.Add(n.Name);
                    Walk(n.Children);
                }
            }

            Walk(spec.Nodes);

            foreach (EventSpec evt in spec.Events)
            {
                if (string.IsNullOrEmpty(evt.Target)) continue;
                if (!names.Contains(evt.Target))
                    result.Error("SPEC_EVENT_TARGET_MISSING", "events",
                        $"事件 '{evt.Name}' 的 target '{evt.Target}' 不存在于节点树");
            }
        }

        private static void CheckResourcePaths(UISpec spec, SpecValidationResult result)
        {
            void Walk(List<NodeSpec> nodes, string prefix)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    NodeSpec n = nodes[i];
                    string nodePath = prefix == null ? $"nodes[{i}]" : $"{prefix}.children[{i}]";

                    if ((n.Type == "loop_scroll_v" || n.Type == "loop_scroll_h")
                        && n.Props.TryGetValue("item", out object item) && !ResourceExists(Str(item)))
                        result.Error("SPEC_RES_NOT_FOUND", $"{nodePath}.props.item", $"资源不存在: {item}");

                    if (n.Type == "prefab"
                        && n.Props.TryGetValue("path", out object p) && !ResourceExists(Str(p)))
                        result.Error("SPEC_RES_NOT_FOUND", $"{nodePath}.props.path", $"资源不存在: {p}");

                    Walk(n.Children, nodePath);
                }
            }

            Walk(spec.Nodes, null);

            if (!string.IsNullOrEmpty(spec.Panel.PrefabPath) && !File.Exists(ToProjectPath(spec.Panel.PrefabPath)))
                result.Warn("SPEC_PREFABPATH_NEW", "panel.prefabPath",
                    $"prefabPath 指向的文件尚不存在（构建时将新建）: {spec.Panel.PrefabPath}");
        }

        // ---------------- 取值辅助（统一小写 key + 类型检查 + 错误报告） ----------------

        private static readonly string[] TextAnchorValues =
        {
            "UpperLeft", "UpperCenter", "UpperRight",
            "MiddleLeft", "MiddleCenter", "MiddleRight",
            "LowerLeft", "LowerCenter", "LowerRight",
        };

        /// <summary>YamlDotNet 对象树 → 小写 key 的字典；非映射返回 null</summary>
        private static Dictionary<string, object> AsDict(object obj)
        {
            if (obj is Dictionary<object, object> raw)
            {
                var dict = new Dictionary<string, object>();
                foreach (KeyValuePair<object, object> kv in raw)
                    dict[Str(kv.Key).ToLowerInvariant()] = kv.Value;
                return dict;
            }

            return null;
        }

        private static List<object> AsList(object obj)
        {
            return obj as List<object>;
        }

        private static string GetString(Dictionary<string, object> dict, string key, string path,
            SpecValidationResult result, bool required = false)
        {
            if (!dict.TryGetValue(key, out object value))
            {
                if (required) result.Error("SPEC_FIELD_MISSING", path, $"缺少必填字段 '{key}'");
                return null;
            }

            if (value == null || !IsScalar(value))
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为字符串，实际 {TypeName(value)}");
                return null;
            }

            string s = Str(value);
            if (required && string.IsNullOrWhiteSpace(s))
            {
                result.Error("SPEC_FIELD_MISSING", path, $"字段 '{key}' 不能为空");
                return null;
            }

            return s;
        }

        private static string GetEnum(Dictionary<string, object> dict, string key, string path, string[] allowed,
            SpecValidationResult result, string defaultOnMissing)
        {
            if (!dict.TryGetValue(key, out object value))
                return defaultOnMissing;

            string s = Str(value);
            if (Array.IndexOf(allowed, s) < 0)
            {
                result.Error("SPEC_ENUM_INVALID", path, $"枚举值 '{s}' 不合法（合法: {Join(allowed)}）");
                return defaultOnMissing;
            }

            return s;
        }

        private static int GetInt(Dictionary<string, object> dict, string key, string path,
            SpecValidationResult result, int fallback)
        {
            if (!dict.TryGetValue(key, out object value)) return fallback;
            if (!IsNumber(value))
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为整数，实际 {TypeName(value)} ('{Str(value)}')");
                return fallback;
            }

            double d = ToFloat(value);
            if (d != Math.Floor(d))
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为整数，实际 {Str(value)}");
                return fallback;
            }

            return (int)d;
        }

        private static float GetFloat(Dictionary<string, object> dict, string key, string path,
            SpecValidationResult result, float fallback)
        {
            if (!dict.TryGetValue(key, out object value)) return fallback;
            if (!IsNumber(value))
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为数值，实际 {TypeName(value)} ('{Str(value)}')");
                return fallback;
            }

            return ToFloat(value);
        }

        private static bool GetBool(Dictionary<string, object> dict, string key, string path,
            SpecValidationResult result, bool fallback)
        {
            if (!dict.TryGetValue(key, out object value)) return fallback;
            if (value is not bool)
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为布尔(true/false)，实际 {TypeName(value)} ('{Str(value)}')");
                return fallback;
            }

            return (bool)value;
        }

        private static float[] GetVecN(Dictionary<string, object> dict, string key, string path, int n,
            SpecValidationResult result)
        {
            if (!dict.TryGetValue(key, out object value)) return null;

            List<object> list = AsList(value);
            if (list == null)
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为 {n} 个数字的列表，实际 {TypeName(value)}");
                return null;
            }

            if (list.Count != n)
            {
                result.Error("SPEC_FIELD_TYPE", path, $"字段 '{key}' 应为 {n} 个数字，实际 {list.Count} 个");
                return null;
            }

            var arr = new float[n];
            for (int i = 0; i < n; i++)
            {
                if (!IsNumber(list[i]))
                {
                    result.Error("SPEC_FIELD_TYPE", $"{path}[{i}]", $"应为数字，实际 {TypeName(list[i])} ('{Str(list[i])}')");
                    return null;
                }

                arr[i] = ToFloat(list[i]);
            }

            return arr;
        }

        // ---------------- 基础工具 ----------------

        private static bool ResourceExists(string projectRelativePath)
        {
            if (string.IsNullOrEmpty(projectRelativePath)) return false;
            return File.Exists(ToProjectPath(projectRelativePath));
        }

        /// <summary>项目内相对路径 → 绝对路径（支持 Packages/...、Assets/...，兼容反斜杠与已绝对路径）</summary>
        private static string ToProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            string p = path.Replace('\\', '/');
            if (Path.IsPathRooted(p)) return p;
            return $"{ProjectRoot}/{p}";
        }

        private static string UnwrapYamlMessage(YamlException ex)
        {
            return ex.InnerException != null ? $"(行 {ex.Start.Line}) {ex.InnerException.Message}" : $"(行 {ex.Start.Line}) {ex.Message}";
        }

        private static bool IsScalar(object value)
        {
            return value == null || value is string || value is bool || IsNumber(value);
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort
                   || value is int || value is uint || value is long || value is ulong
                   || value is float || value is double || value is decimal;
        }

        private static float ToFloat(object value)
        {
            return (float)Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        private static string Str(object value)
        {
            return value == null ? "" : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string TypeName(object value)
        {
            return value?.GetType().Name ?? "null";
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join("/", values);
        }
    }
}
