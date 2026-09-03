using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ET.Editor
{
    /// <summary>
    /// 轻量自绘时间轴（事件数量少时避免为每个标记创建 VisualElement）。
    /// 只读消费 SkillTimelineProjection，通过回调上报播放头，不改 DTO、不复制时间公式。
    /// 吸附只作用于播放头拖动，绝不改写既有数据（GoreCross untilMs=1390 等非网格值保持原样）。
    /// </summary>
    internal sealed class SkillTimelineElement : VisualElement
    {
        private SkillParamJson document;
        private SkillTimelineProjection projection = SkillTimelineProjection.Empty;
        private int currentTimeMs;
        private float zoom = 1f;
        private bool dragging;
        private int pointerId = -1;

        public Action<int> TimeChanged;
        public bool SnapToTick { get; set; } = true;

        private static readonly Color BackgroundColor = new(0.055f, 0.065f, 0.08f, 1f);
        private static readonly Color GridColor = new(0.18f, 0.2f, 0.24f, 1f);
        private static readonly Color PlayheadColor = new(1f, 0.82f, 0.2f, 1f);
        private static readonly Color SpawnColor = new(1f, 0.52f, 0.16f, 1f);
        private static readonly Color SpawnSpanColor = new(1f, 0.52f, 0.16f, 0.45f);
        private static readonly Color HitColor = new(1f, 0.9f, 0.25f, 1f);
        private static readonly Color BoxColor = new(0.2f, 0.82f, 0.42f, 0.82f);
        private static readonly Color PhaseBorderColor = new(0.5f, 0.6f, 0.7f, 0.8f);
        private static readonly Color[] PhaseColors =
        {
            new(0.12f, 0.24f, 0.34f, 0.65f),
            new(0.16f, 0.27f, 0.22f, 0.65f),
            new(0.29f, 0.2f, 0.29f, 0.65f),
        };

        public SkillTimelineElement()
        {
            style.minHeight = 150;
            style.flexGrow = 0;
            generateVisualContent += GenerateVisualContent;
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(_ => dragging = false);
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
        }

        public SkillTimelineProjection Projection { get; private set; } = SkillTimelineProjection.Empty;

        public int CurrentTimeMs
        {
            get => currentTimeMs;
            set
            {
                currentTimeMs = Mathf.Clamp(value, 0, DurationMs);
                MarkDirtyRepaint();
            }
        }

        public int DurationMs => Mathf.Max(1, Projection.TotalDurationMs);

        public void SetDocument(SkillParamJson value)
        {
            document = value;
            Projection = value == null ? SkillTimelineProjection.Empty : new SkillTimelineProjection(value);
            currentTimeMs = Mathf.Clamp(currentTimeMs, 0, DurationMs);
            MarkDirtyRepaint();
        }

        private float TimelineWidth => Mathf.Max(1f, contentRect.width - 12f) * zoom;

        private float ToX(int timeMs)
            => 6f + TimelineWidth * Mathf.Clamp01(timeMs / (float)DurationMs);

        private int ToTime(float x)
        {
            float normalized = Mathf.Clamp01((x - 6f) / TimelineWidth);
            int time = Mathf.RoundToInt(normalized * DurationMs);
            if (SnapToTick) time = Mathf.RoundToInt(time / 50f) * 50;
            return Mathf.Clamp(time, 0, DurationMs);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            dragging = true;
            pointerId = evt.pointerId;
            PointerCaptureHelper.CapturePointer(this, pointerId);
            CurrentTimeMs = ToTime(evt.localPosition.x);
            TimeChanged?.Invoke(CurrentTimeMs);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!dragging || evt.pointerId != pointerId) return;
            CurrentTimeMs = ToTime(evt.localPosition.x);
            TimeChanged?.Invoke(CurrentTimeMs);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != pointerId) return;
            dragging = false;
            PointerCaptureHelper.ReleasePointer(this, pointerId);
            pointerId = -1;
        }

        private void OnWheel(WheelEvent evt)
        {
            zoom = Mathf.Clamp(zoom * (evt.delta.y < 0 ? 1.12f : 0.89f), 0.65f, 6f);
            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            Rect rect = contentRect;
            DrawRect(painter, 0, 0, rect.width, rect.height, BackgroundColor);
            if (document == null) return;

            DrawGrid(painter);
            DrawPhases(painter);
            DrawManualBoxes(painter);
            DrawSpawnEvents(painter);
            DrawHitEvents(painter);

            float playhead = ToX(currentTimeMs);
            painter.strokeColor = PlayheadColor;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(playhead, 0));
            painter.LineTo(new Vector2(playhead, rect.height));
            painter.Stroke();
        }

        private void DrawGrid(Painter2D painter)
        {
            painter.strokeColor = GridColor;
            painter.lineWidth = 1f;
            int step = DurationMs > 3000 ? 500 : 100;
            for (int time = 0; time <= DurationMs; time += step)
            {
                float x = ToX(time);
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0));
                painter.LineTo(new Vector2(x, contentRect.height));
                painter.Stroke();
            }
        }

        private void DrawPhases(Painter2D painter)
        {
            for (int i = 0; i < Projection.PhaseCount; i++)
            {
                int start = Projection.PhaseStart(i);
                int end = Projection.PhaseEnd(i);
                DrawRect(painter, ToX(start), 18, Mathf.Max(1, ToX(end) - ToX(start)), 32,
                    PhaseColors[i % PhaseColors.Length]);
                painter.strokeColor = PhaseBorderColor;
                painter.lineWidth = 1f;
                painter.BeginPath();
                painter.MoveTo(new Vector2(ToX(start), 18));
                painter.LineTo(new Vector2(ToX(start), contentRect.height));
                painter.Stroke();
            }
        }

        private void DrawManualBoxes(Painter2D painter)
        {
            if (document?.manualBoxes == null) return;
            foreach (SkillManualBoxJson box in document.manualBoxes)
            {
                if (!Projection.TryGetManualBoxSpan(box, out int startMs, out int endMs)) continue;
                float x = ToX(startMs);
                DrawRect(painter, x, 58, Mathf.Max(2, ToX(endMs) - x), 18, BoxColor);
            }
        }

        private void DrawSpawnEvents(Painter2D painter)
        {
            if (document?.spawnEvents == null) return;
            foreach (SkillSpawnEventJson spawn in document.spawnEvents)
            {
                if (!Projection.TryGetSpawnMarker(spawn, out SkillTimelineProjection.EventMarker marker)) continue;
                switch (marker.Kind)
                {
                    case SkillTimelineProjection.MarkerKind.Fixed:
                        DrawDiamond(painter, ToX(marker.StartMs), 87, 5, SpawnColor);
                        break;
                    case SkillTimelineProjection.MarkerKind.Span:
                    {
                        float x = ToX(marker.StartMs);
                        float end = marker.EndMs < 0 ? contentRect.width - 6 : ToX(marker.EndMs);
                        DrawRect(painter, x, 84, Mathf.Max(2, end - x), 6, SpawnSpanColor);
                        break;
                    }
                    case SkillTimelineProjection.MarkerKind.Semantic:
                        DrawCircle(painter, ToX(marker.StartMs), 87, 4, SpawnColor);
                        break;
                }
            }
        }

        private void DrawHitEvents(Painter2D painter)
        {
            if (document?.hitEvents == null) return;
            foreach (SkillHitEventJson hitEvent in document.hitEvents)
            {
                if (!Projection.TryGetHitEventMarker(hitEvent, out SkillTimelineProjection.EventMarker marker)) continue;
                DrawDiamond(painter, ToX(marker.StartMs), 108, 5, HitColor);
            }
        }

        private static void DrawRect(Painter2D painter, float x, float y, float width, float height, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, y));
            painter.LineTo(new Vector2(x + width, y));
            painter.LineTo(new Vector2(x + width, y + height));
            painter.LineTo(new Vector2(x, y + height));
            painter.ClosePath();
            painter.Fill();
        }

        private static void DrawDiamond(Painter2D painter, float x, float y, float size, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, y - size));
            painter.LineTo(new Vector2(x + size, y));
            painter.LineTo(new Vector2(x, y + size));
            painter.LineTo(new Vector2(x - size, y));
            painter.ClosePath();
            painter.Fill();
        }

        private static void DrawCircle(Painter2D painter, float x, float y, float radius, Color color)
        {
            painter.strokeColor = color;
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            painter.Arc(new Vector2(x, y), radius, 0f, Mathf.PI * 2f);
            painter.Stroke();
        }
    }
}
