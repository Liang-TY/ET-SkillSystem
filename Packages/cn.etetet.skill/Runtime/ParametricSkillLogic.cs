using TrueSync;

namespace ET
{
    /// <summary>
    /// 通用参数技能执行器。
    ///
    /// 子类只需用无状态 property 返回一个整数 ConfiguredSkillId；所有数值、动画、
    /// 手动盒和事件均从 SkillParamLoader 查询。这样保留现有 [SkillId] 注册协议，
    /// 又不会把 skillId/配置缓存塞进逻辑实例（逻辑实例仍是全局无状态单例）。
    /// </summary>
    public abstract class ParametricSkillLogic : SkillLogic
    {
        public override int CooldownMs => GetConfiguredParam()?.CooldownMs ?? 0;

        public override bool ManualCooldown => GetConfiguredParam()?.ManualCooldown ?? false;

        public override int TotalTimeMs => GetConfiguredParam()?.TotalTimeMs ?? 0;

        public override FP MinCastHpPct
        {
            get
            {
                SkillParam param = GetConfiguredParam();
                return param != null ? (FP)param.MinCastHpPct : FP.Zero;
            }
        }

        public override bool RequireAirborne => GetConfiguredParam()?.RequireAirborne ?? false;

        public override HitReaction HitReaction
            => GetConfiguredParam()?.GetHitReaction(0) ?? HitReaction.Default;

        public override HitReaction PhaseHitReaction(int phase)
            => GetConfiguredParam()?.GetHitReaction(phase) ?? HitReaction.Default;

        public override int[] HitActions => GetConfiguredParam()?.HitActions;

        public override void OnCast(SkillContext ctx)
        {
            SkillParam param = GetConfiguredParam(ctx);
            if (param == null)
            {
                Log.Error($"[SkillParams] skillId={ctx.GetSkillId()} 没有参数，参数化执行器跳过 OnCast");
                return;
            }

            int entryPhase = param.AirborneEntryPhase >= 0 && ctx.IsCasterAirborne()
                ? param.AirborneEntryPhase
                : param.EntryPhase;
            ctx.BeginPhase(entryPhase, 0);
            ctx.ClearHitTargets();

            if (param.CastHpCostPct > 0)
                ctx.ConsumeCasterHp(ctx.GetCasterMaxHp() * param.CastHpCostPct / 100);

            EnterPhase(ctx, param, entryPhase, 0, true);
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            SkillParam param = GetConfiguredParam(ctx);
            if (param == null || param.Phases.Length == 0) return;

            int phaseIndex = ctx.GetSubState();
            if (phaseIndex < 0 || phaseIndex >= param.Phases.Length)
            {
                Log.Error($"[SkillParams] skillId={ctx.GetSkillId()} phase={phaseIndex} 越界，结束施放");
                ctx.EndCast();
                return;
            }

            SkillPhaseParam phase = param.Phases[phaseIndex];
            int elapsed = ctx.GetElapsedMs();
            int previousElapsed = elapsed - dtMs;
            int phaseElapsed = elapsed - ctx.GetPhase();
            int previousPhaseElapsed = phaseElapsed - dtMs;

            if (phase.EndOnLanding && !ctx.IsCasterAirborne())
            {
                ProcessSpawnEvents(ctx, param, phaseIndex, previousElapsed, elapsed,
                    previousPhaseElapsed, phaseElapsed, false, true, true);
                ctx.EndCast();
                return;
            }

            ProcessSpawnEvents(ctx, param, phaseIndex, previousElapsed, elapsed,
                previousPhaseElapsed, phaseElapsed, false, false, false);

            if (UpdateMovement(ctx, phase, dtMs, phaseElapsed))
            {
                TransitionFromPhase(ctx, param, phaseIndex, previousElapsed, elapsed,
                    previousPhaseElapsed, phaseElapsed);
                return;
            }
            bool hadPendingHit = ctx.HasPendingHitTargets();
            if (hadPendingHit && ProcessHitEvents(ctx, param, phaseIndex)) return;

            if (hadPendingHit && phase.NextTrigger == SkillParamNextTrigger.hit)
            {
                TransitionFromPhase(ctx, param, phaseIndex, previousElapsed, elapsed,
                    previousPhaseElapsed, phaseElapsed);
                return;
            }

            if (phase.CancelMs >= 0 && phaseElapsed >= phase.CancelMs
                && (phase.NextTrigger == SkillParamNextTrigger.key
                    || phase.NextTrigger == SkillParamNextTrigger.phaseEnd)
                && ctx.PeekBufferedButton() != 0
                && (phase.CancelButton == 0 || phase.CancelButton == ctx.PeekBufferedButton()))
            {
                ctx.ConsumeBuffer();
                TransitionFromPhase(ctx, param, phaseIndex, previousElapsed, elapsed,
                    previousPhaseElapsed, phaseElapsed);
                return;
            }

            if (phase.DurationMs > 0 && phaseElapsed < phase.DurationMs) return;

            if (phase.NextTrigger == SkillParamNextTrigger.key
                || phase.NextTrigger == SkillParamNextTrigger.hit)
            {
                ProcessSpawnEvents(ctx, param, phaseIndex, previousElapsed, elapsed,
                    previousPhaseElapsed, phaseElapsed, false, true, false);
                ctx.EndCast();
                return;
            }

            TransitionFromPhase(ctx, param, phaseIndex, previousElapsed, elapsed,
                previousPhaseElapsed, phaseElapsed);
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.DisableAttackHitbox();
            ctx.PlayDefaultAnim();
        }

        private static void EnterPhase(SkillContext ctx, SkillParam param, int phaseIndex, int startMs, bool first)
        {
            if (!first) ctx.BeginPhase(phaseIndex, startMs);

            SkillPhaseParam phase = param.Phases[phaseIndex];
            if (phase.ClearHitTargets) ctx.ClearHitTargets();
            ctx.DisableAttackHitbox();
            if (phase.AnimId > 0) ctx.PlayAnim(phase.AnimId);
            if (phase.SuperArmorMs > 0) ctx.SetCasterSuperArmor(phase.SuperArmorMs);
            UpdateManualBoxes(ctx, param, phaseIndex, 0);

            ProcessSpawnEvents(ctx, param, phaseIndex, startMs - 1, startMs,
                -1, 0, true, false, false);
        }

        private static void TransitionFromPhase(
            SkillContext ctx, SkillParam param, int phaseIndex,
            int previousCastMs, int currentCastMs,
            int previousPhaseMs, int currentPhaseMs)
        {
            SkillPhaseParam phase = param.Phases[phaseIndex];
            ProcessSpawnEvents(ctx, param, phaseIndex, previousCastMs, currentCastMs,
                previousPhaseMs, currentPhaseMs, false, true, false);

            if (phase.NextSkillId > 0)
            {
                ctx.EndCast();
                SkillCastHelper.TryCast(ctx.GetCaster(), phase.NextSkillId);
                return;
            }

            int nextPhase = phase.NextPhase >= 0 ? phase.NextPhase : phaseIndex + 1;
            if (nextPhase >= 0 && nextPhase < param.Phases.Length)
                EnterPhase(ctx, param, nextPhase, currentCastMs, false);
            else
                ctx.EndCast();
        }

        /// <summary>
        /// 在帧驱动碰撞系统之后恢复手动盒。这样 phase 切换当帧也能立刻关掉上一段
        /// 的帧盒，同时避免 OnUpdate 早于碰撞系统时手动盒被动画采样覆盖。
        /// </summary>
        public void ApplyPostHitbox(SkillContext ctx)
        {
            SkillParam param = GetConfiguredParam(ctx);
            if (param == null || param.Phases.Length == 0) return;
            int phaseIndex = ctx.GetSubState();
            if (phaseIndex < 0 || phaseIndex >= param.Phases.Length) return;
            UpdateManualBoxes(ctx, param, phaseIndex, ctx.GetElapsedMs() - ctx.GetPhase());
        }

        private static bool UpdateMovement(SkillContext ctx, SkillPhaseParam phase, int dtMs, int phaseElapsed)
        {
            SkillMovementParam movement = phase.Movement;
            if (movement == null || movement.DurationMs <= 0 || phaseElapsed <= 0) return false;
            if (movement.StopOnHit && ctx.AnyEnemyHit())
            {
                return true;
            }

            int remainingMs = movement.DurationMs - phaseElapsed + dtMs;
            if (remainingMs < 0) remainingMs = 0;
            int stepMs = dtMs < remainingMs ? dtMs : remainingMs;
            if (stepMs <= 0) return false;
            ctx.MoveCasterForward(movement.Distance * stepMs / movement.DurationMs);
            return false;
        }

        private static void UpdateManualBoxes(SkillContext ctx, SkillParam param, int phaseIndex, int phaseElapsed)
        {
            SkillManualBoxParam active = null;
            bool hasPhaseBox = false;
            foreach (SkillManualBoxParam box in param.ManualBoxes)
            {
                if (box.Phase != phaseIndex) continue;
                hasPhaseBox = true;
                if (phaseElapsed < box.OnMs || phaseElapsed >= box.OffMs) continue;
                active = box;
                break;
            }

            if (active != null) ctx.SetAttackHitbox(active.Offset, active.Half);
            else if (hasPhaseBox) ctx.DisableAttackHitbox();
        }

        private static void ProcessSpawnEvents(
            SkillContext ctx, SkillParam param, int phaseIndex,
            int previousCastMs, int currentCastMs,
            int previousPhaseMs, int currentPhaseMs,
            bool phaseEntered, bool phaseEnding, bool landed)
        {
            for (int i = 0; i < param.SpawnEvents.Length; i++)
            {
                SkillSpawnEventParam spawn = param.SpawnEvents[i];
                if (spawn.Phase >= 0 && spawn.Phase != phaseIndex) continue;

                bool phaseScoped = spawn.Phase >= 0;
                bool due = false;
                switch (spawn.TimeBase)
                {
                    case SkillParamTimeBase.CastTime:
                        due = spawn.AtMs == 0
                            ? phaseEntered && spawn.Phase >= 0 || previousCastMs < 0 && currentCastMs == 0
                            : previousCastMs < spawn.AtMs && spawn.AtMs <= currentCastMs;
                        break;
                    case SkillParamTimeBase.PhaseTime:
                        due = spawn.AtMs == 0
                            ? phaseEntered
                            : previousPhaseMs < spawn.AtMs && spawn.AtMs <= currentPhaseMs;
                        break;
                    case SkillParamTimeBase.AnimationFrame:
                        due = spawn.AtFrame > 0
                            ? ctx.CurrentFrameIndex() >= spawn.AtFrame
                            : previousPhaseMs < spawn.AtMs && spawn.AtMs <= currentPhaseMs;
                        break;
                    case SkillParamTimeBase.PhaseEnter:
                        due = phaseEntered;
                        break;
                    case SkillParamTimeBase.PhaseEnd:
                        due = phaseEnding;
                        break;
                    case SkillParamTimeBase.Landing:
                        due = landed;
                        break;
                    case SkillParamTimeBase.Input:
                        int inputTime = spawn.Phase >= 0 ? currentPhaseMs : currentCastMs;
                        due = ctx.PeekBufferedButton() == spawn.Button
                            && inputTime >= spawn.AtMs
                            && (spawn.UntilMs < 0 || inputTime <= spawn.UntilMs);
                        break;
                }

                if (!due || !ctx.TryMarkSpawnEvent(i, phaseScoped)) continue;
                ExecuteSpawn(ctx, spawn);
                if (spawn.TimeBase == SkillParamTimeBase.Input && spawn.ConsumeInput)
                    ctx.ConsumeBuffer();
            }
        }

        private static void ExecuteSpawn(SkillContext ctx, SkillSpawnEventParam spawn)
        {
            switch (spawn.Kind)
            {
                case SkillParamSpawnKind.createArea:
                    if (spawn.AreaId <= 0) return;
                    if (spawn.At == SkillParamSpawnAt.inFront)
                        ctx.CreateAreaInFront(spawn.AreaId, spawn.Dist);
                    else if (spawn.At == SkillParamSpawnAt.target)
                        ctx.CreateArea(spawn.AreaId, ctx.GetTargetPosition());
                    else
                        ctx.CreateArea(spawn.AreaId, ctx.GetCaster().Position);
                    break;
                case SkillParamSpawnKind.createBullet:
                    if (spawn.BulletId > 0) ctx.CreateBullet(spawn.BulletId);
                    break;
                case SkillParamSpawnKind.superArmor:
                    if (spawn.DurationMs > 0) ctx.SetCasterSuperArmor(spawn.DurationMs);
                    break;
                case SkillParamSpawnKind.playAnim:
                    if (spawn.AnimId > 0) ctx.PlayAnim(spawn.AnimId);
                    break;
                case SkillParamSpawnKind.addBuff:
                    if (spawn.BuffId <= 0) return;
                    if (spawn.At == SkillParamSpawnAt.target)
                        ctx.AddBuffToLastHitTarget(spawn.BuffId);
                    else
                        ctx.AddBuffToSelf(spawn.BuffId);
                    break;
            }
        }

        private static bool ProcessHitEvents(SkillContext ctx, SkillParam param, int phaseIndex)
        {
            System.Collections.Generic.IReadOnlyList<long> targets = ctx.GetPendingHitTargets();
            if (targets == null || targets.Count == 0) return false;

            bool transitioned = false;
            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                ctx.SelectHitTarget(targets[targetIndex]);
                for (int i = 0; i < param.HitEvents.Length; i++)
                {
                    SkillHitEventParam hitEvent = param.HitEvents[i];
                    if (hitEvent.Phase != phaseIndex) continue;
                    if (!ctx.TryMarkHitEvent(i, hitEvent.HitPolicy, targets[targetIndex])) continue;

                    switch (hitEvent.Kind)
                    {
                        case SkillParamHitEventKind.nextPhase:
                            if (hitEvent.NextPhase >= 0 && hitEvent.NextPhase < param.Phases.Length)
                            {
                                ExitPhaseEvents(ctx, param, phaseIndex);
                                EnterPhase(ctx, param, hitEvent.NextPhase, ctx.GetElapsedMs(), false);
                                transitioned = true;
                                break;
                            }
                            break;
                        case SkillParamHitEventKind.nextSkill:
                            if (hitEvent.NextSkillId > 0)
                            {
                                ExitPhaseEvents(ctx, param, phaseIndex);
                                ctx.EndCast();
                                SkillCastHelper.TryCast(ctx.GetCaster(), hitEvent.NextSkillId);
                                transitioned = true;
                                break;
                            }
                            break;
                        case SkillParamHitEventKind.addBuff:
                            if (hitEvent.BuffId > 0) ctx.AddBuffToLastHitTarget(hitEvent.BuffId);
                            break;
                        case SkillParamHitEventKind.grab:
                            Log.Warning($"[SkillParams] skillId={ctx.GetSkillId()} 的 grab hitEvent 暂未接入 GrabController");
                            break;
                    }

                    if (transitioned) break;
                }

                if (transitioned) break;
            }
            ctx.ClearPendingHitTargets();
            return transitioned;
        }

        private static void ExitPhaseEvents(SkillContext ctx, SkillParam param, int phaseIndex)
        {
            int elapsed = ctx.GetElapsedMs();
            int phaseElapsed = elapsed - ctx.GetPhase();
            ProcessSpawnEvents(ctx, param, phaseIndex,
                elapsed - LSConstValue.UpdateInterval, elapsed,
                phaseElapsed - LSConstValue.UpdateInterval, phaseElapsed,
                false, true, false);
        }
    }
}
