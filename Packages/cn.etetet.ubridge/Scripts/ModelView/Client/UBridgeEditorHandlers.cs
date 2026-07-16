using System;
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
}