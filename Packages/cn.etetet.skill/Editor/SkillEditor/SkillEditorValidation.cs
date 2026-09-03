using System;
using System.Collections.Generic;

namespace ET.Editor
{
    internal enum SkillEditorIssueSeverity
    {
        Info,
        Warning,
        Error,
    }

    internal sealed class SkillEditorIssue
    {
        public SkillEditorIssueSeverity Severity;
        public string Message;
        public string Path;
        public int RelatedId;
    }

    /// <summary>
    /// 无副作用校验（03 §6.5）：只读编辑中的 DTO + 目录快照，不调用
    /// SkillParamEditorLoader.ReloadFromDisk，不清空全局 SkillParamLoader。
    /// 结构规则复用 SkillParamLoader.ValidateXxx（公开纯函数）；跨表引用对
    /// 目录快照检查；重复 id 只报错不自动改值（02 §2.5）。
    /// </summary>
    internal static class SkillEditorValidation
    {
        public static void ValidateDocument(
            SkillEditorDocument document,
            SkillEditorDocumentStore store,
            List<SkillEditorIssue> issues)
        {
            issues.Clear();
            if (document == null) return;

            string path = document.Asset.Path;
            switch (document.Asset.Kind)
            {
                case SkillEditorAssetKind.Skill:
                    ValidateSkillDto(document.Skill, path, store, issues);
                    break;
                case SkillEditorAssetKind.Bullet:
                    CollectStructural(SkillParamLoader.ValidateBullet, document.Bullet, path, issues);
                    ValidateBulletRefs(document.Bullet, store, path, issues);
                    break;
                case SkillEditorAssetKind.Area:
                    CollectStructural(SkillParamLoader.ValidateArea, document.Area, path, issues);
                    ValidateAreaRefs(document.Area, store, path, issues);
                    break;
                case SkillEditorAssetKind.Buff:
                    CollectStructural(SkillParamLoader.ValidateBuff, document.Buff, path, issues);
                    ValidateBuffRefs(document.Buff, store, path, issues);
                    break;
                case SkillEditorAssetKind.Action:
                    CollectStructural(SkillParamLoader.ValidateAction, document.Action, path, issues);
                    RequireId(
                        store.CollectIds(SkillEditorAssetKind.Buff),
                        document.Action?.buffId ?? 0,
                        $"{path}.buffId",
                        issues);
                    break;
            }

            ValidateUniqueId(document, store, issues);
        }

        private static void CollectStructural<T>(
            Action<T, string, List<string>> validate,
            T raw,
            string source,
            List<SkillEditorIssue> issues)
        {
            List<string> errors = new();
            validate(raw, source, errors);
            foreach (string error in errors)
            {
                issues.Add(new SkillEditorIssue { Severity = SkillEditorIssueSeverity.Error, Message = error });
            }
        }

        /// <summary>对未落盘的 Skill DTO 做全量校验（结构 + 跨表引用），供 patch 引擎复用。</summary>
        public static void ValidateSkillDto(
            SkillParamJson skill,
            string path,
            SkillEditorDocumentStore store,
            List<SkillEditorIssue> issues)
        {
            CollectStructural(SkillParamLoader.ValidateSkill, skill, path, issues);
            ValidateSkillRefs(skill, path, store, issues);
        }

        private static void ValidateSkillRefs(
            SkillParamJson skill,
            string path,
            SkillEditorDocumentStore store,
            List<SkillEditorIssue> issues)
        {
            if (skill == null) return;
            HashSet<int> actionIds = store.CollectIds(SkillEditorAssetKind.Action);
            HashSet<int> buffIds = store.CollectIds(SkillEditorAssetKind.Buff);
            HashSet<int> skillIds = store.CollectIds(SkillEditorAssetKind.Skill);
            HashSet<int> areaIds = store.CollectIds(SkillEditorAssetKind.Area);
            HashSet<int> bulletIds = store.CollectIds(SkillEditorAssetKind.Bullet);

            foreach (int id in skill.hitActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.hitActions", issues);

            foreach (SkillHitReactionJson reaction in skill.hitReactions ?? Array.Empty<SkillHitReactionJson>())
            {
                if (reaction == null) continue;
                RequireId(buffIds, reaction.procBuffId, $"{path}.hitReactions[phase={reaction.phase}].procBuffId", issues);
            }

            SkillPhaseJson[] phases = skill.phases ?? Array.Empty<SkillPhaseJson>();
            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i] == null) continue;
                RequireId(skillIds, phases[i].nextSkillId, $"{path}.phases/{i}.nextSkillId", issues);
            }

            SkillSpawnEventJson[] spawns = skill.spawnEvents ?? Array.Empty<SkillSpawnEventJson>();
            for (int i = 0; i < spawns.Length; i++)
            {
                SkillSpawnEventJson spawn = spawns[i];
                if (spawn == null) continue;
                RequireId(areaIds, spawn.areaId ?? 0, $"{path}.spawnEvents/{i}.areaId", issues);
                RequireId(bulletIds, spawn.bulletId ?? 0, $"{path}.spawnEvents/{i}.bulletId", issues);
                RequireId(buffIds, spawn.buffId ?? 0, $"{path}.spawnEvents/{i}.buffId", issues);
                // animId 的动画目录在 AnimRes 映射探针（ISSUE-013）定案前无法校验，暂跳过
            }

            SkillHitEventJson[] hits = skill.hitEvents ?? Array.Empty<SkillHitEventJson>();
            for (int i = 0; i < hits.Length; i++)
            {
                SkillHitEventJson hitEvent = hits[i];
                if (hitEvent == null) continue;
                RequireId(skillIds, hitEvent.nextSkillId ?? 0, $"{path}.hitEvents/{i}.nextSkillId", issues);
                RequireId(buffIds, hitEvent.buffId ?? 0, $"{path}.hitEvents/{i}.buffId", issues);
            }
        }

        private static void ValidateBulletRefs(
            BulletParamJson bullet,
            SkillEditorDocumentStore store,
            string path,
            List<SkillEditorIssue> issues)
        {
            if (bullet == null) return;
            HashSet<int> actionIds = store.CollectIds(SkillEditorAssetKind.Action);
            HashSet<int> buffIds = store.CollectIds(SkillEditorAssetKind.Buff);
            foreach (int id in bullet.hitActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.hitActions", issues);
            RequireId(buffIds, bullet.hitReaction?.procBuffId ?? 0, $"{path}.hitReaction.procBuffId", issues);
        }

        private static void ValidateAreaRefs(
            AreaParamJson area,
            SkillEditorDocumentStore store,
            string path,
            List<SkillEditorIssue> issues)
        {
            if (area == null) return;
            HashSet<int> actionIds = store.CollectIds(SkillEditorAssetKind.Action);
            HashSet<int> buffIds = store.CollectIds(SkillEditorAssetKind.Buff);
            foreach (int id in area.enterActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.enterActions", issues);
            foreach (int id in area.tickActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.tickActions", issues);
            foreach (int id in area.exitActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.exitActions", issues);
            RequireId(buffIds, area.hitReaction?.procBuffId ?? 0, $"{path}.hitReaction.procBuffId", issues);
        }

        private static void ValidateBuffRefs(
            BuffParamJson buff,
            SkillEditorDocumentStore store,
            string path,
            List<SkillEditorIssue> issues)
        {
            if (buff == null) return;
            HashSet<int> actionIds = store.CollectIds(SkillEditorAssetKind.Action);
            foreach (int id in buff.addActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.addActions", issues);
            foreach (int id in buff.tickActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.tickActions", issues);
            foreach (int id in buff.removeActions ?? Array.Empty<int>())
                RequireId(actionIds, id, $"{path}.removeActions", issues);
        }

        private static void ValidateUniqueId(
            SkillEditorDocument document,
            SkillEditorDocumentStore store,
            List<SkillEditorIssue> issues)
        {
            if (document?.Asset == null) return;
            int id = document.Id;
            if (id <= 0) return;
            string selfPath = document.Asset.Path;
            foreach (SkillEditorAsset other in store.Scan(document.Asset.Kind))
            {
                if (other.Error != null || other.Id != id) continue;
                if (string.Equals(other.Path, selfPath, StringComparison.OrdinalIgnoreCase)) continue;
                issues.Add(new SkillEditorIssue
                {
                    Severity = SkillEditorIssueSeverity.Error,
                    Message = $"id={id} 与 {other.Path} 重复（顶层 id 是唯一身份，不自动改值）",
                    RelatedId = id,
                });
                return;
            }
        }

        private static void RequireId(HashSet<int> table, int id, string owner, List<SkillEditorIssue> issues)
        {
            if (id > 0 && !table.Contains(id))
            {
                issues.Add(new SkillEditorIssue
                {
                    Severity = SkillEditorIssueSeverity.Error,
                    Message = $"{owner} 引用了未加载的 id={id}",
                    RelatedId = id,
                });
            }
        }
    }
}
