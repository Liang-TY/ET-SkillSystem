using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ET.Editor
{
    /// <summary>patch 请求里的单个操作（03 §4）。反序列化自请求 JSON。</summary>
    internal sealed class SkillEditorPatchOp
    {
        public string op;
        public string path;
        public JToken value;
    }

    /// <summary>patch 请求（03 §4 请求文件格式）。</summary>
    internal sealed class SkillEditorPatchRequest
    {
        public string kind;
        public int id;
        public SkillEditorPatchOp[] operations;
        public bool dryRun = true;
        public string expectedHash;
        public bool save;
    }

    /// <summary>
    /// patch 引擎：JSON Pointer 白名单操作（03 §4）。
    /// - 顶层 id 禁改（00 §9 身份变更走确认流程）；
    /// - 路径首段必须命中容器/标量白名单，第三段必须命中元素字段白名单；
    /// - value 按目标字段强类型转换，字符串不自动转整数引用；
    /// - add 只支持追加（索引 = 当前数量或 -1）；
    /// - 操作按顺序执行，后续索引基于前序操作后的状态；
    /// - 整体元素替换/新增经 DTO 反序列化（MissingMemberHandling.Error 拒绝未知字段）。
    /// 任一操作失败即停止，调用方应丢弃改动中的 DTO（03 §5 命令失败不部分保存）。
    /// </summary>
    internal static class SkillEditorPatchEngine
    {
        private enum FieldKind
        {
            Int,
            IntNullable,
            Float,
            Bool,
            BoolNullable,
            String,
            IntArray,
            Float3,
            Object,
        }

        private sealed class ContainerDesc
        {
            public Type ElementType;
            public Dictionary<string, FieldKind> Fields;
        }

        private static readonly Dictionary<string, ContainerDesc> Containers = new()
        {
            ["phases"] = new ContainerDesc
            {
                ElementType = typeof(SkillPhaseJson),
                Fields = new Dictionary<string, FieldKind>
                {
                    ["animId"] = FieldKind.Int,
                    ["durationMs"] = FieldKind.Int,
                    ["cancelMs"] = FieldKind.Int,
                    ["clearHitTargets"] = FieldKind.Bool,
                    ["superArmorMs"] = FieldKind.Int,
                    ["nextPhase"] = FieldKind.IntNullable,
                    ["nextSkillId"] = FieldKind.Int,
                    ["nextTrigger"] = FieldKind.String,
                    ["endOnLanding"] = FieldKind.BoolNullable,
                    ["movement"] = FieldKind.Object,
                },
            },
            ["hitReactions"] = new ContainerDesc
            {
                ElementType = typeof(SkillHitReactionJson),
                Fields = new Dictionary<string, FieldKind>
                {
                    ["phase"] = FieldKind.Int,
                    ["damage"] = FieldKind.Int,
                    ["hitstunMs"] = FieldKind.Int,
                    ["kbX"] = FieldKind.Float,
                    ["launchY"] = FieldKind.Float,
                    ["procBuffId"] = FieldKind.Int,
                    ["procChance"] = FieldKind.Int,
                },
            },
            ["hitActions"] = new ContainerDesc { ElementType = null, Fields = null },   // int[] 特例
            ["manualBoxes"] = new ContainerDesc
            {
                ElementType = typeof(SkillManualBoxJson),
                Fields = new Dictionary<string, FieldKind>
                {
                    ["phase"] = FieldKind.Int,
                    ["onMs"] = FieldKind.Int,
                    ["offMs"] = FieldKind.Int,
                    ["offset"] = FieldKind.Float3,
                    ["half"] = FieldKind.Float3,
                },
            },
            ["spawnEvents"] = new ContainerDesc
            {
                ElementType = typeof(SkillSpawnEventJson),
                Fields = new Dictionary<string, FieldKind>
                {
                    ["phase"] = FieldKind.Int,
                    ["atMs"] = FieldKind.Int,
                    ["atFrame"] = FieldKind.Int,
                    ["timeBase"] = FieldKind.String,
                    ["kind"] = FieldKind.String,
                    ["areaId"] = FieldKind.IntNullable,
                    ["bulletId"] = FieldKind.IntNullable,
                    ["buffId"] = FieldKind.IntNullable,
                    ["animId"] = FieldKind.IntNullable,
                    ["at"] = FieldKind.String,
                    ["dist"] = FieldKind.Float,
                    ["durationMs"] = FieldKind.IntNullable,
                    ["button"] = FieldKind.IntNullable,
                    ["consumeInput"] = FieldKind.BoolNullable,
                    ["untilMs"] = FieldKind.IntNullable,
                },
            },
            ["hitEvents"] = new ContainerDesc
            {
                ElementType = typeof(SkillHitEventJson),
                Fields = new Dictionary<string, FieldKind>
                {
                    ["phase"] = FieldKind.Int,
                    ["on"] = FieldKind.String,
                    ["hitPolicy"] = FieldKind.String,
                    ["kind"] = FieldKind.String,
                    ["nextPhase"] = FieldKind.IntNullable,
                    ["nextSkillId"] = FieldKind.IntNullable,
                    ["buffId"] = FieldKind.IntNullable,
                },
            },
        };

        private static readonly Dictionary<string, FieldKind> TopScalars = new()
        {
            ["name"] = FieldKind.String,
            ["type"] = FieldKind.String,
            ["cooldownMs"] = FieldKind.Int,
            ["totalTimeMs"] = FieldKind.Int,
            ["requireAirborne"] = FieldKind.Bool,
            ["manualCooldown"] = FieldKind.Bool,
            ["minCastHpPct"] = FieldKind.Int,
            ["castHpCostPct"] = FieldKind.Int,
            ["entryPhase"] = FieldKind.IntNullable,
            ["airborneEntryPhase"] = FieldKind.IntNullable,
        };

        private static readonly JsonSerializer StrictSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
        });

        /// <summary>
        /// 按顺序校验并应用全部操作到 skill DTO。失败返回 false 且 error 指明
        /// 第几个操作、什么错；DTO 可能已部分修改，调用方必须丢弃（从快照恢复或重载）。
        /// </summary>
        public static bool Apply(SkillParamJson skill, SkillEditorPatchRequest request, out string error)
        {
            error = null;
            if (request.operations == null || request.operations.Length == 0)
            {
                error = "invalid_request: operations 为空";
                return false;
            }
            for (int i = 0; i < request.operations.Length; i++)
            {
                SkillEditorPatchOp op = request.operations[i];
                if (op == null)
                {
                    error = $"invalid_request: operations[{i}] 为空";
                    return false;
                }
                if (!ApplyOp(skill, op, out error))
                {
                    error = $"operations[{i}] {op.op} {op.path}: {error}";
                    return false;
                }
            }
            return true;
        }

        private static bool ApplyOp(SkillParamJson skill, SkillEditorPatchOp op, out string error)
        {
            error = null;
            if (op.op is not ("replace" or "add" or "remove"))
            {
                error = $"invalid_request: op 不在白名单（replace/add/remove）: {op.op}";
                return false;
            }

            string[] segments = ParsePointer(op.path);
            if (segments.Length == 0)
            {
                error = $"invalid_request: path 无效: {op.path}";
                return false;
            }
            if (segments[0] == "id")
            {
                error = "invalid_request: 不允许修改顶层 id（身份变更走确认流程）";
                return false;
            }

            // 顶层标量
            if (segments.Length == 1)
                return ApplyTopScalar(skill, op, segments[0], out error);

            // 容器
            string container = segments[0];
            if (!Containers.TryGetValue(container, out ContainerDesc desc))
            {
                error = $"invalid_request: 容器 {container} 不在白名单";
                return false;
            }
            if (!int.TryParse(segments[1], NumberStyles.None, CultureInfo.InvariantCulture, out int index))
            {
                error = $"invalid_request: 索引无效: {segments[1]}";
                return false;
            }
            int count = ListCount(skill, container);

            if (segments.Length == 2)
                return ApplyListOp(skill, op, container, desc, index, count, out error);

            // 元素字段：仅 replace；hitActions 的元素是 int，无字段
            if (desc.ElementType == null)
            {
                error = $"invalid_request: {container} 元素是标量，无字段可寻址";
                return false;
            }
            if (op.op != "replace")
            {
                error = "invalid_request: 元素字段只支持 replace";
                return false;
            }
            string field = segments[2];
            if (index >= count)
            {
                error = $"validation_error: 索引越界（当前 {count} 项）";
                return false;
            }
            if (!desc.Fields.TryGetValue(field, out FieldKind kind))
            {
                error = $"invalid_request: 字段 {container}/{field} 不在白名单";
                return false;
            }
            return ReplaceElementField(skill, container, desc, index, field, kind, op.value, out error);
        }

        private static bool ApplyTopScalar(SkillParamJson skill, SkillEditorPatchOp op, string field, out string error)
        {
            error = null;
            if (op.op != "replace")
            {
                error = "invalid_request: 顶层标量只支持 replace";
                return false;
            }
            if (!TopScalars.TryGetValue(field, out FieldKind kind))
            {
                error = $"invalid_request: 顶层字段 {field} 不在白名单";
                return false;
            }
            switch (kind)
            {
                case FieldKind.String:
                    if (op.value?.Type != JTokenType.String)
                    {
                        error = TypeMismatch(field, JTokenType.String, op.value);
                        return false;
                    }
                    skill.name = field == "name" ? (string)op.value : skill.name;
                    if (field == "type") skill.type = (string)op.value;
                    return true;
                case FieldKind.Int:
                    if (!TryGetInt(op.value, out int intValue))
                    {
                        error = TypeMismatch(field, JTokenType.Integer, op.value);
                        return false;
                    }
                    switch (field)
                    {
                        case "cooldownMs": skill.cooldownMs = intValue; return true;
                        case "totalTimeMs": skill.totalTimeMs = intValue; return true;
                        case "minCastHpPct": skill.minCastHpPct = intValue; return true;
                        case "castHpCostPct": skill.castHpCostPct = intValue; return true;
                        default:
                            error = $"invalid_request: 字段 {field} 未接线";
                            return false;
                    }
                case FieldKind.IntNullable:
                    if (!TryGetNullableInt(op.value, out int? nullableInt))
                    {
                        error = TypeMismatch(field, JTokenType.Integer, op.value);
                        return false;
                    }
                    switch (field)
                    {
                        case "entryPhase": skill.entryPhase = nullableInt; return true;
                        case "airborneEntryPhase": skill.airborneEntryPhase = nullableInt; return true;
                        default:
                            error = $"invalid_request: 字段 {field} 未接线";
                            return false;
                    }
                case FieldKind.Bool:
                    if (op.value?.Type != JTokenType.Boolean)
                    {
                        error = TypeMismatch(field, JTokenType.Boolean, op.value);
                        return false;
                    }
                    if (field == "requireAirborne") skill.requireAirborne = (bool)op.value;
                    else skill.manualCooldown = (bool)op.value;
                    return true;
                default:
                    error = $"invalid_request: 字段 {field} 类型未支持";
                    return false;
            }
        }

        private static bool ApplyListOp(
            SkillParamJson skill,
            SkillEditorPatchOp op,
            string container,
            ContainerDesc desc,
            int index,
            int count,
            out string error)
        {
            error = null;
            switch (op.op)
            {
                case "replace":
                {
                    if (index >= count && !(index == 0 && count == 0))
                    {
                        error = $"validation_error: 整表 replace 只支持索引 0（当前 {count} 项）";
                        return false;
                    }
                    if (desc.ElementType == null)
                    {
                        if (!TryGetIntArray(op.value, out int[] ids))
                        {
                            error = TypeMismatch(container, JTokenType.Integer, op.value);
                            return false;
                        }
                        skill.hitActions = ids;
                        return true;
                    }
                    if (op.value?.Type != JTokenType.Array)
                    {
                        error = TypeMismatch(container, JTokenType.Array, op.value);
                        return false;
                    }
                    object[] elements = new object[((JArray)op.value).Count];
                    try
                    {
                        for (int i = 0; i < elements.Length; i++)
                            elements[i] = ((JArray)op.value)[i].ToObject(desc.ElementType, StrictSerializer);
                    }
                    catch (Exception e)
                    {
                        error = $"validation_error: {container} 元素反序列化失败（未知字段或类型不符）: {e.Message}";
                        return false;
                    }
                    AssignArray(skill, container, elements);
                    return true;
                }
                case "add":
                {
                    if (index != count && index != -1)
                    {
                        error = $"validation_error: add 只支持追加（索引 {index}，当前 {count} 项）";
                        return false;
                    }
                    if (desc.ElementType == null)
                    {
                        if (!TryGetInt(op.value, out int id))
                        {
                            error = TypeMismatch(container, JTokenType.Integer, op.value);
                            return false;
                        }
                        skill.hitActions = Append(skill.hitActions, id);
                        return true;
                    }
                    object element;
                    try
                    {
                        element = op.value.ToObject(desc.ElementType, StrictSerializer);
                    }
                    catch (Exception e)
                    {
                        error = $"validation_error: {container} 新元素反序列化失败（未知字段或类型不符）: {e.Message}";
                        return false;
                    }
                    InsertArray(skill, container, count, element);
                    return true;
                }
                case "remove":
                {
                    if (index >= count)
                    {
                        error = $"validation_error: 索引越界（当前 {count} 项）";
                        return false;
                    }
                    RemoveAt(skill, container, index);
                    return true;
                }
                default:
                    error = $"invalid_request: op 不在白名单: {op.op}";
                    return false;
            }
        }

        private static bool ReplaceElementField(
            SkillParamJson skill,
            string container,
            ContainerDesc desc,
            int index,
            string field,
            FieldKind kind,
            JToken value,
            out string error)
        {
            error = null;
            object element = GetElement(skill, container, index);

            if (kind == FieldKind.Object)
            {
                // movement 等嵌套对象：整体强类型替换
                try
                {
                    object nested = value.ToObject(typeof(SkillMovementJson), StrictSerializer);
                    SetNested(element, field, nested);
                    return true;
                }
                catch (Exception e)
                {
                    error = $"validation_error: {field} 反序列化失败: {e.Message}";
                    return false;
                }
            }

            // 统一路径：元素 → JObject → 覆写字段 → 严格反序列化回 DTO（拒绝未知字段）
            JObject jo;
            try
            {
                jo = JObject.FromObject(element);
                jo[field] = value;
                object updated = jo.ToObject(desc.ElementType, StrictSerializer);
                SetElement(skill, container, index, updated);
                return true;
            }
            catch (Exception e)
            {
                error = $"validation_error: 字段 {field} 写入失败（类型不符或未知字段）: {e.Message}";
                return false;
            }
        }

        private static void SetNested(object element, string field, object nested)
        {
            if (element is SkillPhaseJson phase && field == "movement")
                phase.movement = (SkillMovementJson)nested;
        }

        private static string TypeMismatch(string field, JTokenType expected, JToken actual)
            => $"validation_error: {field} 期望 {expected}，实际 {actual?.Type.ToString() ?? "null"}（不做自动类型转换）";

        // ── DTO 数组工具（数组定长，增删走重建）──────────────────────────

        private static int ListCount(SkillParamJson skill, string container) => container switch
        {
            "phases" => skill.phases?.Length ?? 0,
            "hitReactions" => skill.hitReactions?.Length ?? 0,
            "hitActions" => skill.hitActions?.Length ?? 0,
            "manualBoxes" => skill.manualBoxes?.Length ?? 0,
            "spawnEvents" => skill.spawnEvents?.Length ?? 0,
            "hitEvents" => skill.hitEvents?.Length ?? 0,
            _ => 0,
        };

        private static object[] ToObjectArray(object array)
        {
            switch (array)
            {
                case SkillPhaseJson[] a: return a;
                case SkillHitReactionJson[] a: return a;
                case SkillManualBoxJson[] a: return a;
                case SkillSpawnEventJson[] a: return a;
                case SkillHitEventJson[] a: return a;
                default: return Array.Empty<object>();
            }
        }

        private static void AssignArray(SkillParamJson skill, string container, object[] elements)
        {
            switch (container)
            {
                case "phases": skill.phases = Array.ConvertAll(elements, e => (SkillPhaseJson)e); break;
                case "hitReactions": skill.hitReactions = Array.ConvertAll(elements, e => (SkillHitReactionJson)e); break;
                case "manualBoxes": skill.manualBoxes = Array.ConvertAll(elements, e => (SkillManualBoxJson)e); break;
                case "spawnEvents": skill.spawnEvents = Array.ConvertAll(elements, e => (SkillSpawnEventJson)e); break;
                case "hitEvents": skill.hitEvents = Array.ConvertAll(elements, e => (SkillHitEventJson)e); break;
            }
        }

        private static object GetElement(SkillParamJson skill, string container, int index)
        {
            object[] items = ToObjectArray(RawArray(skill, container));
            return items[index];
        }

        private static object RawArray(SkillParamJson skill, string container) => container switch
        {
            "phases" => (object)skill.phases,
            "hitReactions" => skill.hitReactions,
            "manualBoxes" => skill.manualBoxes,
            "spawnEvents" => skill.spawnEvents,
            "hitEvents" => skill.hitEvents,
            _ => null,
        };

        private static void SetElement(SkillParamJson skill, string container, int index, object element)
        {
            switch (container)
            {
                case "phases": skill.phases[index] = (SkillPhaseJson)element; break;
                case "hitReactions": skill.hitReactions[index] = (SkillHitReactionJson)element; break;
                case "manualBoxes": skill.manualBoxes[index] = (SkillManualBoxJson)element; break;
                case "spawnEvents": skill.spawnEvents[index] = (SkillSpawnEventJson)element; break;
                case "hitEvents": skill.hitEvents[index] = (SkillHitEventJson)element; break;
            }
        }

        private static void InsertArray(SkillParamJson skill, string container, int index, object element)
        {
            object[] old = ToObjectArray(RawArray(skill, container));
            object[] next = new object[old.Length + 1];
            Array.Copy(old, next, index);
            next[index] = element;
            Array.Copy(old, index, next, index + 1, old.Length - index);
            AssignArray(skill, container, next);
        }

        private static void RemoveAt(SkillParamJson skill, string container, int index)
        {
            if (container == "hitActions")
            {
                skill.hitActions = RemoveAtId(skill.hitActions, index);
                return;
            }
            object[] old = ToObjectArray(RawArray(skill, container));
            object[] next = new object[old.Length - 1];
            Array.Copy(old, next, index);
            Array.Copy(old, index + 1, next, index, old.Length - index - 1);
            AssignArray(skill, container, next);
        }

        private static int[] Append(int[] array, int value)
        {
            if (array == null) return new[] { value };
            int[] next = new int[array.Length + 1];
            Array.Copy(array, next, array.Length);
            next[array.Length] = value;
            return next;
        }

        private static int[] RemoveAtId(int[] array, int index)
        {
            int[] next = new int[array.Length - 1];
            Array.Copy(array, next, index);
            Array.Copy(array, index + 1, next, index, array.Length - index - 1);
            return next;
        }

        // ── 值转换（严格类型，03 §4：字符串不自动转整数引用）──────────────

        private static bool TryGetInt(JToken token, out int value)
        {
            value = 0;
            return token?.Type == JTokenType.Integer && int.TryParse(token.ToString(CultureInfo.InvariantCulture), out value);
        }

        private static bool TryGetNullableInt(JToken token, out int? value)
        {
            value = null;
            if (token == null || token.Type == JTokenType.Null) return true;
            if (token.Type != JTokenType.Integer) return false;
            if (!int.TryParse(token.ToString(CultureInfo.InvariantCulture), out int parsed)) return false;
            value = parsed;
            return true;
        }

        private static bool TryGetIntArray(JToken token, out int[] values)
        {
            values = null;
            if (token?.Type != JTokenType.Array) return false;
            JArray array = (JArray)token;
            values = new int[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                if (!TryGetInt(array[i], out values[i])) return false;
            }
            return true;
        }

        private static string[] ParsePointer(string pointer)
        {
            if (string.IsNullOrEmpty(pointer)) return Array.Empty<string>();
            if (pointer.StartsWith("/", StringComparison.Ordinal)) pointer = pointer.Substring(1);
            if (pointer.Length == 0) return Array.Empty<string>();
            return pointer.Split('/');
        }
    }
}
