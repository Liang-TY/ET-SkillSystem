using System;
using System.Collections.Generic;
using TrueSync;

namespace ET
{
    // These enums intentionally mirror the Luban schema. The JSON loader keeps the
    // names readable while the simulation only consumes the strongly typed values.
    public enum SkillParamType
    {
        MeleeCombo,
        Dash,
        Projectile,
        AreaBurst,
        Buff,
    }

    public enum SkillParamNextTrigger
    {
        none,
        phaseEnd,
        hit,
        key,
    }

    public enum SkillParamTimeBase
    {
        CastTime,
        PhaseTime,
        AnimationFrame,
        PhaseEnter,
        PhaseEnd,
        Landing,
        Input,
    }

    public enum SkillParamSpawnKind
    {
        createArea,
        createBullet,
        superArmor,
        playAnim,
        addBuff,
    }

    public enum SkillParamSpawnAt
    {
        self,
        inFront,
        target,
    }

    public enum SkillParamHitTrigger
    {
        hit,
        resolvedHit,
        targetHit,
    }

    public enum SkillParamHitPolicy
    {
        FirstHitInCast,
        FirstHitInPhase,
        OncePerTargetInPhase,
        EveryResolvedHit,
    }

    public enum SkillParamHitEventKind
    {
        nextPhase,
        nextSkill,
        grab,
        addBuff,
    }

    // JSON DTOs. They deliberately use primitive/string fields so the same files
    // can be edited by a person, checked by Luban, and loaded by Unity/Hotfix.
    [Serializable]
    public sealed class SkillParamJson
    {
        public int id;
        public string name;
        public string type;
        public int cooldownMs;
        public int totalTimeMs;
        public bool requireAirborne;
        public bool manualCooldown;
        public int minCastHpPct;
        public int castHpCostPct;
        public int? entryPhase;
        public int? airborneEntryPhase;
        public SkillPhaseJson[] phases;
        public SkillHitReactionJson[] hitReactions;
        public int[] hitActions;
        public SkillManualBoxJson[] manualBoxes;
        public SkillSpawnEventJson[] spawnEvents;
        public SkillHitEventJson[] hitEvents;
    }

    [Serializable]
    public sealed class SkillMovementJson
    {
        public float distance;
        public int durationMs;
        public bool stopOnHit;
    }

    [Serializable]
    public sealed class SkillPhaseJson
    {
        public int animId;
        public int durationMs;
        public int cancelMs;
        public int? cancelButton;
        public bool clearHitTargets;
        public int superArmorMs;
        public SkillMovementJson movement;
        public int? nextPhase;
        public int nextSkillId;
        public string nextTrigger;
        public bool? endOnLanding;
    }

    [Serializable]
    public sealed class SkillManualBoxJson
    {
        public int phase;
        public int onMs;
        public int offMs;
        public float[] offset;
        public float[] half;
    }

    [Serializable]
    public sealed class SkillSpawnEventJson
    {
        // phase = -1 means a cast-wide event. Phase-specific events are reset on
        // every phase entry and are therefore safe for looping/branching skills.
        public int phase = -1;
        public int atMs;
        public int atFrame;
        public string timeBase;
        public string kind;
        public int? areaId;
        public int? bulletId;
        public int? buffId;
        public int? animId;
        public string at;
        public float dist;
        public int? durationMs;
        public int? button;
        public bool? consumeInput;
        public int? untilMs;
    }

    [Serializable]
    public sealed class SkillHitReactionJson
    {
        public int phase;
        public int damage;
        public int hitstunMs;
        public float kbX;
        public float launchY;
        public int procBuffId;
        public int procChance;
    }

    [Serializable]
    public sealed class SkillHitEventJson
    {
        public int phase;
        public string on;
        public string hitPolicy;
        public string kind;
        public int? nextPhase;
        public int? nextSkillId;
        public int? buffId;
    }

    [Serializable]
    public sealed class BulletParamJson
    {
        public int id;
        public string name;
        public float speed;
        public int totalTimeMs;
        public bool destroyOnHit;
        public int hitResetIntervalMs;
        public float[] halfExtents;
        public float[] spawnOffset;
        public float[] viewOffset;
        public bool viewGrounded = true;
        public int viewAnimId;
        public SkillHitReactionJson hitReaction;
        public int[] hitActions;
    }

    [Serializable]
    public sealed class AreaParamJson
    {
        public int id;
        public string name;
        public float[] halfExtents;
        public int totalTimeMs;
        public int tickTimeMs;
        public int[] enterActions;
        public int[] tickActions;
        public int[] exitActions;
        public SkillHitReactionJson hitReaction;
        public int viewAnimId;
        public int? viewBackAnimId;
        public int? viewEndAnimId;
    }

    [Serializable]
    public sealed class BuffParamJson
    {
        public int id;
        public string name;
        public int durationMs;
        public int tickTimeMs;
        public int maxStacks;
        public bool refreshOnApply = true;
        public int[] addActions;
        public int[] tickActions;
        public int[] removeActions;
    }

    [Serializable]
    public sealed class ActionParamJson
    {
        public int id;
        public string name;
        public string kind;
        public float value;
        public int intervalMs;
        public int? buffId;
    }

    [Serializable]
    public sealed class SkillButtonMappingsJson
    {
        public SkillButtonMappingJson[] buttons;
    }

    [Serializable]
    public sealed class SkillButtonMappingJson
    {
        public int button;
        public int skillId;
    }

    [Serializable]
    public sealed class SkillParamManifestJson
    {
        public string[] skills;
        public string[] bullets;
        public string[] areas;
        public string[] buffs;
        public string[] actions;
        public string index;
    }

    public sealed class SkillMovementParam
    {
        public readonly FP Distance;
        public readonly int DurationMs;
        public readonly bool StopOnHit;

        public SkillMovementParam(float distance, int durationMs, bool stopOnHit)
        {
            Distance = FP.FromFloat(distance);
            DurationMs = durationMs;
            StopOnHit = stopOnHit;
        }
    }

    public sealed class SkillPhaseParam
    {
        public readonly int AnimId;
        public readonly int DurationMs;
        public readonly int CancelMs;
        public readonly int CancelButton;
        public readonly bool ClearHitTargets;
        public readonly int SuperArmorMs;
        public readonly SkillMovementParam Movement;
        public readonly int NextPhase;
        public readonly int NextSkillId;
        public readonly SkillParamNextTrigger NextTrigger;
        public readonly bool EndOnLanding;

        public SkillPhaseParam(SkillPhaseJson json)
        {
            AnimId = json?.animId ?? 0;
            DurationMs = json?.durationMs ?? 0;
            CancelMs = json?.cancelMs ?? -1;
            CancelButton = json?.cancelButton ?? 0;
            ClearHitTargets = json != null && json.clearHitTargets;
            SuperArmorMs = json?.superArmorMs ?? 0;
            SkillMovementJson movement = json?.movement;
            Movement = new SkillMovementParam(
                movement?.distance ?? 0,
                movement?.durationMs ?? 0,
                movement != null && movement.stopOnHit);
            NextPhase = json?.nextPhase ?? -1;
            NextSkillId = json?.nextSkillId ?? 0;
            NextTrigger = SkillParamParser.ParseEnum(json?.nextTrigger, SkillParamNextTrigger.none);
            EndOnLanding = json?.endOnLanding ?? false;
        }
    }

    public sealed class SkillManualBoxParam
    {
        public readonly int Phase;
        public readonly int OnMs;
        public readonly int OffMs;
        public readonly TSVector Offset;
        public readonly TSVector Half;

        public SkillManualBoxParam(SkillManualBoxJson json)
        {
            Phase = json?.phase ?? 0;
            OnMs = json?.onMs ?? 0;
            OffMs = json?.offMs ?? 0;
            Offset = SkillParamParser.ToVector(json?.offset);
            Half = SkillParamParser.ToVector(json?.half);
        }
    }

    public sealed class SkillSpawnEventParam
    {
        public readonly int Phase;
        public readonly int AtMs;
        public readonly int AtFrame;
        public readonly SkillParamTimeBase TimeBase;
        public readonly SkillParamSpawnKind Kind;
        public readonly int AreaId;
        public readonly int BulletId;
        public readonly int BuffId;
        public readonly int AnimId;
        public readonly SkillParamSpawnAt At;
        public readonly FP Dist;
        public readonly int DurationMs;
        public readonly int Button;
        public readonly bool ConsumeInput;
        public readonly int UntilMs;

        public SkillSpawnEventParam(SkillSpawnEventJson json)
        {
            Phase = json?.phase ?? -1;
            AtMs = json?.atMs ?? 0;
            AtFrame = json?.atFrame ?? 0;
            TimeBase = SkillParamParser.ParseEnum(json?.timeBase, SkillParamTimeBase.CastTime);
            Kind = SkillParamParser.ParseEnum(json?.kind, SkillParamSpawnKind.createArea);
            AreaId = json?.areaId ?? 0;
            BulletId = json?.bulletId ?? 0;
            BuffId = json?.buffId ?? 0;
            AnimId = json?.animId ?? 0;
            At = SkillParamParser.ParseEnum(json?.at, SkillParamSpawnAt.self);
            Dist = FP.FromFloat(json?.dist ?? 0);
            DurationMs = json?.durationMs ?? 0;
            Button = json?.button ?? 0;
            ConsumeInput = json?.consumeInput ?? true;
            UntilMs = json?.untilMs ?? -1;
        }
    }

    public sealed class SkillHitEventParam
    {
        public readonly int Phase;
        public readonly SkillParamHitTrigger On;
        public readonly SkillParamHitPolicy HitPolicy;
        public readonly SkillParamHitEventKind Kind;
        public readonly int NextPhase;
        public readonly int NextSkillId;
        public readonly int BuffId;

        public SkillHitEventParam(SkillHitEventJson json)
        {
            Phase = json?.phase ?? 0;
            On = SkillParamParser.ParseEnum(json?.on, SkillParamHitTrigger.hit);
            HitPolicy = SkillParamParser.ParseEnum(json?.hitPolicy, SkillParamHitPolicy.FirstHitInPhase);
            Kind = SkillParamParser.ParseEnum(json?.kind, SkillParamHitEventKind.nextPhase);
            NextPhase = json?.nextPhase ?? 0;
            NextSkillId = json?.nextSkillId ?? 0;
            BuffId = json?.buffId ?? 0;
        }
    }

    public sealed class SkillParam
    {
        private readonly Dictionary<int, HitReaction> reactions;

        public readonly int Id;
        public readonly string Name;
        public readonly SkillParamType Type;
        public readonly int CooldownMs;
        public readonly int TotalTimeMs;
        public readonly bool RequireAirborne;
        public readonly bool ManualCooldown;
        public readonly int MinCastHpPct;
        public readonly int CastHpCostPct;
        public readonly int EntryPhase;
        public readonly int AirborneEntryPhase;
        public readonly SkillPhaseParam[] Phases;
        public IReadOnlyDictionary<int, HitReaction> HitReactions => reactions;
        public readonly int[] HitActions;
        public readonly SkillManualBoxParam[] ManualBoxes;
        public readonly SkillSpawnEventParam[] SpawnEvents;
        public readonly SkillHitEventParam[] HitEvents;

        public SkillParam(SkillParamJson json)
        {
            Id = json?.id ?? 0;
            Name = json?.name ?? string.Empty;
            Type = SkillParamParser.ParseEnum(json?.type, SkillParamType.MeleeCombo);
            CooldownMs = json?.cooldownMs ?? 0;
            TotalTimeMs = json?.totalTimeMs ?? 0;
            RequireAirborne = json != null && json.requireAirborne;
            ManualCooldown = json != null && json.manualCooldown;
            MinCastHpPct = json?.minCastHpPct ?? 0;
            CastHpCostPct = json?.castHpCostPct ?? 0;
            EntryPhase = json?.entryPhase ?? 0;
            AirborneEntryPhase = json?.airborneEntryPhase ?? -1;

            SkillPhaseJson[] phases = json?.phases;
            Phases = new SkillPhaseParam[phases?.Length ?? 0];
            for (int i = 0; i < Phases.Length; i++) Phases[i] = new SkillPhaseParam(phases[i]);

            SkillHitReactionJson[] reactionJson = json?.hitReactions;
            reactions = new Dictionary<int, HitReaction>();
            if (reactionJson != null)
            {
                foreach (SkillHitReactionJson reaction in reactionJson)
                {
                    if (reaction == null) continue;
                    reactions[reaction.phase] = SkillParamParser.ToHitReaction(reaction);
                }
            }

            HitActions = SkillParamParser.CopyIds(json?.hitActions);

            SkillManualBoxJson[] boxes = json?.manualBoxes;
            ManualBoxes = new SkillManualBoxParam[boxes?.Length ?? 0];
            for (int i = 0; i < ManualBoxes.Length; i++) ManualBoxes[i] = new SkillManualBoxParam(boxes[i]);

            SkillSpawnEventJson[] spawnEvents = json?.spawnEvents;
            SpawnEvents = new SkillSpawnEventParam[spawnEvents?.Length ?? 0];
            for (int i = 0; i < SpawnEvents.Length; i++) SpawnEvents[i] = new SkillSpawnEventParam(spawnEvents[i]);

            SkillHitEventJson[] hitEvents = json?.hitEvents;
            HitEvents = new SkillHitEventParam[hitEvents?.Length ?? 0];
            for (int i = 0; i < HitEvents.Length; i++) HitEvents[i] = new SkillHitEventParam(hitEvents[i]);
        }

        public HitReaction GetHitReaction(int phase)
        {
            if (reactions.TryGetValue(phase, out HitReaction reaction)) return reaction;
            if (reactions.TryGetValue(0, out reaction)) return reaction;
            return HitReaction.Default;
        }
    }

    public sealed class BulletParam
    {
        public readonly int Id;
        public readonly string Name;
        public readonly FP Speed;
        public readonly int TotalTimeMs;
        public readonly bool DestroyOnHit;
        public readonly int HitResetIntervalMs;
        public readonly TSVector HalfExtents;
        public readonly TSVector SpawnOffset;
        public readonly TSVector ViewOffset;
        public readonly bool ViewGrounded;
        public readonly int ViewAnimId;
        public readonly HitReaction HitReaction;
        public readonly int[] HitActions;

        public BulletParam(BulletParamJson json)
        {
            Id = json?.id ?? 0;
            Name = json?.name ?? string.Empty;
            Speed = FP.FromFloat(json?.speed ?? 0);
            TotalTimeMs = json?.totalTimeMs ?? 0;
            DestroyOnHit = json != null && json.destroyOnHit;
            HitResetIntervalMs = json?.hitResetIntervalMs ?? 0;
            HalfExtents = SkillParamParser.ToVector(json?.halfExtents);
            SpawnOffset = SkillParamParser.ToVector(json?.spawnOffset);
            ViewOffset = SkillParamParser.ToVector(json?.viewOffset);
            ViewGrounded = json == null || json.viewGrounded;
            ViewAnimId = json?.viewAnimId ?? 0;
            HitReaction = SkillParamParser.ToHitReaction(json?.hitReaction);
            HitActions = SkillParamParser.CopyIds(json?.hitActions);
        }
    }

    public sealed class AreaParam
    {
        public readonly int Id;
        public readonly string Name;
        public readonly TSVector HalfExtents;
        public readonly int TotalTimeMs;
        public readonly int TickTimeMs;
        public readonly int[] EnterActions;
        public readonly int[] TickActions;
        public readonly int[] ExitActions;
        public readonly HitReaction HitReaction;
        public readonly int ViewAnimId;
        public readonly int ViewBackAnimId;
        public readonly int ViewEndAnimId;

        public AreaParam(AreaParamJson json)
        {
            Id = json?.id ?? 0;
            Name = json?.name ?? string.Empty;
            HalfExtents = SkillParamParser.ToVector(json?.halfExtents);
            TotalTimeMs = json?.totalTimeMs ?? 0;
            TickTimeMs = json?.tickTimeMs ?? 0;
            EnterActions = SkillParamParser.CopyIds(json?.enterActions);
            TickActions = SkillParamParser.CopyIds(json?.tickActions);
            ExitActions = SkillParamParser.CopyIds(json?.exitActions);
            HitReaction = SkillParamParser.ToHitReaction(json?.hitReaction);
            ViewAnimId = json?.viewAnimId ?? 0;
            ViewBackAnimId = json?.viewBackAnimId ?? 0;
            ViewEndAnimId = json?.viewEndAnimId ?? 0;
        }
    }

    public sealed class BuffParam
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int DurationMs;
        public readonly int TickTimeMs;
        public readonly int MaxStacks;
        public readonly bool RefreshOnApply;
        public readonly int[] AddActions;
        public readonly int[] TickActions;
        public readonly int[] RemoveActions;

        public BuffParam(BuffParamJson json)
        {
            Id = json?.id ?? 0;
            Name = json?.name ?? string.Empty;
            DurationMs = json?.durationMs ?? 0;
            TickTimeMs = json?.tickTimeMs ?? 0;
            MaxStacks = json?.maxStacks ?? 0;
            RefreshOnApply = json == null || json.refreshOnApply;
            AddActions = SkillParamParser.CopyIds(json?.addActions);
            TickActions = SkillParamParser.CopyIds(json?.tickActions);
            RemoveActions = SkillParamParser.CopyIds(json?.removeActions);
        }
    }

    public sealed class ActionParam
    {
        public readonly int Id;
        public readonly string Name;
        public readonly string Kind;
        public readonly FP Value;
        public readonly int IntervalMs;
        public readonly int BuffId;

        public ActionParam(ActionParamJson json)
        {
            Id = json?.id ?? 0;
            Name = json?.name ?? string.Empty;
            Kind = json?.kind ?? string.Empty;
            Value = FP.FromFloat(json?.value ?? 0);
            IntervalMs = json?.intervalMs ?? 0;
            BuffId = json?.buffId ?? 0;
        }
    }

    internal static class SkillParamParser
    {
        public static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            return Enum.TryParse(value, true, out T result) ? result : fallback;
        }

        public static TSVector ToVector(float[] values)
        {
            if (values == null || values.Length < 3) return TSVector.zero;
            return new TSVector(FP.FromFloat(values[0]), FP.FromFloat(values[1]), FP.FromFloat(values[2]));
        }

        public static HitReaction ToHitReaction(SkillHitReactionJson json)
        {
            if (json == null) return HitReaction.Default;
            return new HitReaction
            {
                Damage = json.damage,
                HitstunMs = json.hitstunMs,
                KnockbackX = (int)json.kbX,
                LaunchY = (int)json.launchY,
                ProcBuffId = json.procBuffId,
                ProcChance = json.procChance,
            };
        }

        public static int[] CopyIds(int[] ids)
        {
            if (ids == null || ids.Length == 0) return null;
            int[] copy = new int[ids.Length];
            Array.Copy(ids, copy, ids.Length);
            return copy;
        }
    }
}
