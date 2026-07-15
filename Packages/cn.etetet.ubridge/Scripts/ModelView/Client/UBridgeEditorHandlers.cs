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
}