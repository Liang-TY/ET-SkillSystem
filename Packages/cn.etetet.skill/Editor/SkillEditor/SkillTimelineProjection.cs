using System;

namespace ET.Editor
{
    /// <summary>
    /// 唯一时间换算实现（02 §5/§8）。语义与运行时 ParametricSkillLogic.ProcessSpawnEvents
    /// 对齐，不得在 View 中复制公式：
    /// - CastTime：cast 全局毫秒（atMs=0 且带 phase 时等价于该 phase 进入时刻）；
    /// - PhaseTime：phase 局部毫秒（atMs=0 即 phase 进入）；
    /// - AnimationFrame：帧号无固定毫秒位置，只给语义标记，不伪造 ms；
    /// - PhaseEnter / PhaseEnd：phase起止边界；Landing：语义标记；
    /// - Input：窗口 [atMs, untilMs]，untilMs&lt;0 为开放窗口。
    /// ManualBox 恒为 phase 局部 [onMs, offMs)；HitEvent 由命中触发，无固定 ms。
    /// </summary>
    internal sealed class SkillTimelineProjection
    {
        public static readonly SkillTimelineProjection Empty = new(null);

        public enum MarkerKind
        {
            /// <summary>有确定的全局毫秒位置。</summary>
            Fixed,
            /// <summary>窗口 [StartMs, EndMs]；EndMs &lt; 0 表示开放窗口。</summary>
            Span,
            /// <summary>没有确定毫秒，只有触发语义（钉在 StartMs 展示，不算伪造时间点）。</summary>
            Semantic,
        }

        public readonly struct EventMarker
        {
            public EventMarker(MarkerKind kind, int startMs, int endMs, string semantic)
            {
                Kind = kind;
                StartMs = startMs;
                EndMs = endMs;
                Semantic = semantic;
            }

            public MarkerKind Kind { get; }
            public int StartMs { get; }
            public int EndMs { get; }
            public string Semantic { get; }
        }

        private readonly int[] phaseStarts;
        private readonly int[] phaseDurations;

        /// <summary>
        /// 线性计划视图：按 phases[0..n] 顺序累计起点。已知局限——entryPhase≠0 或
        /// 相位回环（如 normalattack 空中分支）时实际播放顺序与线性布局不一致，
        /// phase 重入标记待 Step 3 确定性采样器实现（02 §8）。
        /// </summary>
        public SkillTimelineProjection(SkillParamJson document)
        {
            int count = document?.phases?.Length ?? 0;
            phaseStarts = new int[count];
            phaseDurations = new int[count];
            int cursor = 0;
            for (int i = 0; i < count; i++)
            {
                SkillPhaseJson phase = document.phases[i];
                phaseDurations[i] = phase != null && phase.durationMs > 0 ? phase.durationMs : 0;
                phaseStarts[i] = cursor;
                cursor += phaseDurations[i];
            }
            // 显示范围 = min(相位累计, totalTimeMs) 与 spawn 特效残留的较大者：
            // LSCast 到点结束（相位累计超出 totalTime 的尾段事件不执行），但 createArea/Bullet
            // 是独立实体，触发后按自身 TotalTimeMs 存活——血爆 910ms 触发 + 500ms 残留 = 1410ms。
            // 超出 cast 时长的尾段在时间轴上属"施放已结束、特效仍在播"，画进刻度（拖轴可检）。
            int totalMs = document?.totalTimeMs ?? 0;
            int castEnd = totalMs > 0 ? Math.Min(cursor, totalMs) : cursor;
            int residueEnd = castEnd;
            if (document?.spawnEvents != null)
            {
                foreach (SkillSpawnEventJson spawn in document.spawnEvents)
                {
                    if (spawn == null || (spawn.kind != "createArea" && spawn.kind != "createBullet")) continue;
                    // 近似触发时刻（与窗口 ResolveTriggerMs 同构的简化版：帧时刻/phase 起点含 atMs）
                    int trigger = 0;
                    if (spawn.timeBase == "AnimationFrame" && spawn.atFrame > 0)
                        trigger = spawn.phase >= 0 ? PhaseStart(spawn.phase) : 0;   // 帧精确值由窗口侧算；投影只给下限
                    else if (spawn.timeBase == "PhaseTime" || spawn.timeBase == "PhaseEnter" || spawn.timeBase == "PhaseEnd")
                        trigger = spawn.phase >= 0
                            ? (spawn.timeBase == "PhaseEnd" ? PhaseEnd(spawn.phase) : PhaseStart(spawn.phase) + spawn.atMs)
                            : spawn.atMs;
                    else if (spawn.timeBase == "CastTime")
                        trigger = spawn.atMs;
                    int duration = spawn.durationMs > 0 ? spawn.durationMs : 500;   // Area 默认 500 与数据一致
                    residueEnd = Math.Max(residueEnd, trigger + duration);
                }
            }
            TotalDurationMs = Math.Max(castEnd, residueEnd);   // 至少 1（下方 Max(,1) 兜底）
        }

        public int PhaseCount => phaseStarts.Length;
        public int TotalDurationMs { get; }

        public int PhaseStart(int phase)
            => phase >= 0 && phase < phaseStarts.Length ? phaseStarts[phase] : 0;

        public int PhaseDuration(int phase)
            => phase >= 0 && phase < phaseDurations.Length ? phaseDurations[phase] : 0;

        public int PhaseEnd(int phase) => PhaseStart(phase) + PhaseDuration(phase);

        public int GlobalFromPhaseLocal(int phase, int localMs) => PhaseStart(phase) + Math.Max(0, localMs);

        /// <summary>全局毫秒落在哪个 phase（display 用；超出总时长归最后一段）。</summary>
        public int LocatePhase(int globalMs)
        {
            if (phaseStarts.Length == 0) return -1;
            for (int i = phaseStarts.Length - 1; i >= 0; i--)
            {
                if (globalMs >= phaseStarts[i]) return i;
            }
            return 0;
        }

        public bool HasPhase(int phase) => phase >= 0 && phase < phaseStarts.Length;

        public bool TryGetSpawnMarker(SkillSpawnEventJson spawn, out EventMarker marker)
        {
            marker = default;
            if (spawn == null) return false;
            bool scoped = HasPhase(spawn.phase);
            SkillParamTimeBase timeBase = ParseTimeBase(spawn.timeBase);
            switch (timeBase)
            {
                case SkillParamTimeBase.CastTime:
                    // 运行时：atMs=0 且带 phase 时随 phase 进入触发（事件掩码每进入重置）
                    marker = spawn.atMs == 0 && scoped
                        ? new EventMarker(MarkerKind.Fixed, PhaseStart(spawn.phase), -1, "cast0")
                        : new EventMarker(MarkerKind.Fixed, spawn.atMs, -1, "cast");
                    return true;
                case SkillParamTimeBase.PhaseTime:
                    // phase=-1 的 PhaseTime 在运行时按“当前 phase 的局部时间”判定且每 cast 一次，
                    // 画成单一全局点等于伪造位置——按语义标记展示（02 §2.5）
                    marker = scoped
                        ? new EventMarker(MarkerKind.Fixed, GlobalFromPhaseLocal(spawn.phase, spawn.atMs), -1, "phase")
                        : new EventMarker(MarkerKind.Semantic, GlobalFromPhaseLocal(0, spawn.atMs), -1, $"每段+{spawn.atMs}ms");
                    return true;
                case SkillParamTimeBase.AnimationFrame:
                    if (spawn.atFrame > 0)
                    {
                        marker = new EventMarker(
                            MarkerKind.Semantic, scoped ? PhaseStart(spawn.phase) : 0, -1, $"f{spawn.atFrame}");
                        return true;
                    }
                    // 运行时 atFrame<=0 退化为 PhaseTime
                    goto case SkillParamTimeBase.PhaseTime;
                case SkillParamTimeBase.PhaseEnter:
                    marker = new EventMarker(
                        MarkerKind.Fixed, scoped ? PhaseStart(spawn.phase) : 0, -1, "进入");
                    return true;
                case SkillParamTimeBase.PhaseEnd:
                    marker = new EventMarker(
                        MarkerKind.Fixed, scoped ? PhaseEnd(spawn.phase) : 0, -1, "结束");
                    return true;
                case SkillParamTimeBase.Landing:
                    marker = new EventMarker(
                        MarkerKind.Semantic, scoped ? PhaseStart(spawn.phase) : 0, -1, "落地");
                    return true;
                case SkillParamTimeBase.Input:
                {
                    int start = scoped ? GlobalFromPhaseLocal(spawn.phase, spawn.atMs) : spawn.atMs;
                    int until = spawn.untilMs ?? -1;
                    int end = until < 0 ? -1 : scoped ? GlobalFromPhaseLocal(spawn.phase, until) : until;
                    // phase 内窗口超出 phase 时长部分运行时永不生效，绘制时按 phase 末尾截断
                    if (scoped && end >= 0 && end > PhaseEnd(spawn.phase)) end = PhaseEnd(spawn.phase);
                    marker = new EventMarker(MarkerKind.Span, start, end, $"键{spawn.button}");
                    return true;
                }
                default:
                    return false;
            }
        }

        public bool TryGetManualBoxSpan(SkillManualBoxJson box, out int startMs, out int endMs)
        {
            startMs = endMs = 0;
            if (box == null || !HasPhase(box.phase)) return false;
            startMs = GlobalFromPhaseLocal(box.phase, box.onMs);
            endMs = GlobalFromPhaseLocal(box.phase, Math.Max(box.onMs, box.offMs));
            return true;
        }

        public bool TryGetHitEventMarker(SkillHitEventJson hitEvent, out EventMarker marker)
        {
            marker = default;
            if (hitEvent == null || !HasPhase(hitEvent.phase)) return false;
            marker = new EventMarker(
                MarkerKind.Semantic, PhaseStart(hitEvent.phase), -1,
                $"{hitEvent.on}/{hitEvent.hitPolicy}");
            return true;
        }

        private static SkillParamTimeBase ParseTimeBase(string value)
            => Enum.TryParse(value, true, out SkillParamTimeBase result)
                ? result
                : SkillParamTimeBase.CastTime;
    }
}
