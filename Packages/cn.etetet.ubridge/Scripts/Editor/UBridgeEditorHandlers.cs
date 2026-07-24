using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class UBridgeReloadHandler
    {
        public static string Handle(string p)
        {
            var resp = ReloadResponse.Create();
            if (!EditorApplication.isPlaying) { resp.Error = 7; resp.Message = "Not in PlayMode"; return UBridgeJsonHelper.ToJson(resp); }
            resp.Reloaded = EditorApplication.ExecuteMenuItem("ET/Loader/Reload");
            if (!resp.Reloaded) resp.Message = "Reload failed";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeEditorUndoHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<EditorUndoRequest>(p);
            var resp = EditorUndoResponse.Create();
            int count = r?.Count > 0 ? r.Count : 1;
            for (int i = 0; i < count; i++) Undo.PerformUndo();
            resp.Count = count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeEditorRedoHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<EditorRedoRequest>(p);
            var resp = EditorRedoResponse.Create();
            int count = r?.Count > 0 ? r.Count : 1;
            for (int i = 0; i < count; i++) Undo.PerformRedo();
            resp.Count = count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeEditorPauseHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<EditorPauseRequest>(p);
            var resp = EditorPauseResponse.Create();
            if (r?.Toggle ?? false) EditorApplication.isPaused = !EditorApplication.isPaused;
            else if (r != null) EditorApplication.isPaused = r.Pause;
            resp.IsPaused = EditorApplication.isPaused;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeEditorGetStateHandler
    {
        public static string Handle(string p)
        {
            var resp = EditorGetStateResponse.Create();
            resp.IsPlaying = EditorApplication.isPlaying;
            resp.IsPaused = EditorApplication.isPaused;
            resp.IsCompiling = EditorApplication.isCompiling;
            resp.IsUpdating = EditorApplication.isUpdating;
            resp.ApplicationPath = EditorApplication.applicationPath;
            resp.ApplicationContentsPath = EditorApplication.applicationContentsPath;
            resp.EnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            resp.EnterPlayModeOptions = EditorSettings.enterPlayModeOptions.ToString();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeQueryHostStateHandler
    {
        public static string Handle(string p)
        {
            var resp = HostStateResponse.Create();
            resp.IsCompiling = EditorApplication.isCompiling;
            resp.IsPlaying = EditorApplication.isPlaying;
            resp.IsPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
            resp.CodeMode = ReadCodeMode();
            resp.UnityVersion = Application.unityVersion;
            resp.AvailableCommands = string.Join(",", UBridgeEditorHost.GetRegisteredCommands());
            return UBridgeJsonHelper.ToJson(resp);
        }
        static string ReadCodeMode()
        {
            try { var cfg = Resources.Load("GlobalConfig"); if (cfg) { var f = cfg.GetType().GetField("CodeMode", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (f != null) return f.GetValue(cfg)?.ToString() ?? ""; } } catch { }
            return "";
        }
    }

    public static class UBridgeBatchExecuteHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<BatchExecuteRequest>(p);
            var resp = BatchExecuteResponse.Create();
            if (r?.Commands == null || r.Commands.Count == 0) { resp.Error = 3; resp.Message = "Commands empty"; return UBridgeJsonHelper.ToJson(resp); }
            var handlers = UBridgeEditorHost.GetHandlers();
            for (int i = 0; i < r.Commands.Count; i++)
            {
                var step = BridgeBatchStepResult.Create();
                step.Name = $"Step{i + 1}"; step.Command = r.Commands[i];
                if (string.IsNullOrWhiteSpace(r.Commands[i])) { step.Error = 3; step.Message = "empty"; resp.Results.Add(step); resp.Failed++; if (r.StopOnError) break; continue; }
                try
                {
                    var env = UBridgeJsonHelper.FromJson<UBridgeRequestEnvelope>(r.Commands[i]);
                    if (env == null || string.IsNullOrEmpty(env.Command)) { step.Error = 3; step.Message = "invalid envelope"; }
                    else if (!handlers.TryGetValue(env.Command, out var h)) { step.Error = 3; step.Message = $"unknown: {env.Command}"; }
                    else { h(env.PayloadJson ?? ""); step.Error = 0; step.Message = "ok"; }
                }
                catch (Exception ex) { step.Error = 8; step.Message = ex.Message; }
                resp.Results.Add(step);
                if (step.Error != 0) { resp.Failed++; if (r.StopOnError) break; }
            }
            resp.Count = resp.Results.Count; resp.Completed = resp.Failed == 0;
            if (!resp.Completed) { resp.Error = 8; resp.Message = $"{resp.Failed} failed"; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    // ===== TestEcho + EditorLog + GameView =====

    public static class UBridgeTestEchoHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<TestEcho>(p);
            var resp = TestEchoResponse.Create();
            resp.Text = r?.Text ?? "";
            resp.HandledAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            resp.Handler = "UBridgeTestEchoHandler";
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeEditorLogHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<EditorLogRequest>(p);
            var resp = EditorLogResponse.Create();
            if (string.IsNullOrWhiteSpace(r?.Message)) { resp.Error = 3; resp.Message = "Message required"; return UBridgeJsonHelper.ToJson(resp); }
            string lt = (r?.LogType ?? "Log").Trim().ToLowerInvariant();
            string msg = "[UBridge] " + r.Message;
            if (lt == "error") Debug.LogError(msg);
            else if (lt == "warning") Debug.LogWarning(msg);
            else Debug.Log(msg);
            resp.Logged = true; resp.LogType = lt; resp.LoggedMessage = msg;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameViewGetResolutionHandler
    {
        public static string Handle(string p)
        {
            var resp = GameViewGetResolutionResponse.Create();
            try
            {
                var gvType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                var gv = GVHelper.GetMainGameView(gvType);
                var sizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
                var sizes = sizesType?.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
                var group = sizes?.GetType().GetProperty("currentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(sizes);
                int total = (int)(group?.GetType().GetMethod("GetTotalCount")?.Invoke(group, null) ?? 0);
                int idx = gv == null ? -1 : (int)(gvType?.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(gv) ?? -1);
                if (idx >= 0 && idx < total) { var size = group?.GetType().GetMethod("GetGameViewSize")?.Invoke(group, new object[] { idx }); resp.Resolution = GVHelper.MakeResolution(size, true); }
                else { resp.Error = 8; resp.Message = "No resolution found"; }
                resp.SelectedIndex = idx;
            }
            catch (Exception ex) { resp.Error = 8; resp.Message = ex.Message; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameViewListResolutionsHandler
    {
        public static string Handle(string p)
        {
            var resp = GameViewListResolutionsResponse.Create();
            try
            {
                var sizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
                var sizes = sizesType?.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
                var group = sizes?.GetType().GetProperty("currentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(sizes);
                int total = (int)(group?.GetType().GetMethod("GetTotalCount")?.Invoke(group, null) ?? 0);
                for (int i = 0; i < total; i++) { var size = group?.GetType().GetMethod("GetGameViewSize")?.Invoke(group, new object[] { i }); resp.Resolutions.Add(GVHelper.MakeResolution(size, false)); }
                resp.Count = resp.Resolutions.Count;
                var gvType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                var gv = GVHelper.GetMainGameView(gvType);
                resp.CurrentIndex = gv == null ? -1 : (int)(gvType?.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(gv) ?? -1);
            }
            catch (Exception ex) { resp.Error = 8; resp.Message = ex.Message; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeGameViewSetResolutionHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<GameViewSetResolutionRequest>(p);
            var resp = GameViewSetResolutionResponse.Create();
            if (r?.Width <= 0 || r?.Height <= 0) { resp.Error = 3; resp.Message = "Width/Height positive"; return UBridgeJsonHelper.ToJson(resp); }
            try
            {
                var gvType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                var gv = GVHelper.GetMainGameView(gvType);
                if (gv == null) { resp.Error = 8; resp.Message = "No GameView"; return UBridgeJsonHelper.ToJson(resp); }
                var sizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
                var sizes = sizesType?.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
                var group = sizes?.GetType().GetProperty("currentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(sizes);
                int total = (int)(group?.GetType().GetMethod("GetTotalCount")?.Invoke(group, null) ?? 0);
                int found = -1;
                for (int i = 0; i < total; i++) { var sz = group?.GetType().GetMethod("GetGameViewSize")?.Invoke(group, new object[] { i }); if (GVHelper.GetInt(sz, "width") == r.Width && GVHelper.GetInt(sz, "height") == r.Height) { found = i; break; } }
                if (found >= 0) { gvType?.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(gv, found); }
                else { resp.Error = 8; resp.Message = "Resolution not found"; return UBridgeJsonHelper.ToJson(resp); }
                ((EditorWindow)gv).Repaint();
                resp.SelectedIndex = found;
                var sz2 = group?.GetType().GetMethod("GetGameViewSize")?.Invoke(group, new object[] { found });
                if (sz2 != null) resp.Resolution = GVHelper.MakeResolution(sz2, true);
            }
            catch (Exception ex) { resp.Error = 8; resp.Message = ex.Message; }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    static class GVHelper
    {
        public static object GetMainGameView(Type t) { foreach (var w in Resources.FindObjectsOfTypeAll(t)) { if ((bool)t.GetProperty("isMainGameView", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(w)) return w; } return null; }
        public static BridgeGameViewResolution MakeResolution(object size, bool cur) { var r = BridgeGameViewResolution.Create(); r.Width = GetInt(size, "width"); r.Height = GetInt(size, "height"); r.Label = GetStr(size, "baseText"); r.IsCurrent = cur; return r; }
        public static int GetInt(object o, string f) { return (int)(o?.GetType().GetProperty(f, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(o) ?? 0); }
        public static string GetStr(object o, string f) { return o?.GetType().GetProperty(f, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(o)?.ToString() ?? ""; }
    }
}