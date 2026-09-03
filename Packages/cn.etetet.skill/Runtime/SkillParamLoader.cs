using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ET
{
    /// <summary>
    /// SkillParams 的运行时缓存和校验入口。
    ///
    /// Luban 负责 schema、引用检查和生成代码；这里保留一个很薄的 JSON DTO
    /// 转换层，避免把生成类的序列化实现绑死在 Unity/服务器某一侧。所有跨内容
    /// 引用仍然是整数，名称只用于错误信息和编辑器展示。
    /// </summary>
    public static class SkillParamLoader
    {
        [StaticField]
        private static readonly Dictionary<int, SkillParam> skills = new();

        [StaticField]
        private static readonly Dictionary<int, BulletParam> bullets = new();

        [StaticField]
        private static readonly Dictionary<int, AreaParam> areas = new();

        [StaticField]
        private static readonly Dictionary<int, BuffParam> buffs = new();

        [StaticField]
        private static readonly Dictionary<int, ActionParam> actions = new();

        [StaticField]
        private static readonly Dictionary<int, int> buttonMappings = new();

        public static bool IsLoaded { get; private set; }

        public static IReadOnlyDictionary<int, SkillParam> Skills => skills;
        public static IReadOnlyDictionary<int, BulletParam> Bullets => bullets;
        public static IReadOnlyDictionary<int, AreaParam> Areas => areas;
        public static IReadOnlyDictionary<int, BuffParam> Buffs => buffs;
        public static IReadOnlyDictionary<int, ActionParam> Actions => actions;
        public static IReadOnlyDictionary<int, int> ButtonMappings => buttonMappings;

        public static SkillParam GetSkill(int id)
        {
            skills.TryGetValue(id, out SkillParam value);
            return value;
        }

        public static BulletParam GetBullet(int id)
        {
            bullets.TryGetValue(id, out BulletParam value);
            return value;
        }

        public static AreaParam GetArea(int id)
        {
            areas.TryGetValue(id, out AreaParam value);
            return value;
        }

        public static BuffParam GetBuff(int id)
        {
            buffs.TryGetValue(id, out BuffParam value);
            return value;
        }

        public static ActionParam GetAction(int id)
        {
            actions.TryGetValue(id, out ActionParam value);
            return value;
        }

        public static bool TryGetSkillForButton(int button, out int skillId)
            => buttonMappings.TryGetValue(button, out skillId);

        /// <summary>清空当前批次；调用方应在加载整批文件前调用。</summary>
        public static void Clear()
        {
            skills.Clear();
            bullets.Clear();
            areas.Clear();
            buffs.Clear();
            actions.Clear();
            buttonMappings.Clear();
            IsLoaded = false;
        }

        public static bool LoadSkillJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out SkillParamJson raw)) return false;
            if (!ValidateSkill(raw, source)) return false;
            if (skills.ContainsKey(raw.id)) return Reject(source, $"重复 skillId={raw.id}");
            skills.Add(raw.id, new SkillParam(raw));
            ContentIds.Register(ContentIdKind.Skill, raw.id, raw.name);
            IsLoaded = true;
            return true;
        }

        public static bool LoadBulletJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out BulletParamJson raw)) return false;
            if (!ValidateBullet(raw, source)) return false;
            if (bullets.ContainsKey(raw.id)) return Reject(source, $"重复 bulletId={raw.id}");
            bullets.Add(raw.id, new BulletParam(raw));
            ContentIds.Register(ContentIdKind.Bullet, raw.id, raw.name);
            IsLoaded = true;
            return true;
        }

        public static bool LoadAreaJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out AreaParamJson raw)) return false;
            if (!ValidateArea(raw, source)) return false;
            if (areas.ContainsKey(raw.id)) return Reject(source, $"重复 areaId={raw.id}");
            areas.Add(raw.id, new AreaParam(raw));
            ContentIds.Register(ContentIdKind.Area, raw.id, raw.name);
            IsLoaded = true;
            return true;
        }

        public static bool LoadBuffJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out BuffParamJson raw)) return false;
            if (!ValidateBuff(raw, source)) return false;
            if (buffs.ContainsKey(raw.id)) return Reject(source, $"重复 buffId={raw.id}");
            buffs.Add(raw.id, new BuffParam(raw));
            ContentIds.Register(ContentIdKind.Buff, raw.id, raw.name);
            IsLoaded = true;
            return true;
        }

        public static bool LoadActionJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out ActionParamJson raw)) return false;
            if (!ValidateAction(raw, source)) return false;
            if (actions.ContainsKey(raw.id)) return Reject(source, $"重复 actionId={raw.id}");
            actions.Add(raw.id, new ActionParam(raw));
            ContentIds.Register(ContentIdKind.Action, raw.id, raw.name);
            IsLoaded = true;
            return true;
        }

        public static bool LoadButtonMappingsJson(string json, string source)
        {
            if (!TryDeserialize(json, source, out SkillButtonMappingsJson raw)) return false;
            if (raw.buttons == null) return Reject(source, "缺少 buttons 数组");
            foreach (SkillButtonMappingJson mapping in raw.buttons)
            {
                if (mapping == null || mapping.button <= 0 || mapping.skillId <= 0)
                    return Reject(source, "buttons 中存在非法 button/skillId");
                if (buttonMappings.ContainsKey(mapping.button))
                    return Reject(source, $"重复 button={mapping.button}");
                buttonMappings.Add(mapping.button, mapping.skillId);
            }
            IsLoaded = true;
            return true;
        }

        /// <summary>
        /// 在所有表加载完后执行跨表引用检查。加载阶段只做本表结构检查，
        /// 这样运行时可以按 YooAsset 返回顺序读取文件而不依赖顺序。
        /// </summary>
        public static SkillParamValidationReport ValidateAll()
        {
            SkillParamValidationReport report = new();

            foreach (SkillParam skill in skills.Values)
            {
                foreach (int actionId in skill.HitActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"skillId={skill.Id}.hitActions", report);
                foreach (KeyValuePair<int, HitReaction> reaction in skill.HitReactions)
                    Require(buffs, reaction.Value?.ProcBuffId ?? 0, $"skillId={skill.Id}.hitReactions", report);
                foreach (SkillPhaseParam phase in skill.Phases)
                {
                    if (phase.NextSkillId > 0)
                        Require(skills, phase.NextSkillId, $"skillId={skill.Id}.phases.nextSkillId", report);
                }
                foreach (SkillSpawnEventParam spawn in skill.SpawnEvents)
                {
                    if (spawn.AreaId > 0) Require(areas, spawn.AreaId, $"skillId={skill.Id}.spawnEvents.areaId", report);
                    if (spawn.BulletId > 0) Require(bullets, spawn.BulletId, $"skillId={skill.Id}.spawnEvents.bulletId", report);
                    if (spawn.BuffId > 0) Require(buffs, spawn.BuffId, $"skillId={skill.Id}.spawnEvents.buffId", report);
                }
                foreach (SkillHitEventParam hitEvent in skill.HitEvents)
                {
                    if (hitEvent.NextSkillId > 0)
                        Require(skills, hitEvent.NextSkillId, $"skillId={skill.Id}.hitEvents.nextSkillId", report);
                    if (hitEvent.BuffId > 0)
                        Require(buffs, hitEvent.BuffId, $"skillId={skill.Id}.hitEvents.buffId", report);
                }
            }

            foreach (BulletParam bullet in bullets.Values)
            {
                foreach (int actionId in bullet.HitActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"bulletId={bullet.Id}.hitActions", report);
                Require(buffs, bullet.HitReaction.ProcBuffId, $"bulletId={bullet.Id}.hitReaction", report);
            }

            foreach (AreaParam area in areas.Values)
            {
                foreach (int actionId in area.EnterActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"areaId={area.Id}.enterActions", report);
                foreach (int actionId in area.TickActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"areaId={area.Id}.tickActions", report);
                foreach (int actionId in area.ExitActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"areaId={area.Id}.exitActions", report);
                Require(buffs, area.HitReaction.ProcBuffId, $"areaId={area.Id}.hitReaction", report);
            }

            foreach (BuffParam buff in buffs.Values)
            {
                foreach (int actionId in buff.AddActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"buffId={buff.Id}.addActions", report);
                foreach (int actionId in buff.TickActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"buffId={buff.Id}.tickActions", report);
                foreach (int actionId in buff.RemoveActions ?? Array.Empty<int>())
                    Require(actions, actionId, $"buffId={buff.Id}.removeActions", report);
            }

            foreach (KeyValuePair<int, int> mapping in buttonMappings)
                Require(skills, mapping.Value, $"button={mapping.Key}", report);

            if (report.Errors.Count > 0)
            {
                foreach (string error in report.Errors) Log.Error($"[SkillParams] {error}");
            }
            return report;
        }

        private static void Require<T>(Dictionary<int, T> table, int id, string owner, SkillParamValidationReport report)
        {
            if (id > 0 && !table.ContainsKey(id))
                report.Errors.Add($"{owner} 引用了未加载的 id={id}");
        }

        private static bool TryDeserialize<T>(string json, string source, out T value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(json)) return Reject(source, "文件为空");
            try
            {
                value = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    NullValueHandling = NullValueHandling.Include,
                });
                if (value == null) return Reject(source, "JSON 根对象为空");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"[SkillParams] 解析失败 {source}: {e.Message}");
                return false;
            }
        }

        private static bool ValidateSkill(SkillParamJson raw, string source)
        {
            if (raw == null || raw.id <= 0) return Reject(source, "skill id 必须 > 0");
            if (string.IsNullOrWhiteSpace(raw.name)) return Reject(source, $"skillId={raw.id} 缺少 name");
            if (!IsEnum(raw.type, out SkillParamType _)) return Reject(source, $"skillId={raw.id} type 无效: {raw.type}");
            if (raw.cooldownMs < 0 || raw.totalTimeMs < 0 || raw.minCastHpPct < 0 || raw.castHpCostPct < 0)
                return Reject(source, $"skillId={raw.id} 时间/百分比不能为负");
            if (raw.minCastHpPct > 100 || raw.castHpCostPct > 100)
                return Reject(source, $"skillId={raw.id} 百分比必须在 0..100");
            if (raw.phases == null || raw.phases.Length == 0)
                return Reject(source, $"skillId={raw.id} 至少需要一个 phase");
            int entryPhase = raw.entryPhase ?? 0;
            int airborneEntryPhase = raw.airborneEntryPhase ?? -1;
            if (entryPhase < 0 || entryPhase >= raw.phases.Length
                || airborneEntryPhase < -1 || airborneEntryPhase >= raw.phases.Length)
                return Reject(source, $"skillId={raw.id} entryPhase/airborneEntryPhase 越界");
            if (raw.spawnEvents != null && raw.spawnEvents.Length > 64)
                return Reject(source, $"skillId={raw.id} spawnEvents 最多 64 个");
            if (raw.hitEvents != null && raw.hitEvents.Length > 64)
                return Reject(source, $"skillId={raw.id} hitEvents 最多 64 个");
            for (int i = 0; i < raw.phases.Length; i++)
            {
                SkillPhaseJson phase = raw.phases[i];
                if (phase == null || phase.durationMs < 0 || phase.cancelMs < -1
                    || (phase.cancelButton.HasValue && phase.cancelButton.Value < 0) || phase.superArmorMs < 0)
                    return Reject(source, $"skillId={raw.id} phase[{i}] 时间字段无效");
                if (!IsEnum(phase.nextTrigger, out SkillParamNextTrigger _))
                    return Reject(source, $"skillId={raw.id} phase[{i}] nextTrigger 无效");
                if (phase.nextPhase.HasValue
                    && (phase.nextPhase.Value < 0 || phase.nextPhase.Value >= raw.phases.Length))
                    return Reject(source, $"skillId={raw.id} phase[{i}] nextPhase 越界");
                if (phase.movement != null && phase.movement.durationMs < 0)
                    return Reject(source, $"skillId={raw.id} phase[{i}] movement.durationMs 不能为负");
            }
            if (raw.manualBoxes != null)
            {
                foreach (SkillManualBoxJson box in raw.manualBoxes)
                {
                    if (box == null || box.phase < 0 || box.phase >= raw.phases.Length || box.onMs < 0 || box.offMs < box.onMs)
                        return Reject(source, $"skillId={raw.id} manualBox 无效");
                    if (!HasVector3(box.offset) || !HasVector3(box.half))
                        return Reject(source, $"skillId={raw.id} manualBox 必须提供 3 维 offset/half");
                }
            }
            if (raw.spawnEvents != null)
            {
                foreach (SkillSpawnEventJson spawn in raw.spawnEvents)
                {
                    if (spawn == null || spawn.phase < -1 || spawn.atMs < 0 || spawn.atFrame < 0)
                        return Reject(source, $"skillId={raw.id} spawnEvent 时间/phase 无效");
                    if (spawn.phase >= raw.phases.Length)
                        return Reject(source, $"skillId={raw.id} spawnEvent phase 越界: {spawn.phase}");
                    if (!IsEnum(spawn.timeBase, out SkillParamTimeBase timeBase)
                        || !IsEnum(spawn.kind, out SkillParamSpawnKind kind)
                        || !IsEnum(spawn.at, out SkillParamSpawnAt _))
                        return Reject(source, $"skillId={raw.id} spawnEvent 枚举无效");
                    if (spawn.dist < 0 || (spawn.durationMs.HasValue && spawn.durationMs.Value < 0))
                        return Reject(source, $"skillId={raw.id} spawnEvent 数值无效");
                    if (spawn.untilMs.HasValue && (spawn.untilMs.Value < spawn.atMs))
                        return Reject(source, $"skillId={raw.id} spawnEvent untilMs 必须 >= atMs");
                    if (timeBase != SkillParamTimeBase.CastTime && timeBase != SkillParamTimeBase.Input
                        && spawn.phase < 0)
                        return Reject(source, $"skillId={raw.id} 该 timeBase 的 spawnEvent 必须指定 phase");
                    if (timeBase == SkillParamTimeBase.Input && (!spawn.button.HasValue || spawn.button.Value <= 0))
                        return Reject(source, $"skillId={raw.id} Input spawnEvent 必须指定 button>0");
                    if (kind == SkillParamSpawnKind.createArea && (!spawn.areaId.HasValue || spawn.areaId.Value <= 0)
                        || kind == SkillParamSpawnKind.createBullet && (!spawn.bulletId.HasValue || spawn.bulletId.Value <= 0)
                        || kind == SkillParamSpawnKind.addBuff && (!spawn.buffId.HasValue || spawn.buffId.Value <= 0)
                        || kind == SkillParamSpawnKind.playAnim && (!spawn.animId.HasValue || spawn.animId.Value <= 0)
                        || kind == SkillParamSpawnKind.superArmor && (!spawn.durationMs.HasValue || spawn.durationMs.Value <= 0))
                        return Reject(source, $"skillId={raw.id} spawnEvent 缺少 kind 对应的整数引用/持续时间");
                }
            }
            if (raw.hitEvents != null)
            {
                foreach (SkillHitEventJson hitEvent in raw.hitEvents)
                {
                    if (hitEvent == null || hitEvent.phase < 0 || hitEvent.phase >= raw.phases.Length)
                        return Reject(source, $"skillId={raw.id} hitEvent phase 无效");
                    if (!IsEnum(hitEvent.on, out SkillParamHitTrigger _)
                        || !IsEnum(hitEvent.hitPolicy, out SkillParamHitPolicy _)
                        || !IsEnum(hitEvent.kind, out SkillParamHitEventKind _))
                        return Reject(source, $"skillId={raw.id} hitEvent 枚举无效");
                    if (hitEvent.nextPhase.HasValue
                        && (hitEvent.nextPhase.Value < 0 || hitEvent.nextPhase.Value >= raw.phases.Length))
                        return Reject(source, $"skillId={raw.id} hitEvent nextPhase 越界");
                }
            }
            if (raw.hitReactions != null)
            {
                foreach (SkillHitReactionJson reaction in raw.hitReactions)
                {
                    if (reaction == null || reaction.phase < 0 || reaction.phase >= raw.phases.Length)
                        return Reject(source, $"skillId={raw.id} hitReaction phase 无效");
                    if (reaction.hitstunMs < 0 || reaction.procChance < 0 || reaction.procChance > 100)
                        return Reject(source, $"skillId={raw.id} hitReaction 数值无效");
                }
            }
            return true;
        }

        private static bool ValidateBullet(BulletParamJson raw, string source)
        {
            if (raw == null || raw.id <= 0) return Reject(source, "bullet id 必须 > 0");
            if (string.IsNullOrWhiteSpace(raw.name)) return Reject(source, $"bulletId={raw.id} 缺少 name");
            if (raw.speed < 0 || raw.totalTimeMs < 0 || raw.hitResetIntervalMs < 0)
                return Reject(source, $"bulletId={raw.id} 数值不能为负");
            if (!HasVector3(raw.halfExtents) || !HasVector3(raw.spawnOffset) || !HasVector3(raw.viewOffset))
                return Reject(source, $"bulletId={raw.id} 必须提供 3 维盒/偏移");
            return true;
        }

        private static bool ValidateArea(AreaParamJson raw, string source)
        {
            if (raw == null || raw.id <= 0) return Reject(source, "area id 必须 > 0");
            if (string.IsNullOrWhiteSpace(raw.name)) return Reject(source, $"areaId={raw.id} 缺少 name");
            if (raw.totalTimeMs < 0 || raw.tickTimeMs < 0)
                return Reject(source, $"areaId={raw.id} 时间不能为负");
            if (!HasVector3(raw.halfExtents)) return Reject(source, $"areaId={raw.id} halfExtents 必须为 3 维");
            return true;
        }

        private static bool ValidateBuff(BuffParamJson raw, string source)
        {
            if (raw == null || raw.id <= 0) return Reject(source, "buff id 必须 > 0");
            if (string.IsNullOrWhiteSpace(raw.name)) return Reject(source, $"buffId={raw.id} 缺少 name");
            if (raw.durationMs < 0 || raw.tickTimeMs < 0 || raw.maxStacks < 0)
                return Reject(source, $"buffId={raw.id} 数值不能为负");
            return true;
        }

        private static bool ValidateAction(ActionParamJson raw, string source)
        {
            if (raw == null || raw.id <= 0) return Reject(source, "action id 必须 > 0");
            if (string.IsNullOrWhiteSpace(raw.name) || string.IsNullOrWhiteSpace(raw.kind))
                return Reject(source, $"actionId={raw.id} 缺少 name/kind");
            if (raw.intervalMs < 0) return Reject(source, $"actionId={raw.id} intervalMs 不能为负");
            return true;
        }

        private static bool IsEnum<T>(string value, out T result) where T : struct
        {
            return Enum.TryParse(value, true, out result) && Enum.IsDefined(typeof(T), result);
        }

        private static bool HasVector3(float[] values)
            => values != null && values.Length == 3;

        private static bool Reject(string source, string message)
        {
            Log.Error($"[SkillParams] 拒绝 {source}: {message}");
            return false;
        }
    }

    public sealed class SkillParamValidationReport
    {
        public readonly List<string> Errors = new();
        public bool IsValid => Errors.Count == 0;
    }
}
