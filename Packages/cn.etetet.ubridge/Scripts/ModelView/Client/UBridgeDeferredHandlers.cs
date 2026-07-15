using System;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class UBridgeCompileHandler
    {
        public static string Handle(string p)
        {
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (EditorApplication.isCompiling) throw new DeferredNotReady();
                var resp = CompileResponse.Create();
                resp.DurationMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - UBridgeDeferredRuntime.GetStartedAt();
                if (EditorUtility.scriptCompilationFailed) { resp.Error = 8; resp.Message = "Compilation failed"; }
                UBridgeDeferredRuntime.Clear();
                return UBridgeJsonHelper.ToJson(resp);
            }
            if (EditorApplication.isCompiling) { var er = CompileResponse.Create(); er.Error = 8; er.Message = "Already compiling"; return UBridgeJsonHelper.ToJson(er); }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UBridgeDeferredRuntime.SavePending(p, "Compile", 180000);
            throw new DeferredStarted();
        }
    }

    public static class UBridgeRefreshHandler
    {
        public static string Handle(string p)
        {
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (EditorApplication.isCompiling) throw new DeferredNotReady();
                var resp = RefreshResponse.Create();
                if (EditorUtility.scriptCompilationFailed) { resp.Error = 8; resp.Message = "Refresh/compile failed"; }
                UBridgeDeferredRuntime.Clear();
                return UBridgeJsonHelper.ToJson(resp);
            }
            if (EditorApplication.isCompiling) { var er = RefreshResponse.Create(); er.Error = 8; er.Message = "Already compiling"; return UBridgeJsonHelper.ToJson(er); }
            var r = UBridgeJsonHelper.FromJson<Refresh>(p);
            AssetDatabase.Refresh(r?.ForceUpdate ?? false ? ImportAssetOptions.ForceUpdate : ImportAssetOptions.Default);
            UBridgeDeferredRuntime.SavePending(p, "Refresh", 60000);
            throw new DeferredStarted();
        }
    }

    public static class UBridgeRegenProjectHandler
    {
        public static string Handle(string p)
        {
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (EditorApplication.isCompiling) throw new DeferredNotReady();
                var resp = RegenProjectResponse.Create();
                if (EditorUtility.scriptCompilationFailed) { resp.Error = 8; resp.Message = "Regen/compile failed"; }
                UBridgeDeferredRuntime.Clear();
                return UBridgeJsonHelper.ToJson(resp);
            }
            if (EditorApplication.isCompiling) { var er = RegenProjectResponse.Create(); er.Error = 8; er.Message = "Already compiling"; return UBridgeJsonHelper.ToJson(er); }
            EditorApplication.ExecuteMenuItem("ET/Loader/ReGenerateProjectFiles");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UBridgeDeferredRuntime.SavePending(p, "RegenProject", 60000);
            throw new DeferredStarted();
        }
    }

    public static class UBridgeEnterPlayHandler
    {
        public static string Handle(string p)
        {
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying) throw new DeferredNotReady();
                if (EditorApplication.isCompiling && !EditorApplication.isPlaying) throw new DeferredNotReady();
                var resp = EnterPlayResponse.Create(); resp.IsPlaying = EditorApplication.isPlaying;
                if (!resp.IsPlaying) resp.Error = 8;
                UBridgeDeferredRuntime.Clear();
                return UBridgeJsonHelper.ToJson(resp);
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) { var er = EnterPlayResponse.Create(); er.Error = 8; er.Message = "Already in/entering PlayMode"; return UBridgeJsonHelper.ToJson(er); }
            EditorApplication.isPlaying = true;
            UBridgeDeferredRuntime.SavePending(p, "EnterPlay", 60000);
            throw new DeferredStarted();
        }
    }

    public static class UBridgeExitPlayHandler
    {
        public static string Handle(string p)
        {
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new DeferredNotReady();
                var resp = ExitPlayResponse.Create(); resp.IsPlaying = EditorApplication.isPlaying;
                UBridgeDeferredRuntime.Clear();
                return UBridgeJsonHelper.ToJson(resp);
            }
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) { var er = ExitPlayResponse.Create(); er.Error = 7; er.Message = "Not in PlayMode"; return UBridgeJsonHelper.ToJson(er); }
            EditorApplication.isPlaying = false;
            UBridgeDeferredRuntime.SavePending(p, "ExitPlay", 60000);
            throw new DeferredStarted();
        }
    }

    static class DeferredHelper
    {
        public static string ExtractRpcId(string p)
        {
            try { int i = p.IndexOf("\"RpcId\":") + 8; int e = p.IndexOf(',', i); if (e < 0) e = p.IndexOf('}', i); return p.Substring(i, e - i).Trim(); } catch { return "0"; }
        }
    }
}