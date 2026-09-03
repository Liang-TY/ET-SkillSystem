using System;

namespace ET.Editor
{
    /// <summary>
    /// 所有编辑修改的唯一入口（02 §2.3：View 不直接散改 DTO）。
    /// 快照式 Undo：Session.Execute 捕获前后快照，Undo/Redo 由 SkillEditorHistory 恢复。
    /// </summary>
    internal interface ISkillEditorCommand
    {
        string Description { get; }
        void Do(SkillEditorDocument document);
    }

    /// <summary>通用命令：修改逻辑以委托表达，可审计性由 Description + 前后快照保证。</summary>
    internal sealed class SkillEditorDelegateCommand : ISkillEditorCommand
    {
        private readonly string description;
        private readonly Action<SkillEditorDocument> apply;

        public SkillEditorDelegateCommand(string description, Action<SkillEditorDocument> apply)
        {
            this.description = description ?? "修改";
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        public string Description => description;

        public void Do(SkillEditorDocument document) => apply(document);
    }

    /// <summary>列表结构性操作（Step 2）：索引在执行时解析，命令描述带容器/索引。</summary>
    internal static class SkillEditorListCommands
    {
        public static ISkillEditorCommand AddPhase(int afterIndex)
            => new SkillEditorDelegateCommand($"phases 添加（{afterIndex + 1}）", document =>
            {
                SkillParamJson skill = document.Skill;
                SkillPhaseJson[] old = skill.phases ?? Array.Empty<SkillPhaseJson>();
                SkillPhaseJson phase = new() { durationMs = 500, cancelMs = -1 };
                skill.phases = Insert(old, afterIndex + 1, phase);
            });

        public static ISkillEditorCommand AddManualBox()
            => new SkillEditorDelegateCommand("manualBoxes 添加", document =>
            {
                SkillParamJson skill = document.Skill;
                SkillManualBoxJson[] old = skill.manualBoxes ?? Array.Empty<SkillManualBoxJson>();
                old ??= Array.Empty<SkillManualBoxJson>();
                SkillManualBoxJson box = new()
                {
                    phase = 0,
                    onMs = 0,
                    offMs = 100,
                    offset = new float[] { 0.9f, 0.8f, 0f },
                    half = new float[] { 0.8f, 0.6f, 0.3f },
                };
                skill.manualBoxes = Append(old, box);
            });

        public static ISkillEditorCommand AddSpawnEvent()
            => new SkillEditorDelegateCommand("spawnEvents 添加", document =>
            {
                SkillParamJson skill = document.Skill;
                SkillSpawnEventJson[] old = skill.spawnEvents ?? Array.Empty<SkillSpawnEventJson>();
                SkillSpawnEventJson spawn = new() { phase = 0, atMs = 0, timeBase = "PhaseTime", kind = "playAnim", animId = 0 };
                skill.spawnEvents = Append(old, spawn);
            });

        public static ISkillEditorCommand AddHitEvent()
            => new SkillEditorDelegateCommand("hitEvents 添加", document =>
            {
                SkillParamJson skill = document.Skill;
                SkillHitEventJson[] old = skill.hitEvents ?? Array.Empty<SkillHitEventJson>();
                SkillHitEventJson hit = new() { phase = 0, on = "hit", hitPolicy = "OncePerTargetInPhase", kind = "nextPhase" };
                skill.hitEvents = Append(old, hit);
            });

        public static ISkillEditorCommand AddHitReaction()
            => new SkillEditorDelegateCommand("hitReactions 添加", document =>
            {
                SkillParamJson skill = document.Skill;
                SkillHitReactionJson[] old = skill.hitReactions ?? Array.Empty<SkillHitReactionJson>();
                SkillHitReactionJson reaction = new() { phase = 0 };
                skill.hitReactions = Append(old, reaction);
            });

        public static ISkillEditorCommand RemoveAt(string container, int index)
            => new SkillEditorDelegateCommand($"{container} 删除 [{index}]", document =>
            {
                SkillParamJson skill = document.Skill;
                switch (container)
                {
                    case "phases": skill.phases = RemoveAt(skill.phases, index); break;
                    case "manualBoxes": skill.manualBoxes = RemoveAt(skill.manualBoxes, index); break;
                    case "spawnEvents": skill.spawnEvents = RemoveAt(skill.spawnEvents, index); break;
                    case "hitEvents": skill.hitEvents = RemoveAt(skill.hitEvents, index); break;
                    case "hitReactions": skill.hitReactions = RemoveAt(skill.hitReactions, index); break;
                }
            });

        public static ISkillEditorCommand DuplicateAt(string container, int index)
            => new SkillEditorDelegateCommand($"{container} 复制 [{index}]", document =>
            {
                SkillParamJson skill = document.Skill;
                switch (container)
                {
                    case "phases": skill.phases = Duplicate(skill.phases, index); break;
                    case "manualBoxes": skill.manualBoxes = Duplicate(skill.manualBoxes, index); break;
                    case "spawnEvents": skill.spawnEvents = Duplicate(skill.spawnEvents, index); break;
                    case "hitEvents": skill.hitEvents = Duplicate(skill.hitEvents, index); break;
                    case "hitReactions": skill.hitReactions = Duplicate(skill.hitReactions, index); break;
                }
            });

        public static ISkillEditorCommand MoveWithin(string container, int from, int to)
            => new SkillEditorDelegateCommand($"{container} 移动 [{from}→{to}]", document =>
            {
                SkillParamJson skill = document.Skill;
                switch (container)
                {
                    case "phases": skill.phases = Move(skill.phases, from, to); break;
                    case "manualBoxes": skill.manualBoxes = Move(skill.manualBoxes, from, to); break;
                    case "spawnEvents": skill.spawnEvents = Move(skill.spawnEvents, from, to); break;
                    case "hitEvents": skill.hitEvents = Move(skill.hitEvents, from, to); break;
                    case "hitReactions": skill.hitReactions = Move(skill.hitReactions, from, to); break;
                }
            });

        private static T[] Append<T>(T[] array, T item)
        {
            if (array == null) return new[] { item };
            T[] next = new T[array.Length + 1];
            Array.Copy(array, next, array.Length);
            next[array.Length] = item;
            return next;
        }

        private static T[] Insert<T>(T[] array, int index, T item)
        {
            if (array == null || array.Length == 0) return new[] { item };
            T[] next = new T[array.Length + 1];
            Array.Copy(array, next, index);
            next[index] = item;
            Array.Copy(array, index, next, index + 1, array.Length - index);
            return next;
        }

        private static T[] RemoveAt<T>(T[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length) return array;
            T[] next = new T[array.Length - 1];
            Array.Copy(array, next, index);
            Array.Copy(array, index + 1, next, index, array.Length - index - 1);
            return next;
        }

        private static T[] Duplicate<T>(T[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length) return array;
            T source = array[index];
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            T copy = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return Insert(array, index + 1, copy);
        }

        private static T[] Move<T>(T[] array, int from, int to)
        {
            if (array == null || from < 0 || from >= array.Length || to < 0 || to >= array.Length || from == to)
                return array;
            T[] next = (T[])array.Clone();
            T item = next[from];
            if (from < to)
            {
                Array.Copy(next, from + 1, next, from, to - from);
            }
            else
            {
                Array.Copy(next, to, next, to + 1, from - to);
            }
            next[to] = item;
            return next;
        }
    }
}
