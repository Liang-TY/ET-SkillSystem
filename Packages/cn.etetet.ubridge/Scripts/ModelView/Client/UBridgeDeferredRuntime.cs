using System;
using System.IO;
using UnityEngine;

namespace ET
{
    [EnableClass]
    class DeferredStarted : Exception { }
    [EnableClass]
    class DeferredNotReady : Exception { }

    /// <summary>延迟命令泵送</summary>
    public static class UBridgeDeferredRuntime
    {
        [StaticField] static string s_PendingJson;
        [StaticField] static string s_PendingRpcId;
        [StaticField] static long s_StartedAt;
        [StaticField] static int s_TimeoutMs;
        [StaticField] static string s_CommandName;

        [StaticField]
        static string StateFile => Path.Combine(UBridgePathHelper.ResolveRoot(), "state", "pending.json");

        [StaticField] static string s_RequestRpcId;
        public static void SetRequestRpcId(string rpcId) { s_RequestRpcId = rpcId; }

        [StaticField]
        public static bool HasPending
        {
            get
            {
                if (!string.IsNullOrEmpty(s_PendingRpcId)) return true;
                if (File.Exists(StateFile)) Restore();
                return !string.IsNullOrEmpty(s_PendingRpcId);
            }
        }

        public static void SavePending(string json, string command, int timeoutMs)
        {
            s_PendingRpcId = s_RequestRpcId; s_PendingJson = json; s_CommandName = command;
            s_TimeoutMs = timeoutMs; s_StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Directory.CreateDirectory(Path.GetDirectoryName(StateFile));
            File.WriteAllText(StateFile, $"{s_PendingRpcId}|{command}|{s_StartedAt}|{timeoutMs}|{json}");
        }

        public static void Clear() { s_PendingRpcId = null; s_PendingJson = null; if (File.Exists(StateFile)) File.Delete(StateFile); }

        static void Restore()
        {
            if (!File.Exists(StateFile)) return;
            try
            {
                string s = File.ReadAllText(StateFile);
                int i1 = s.IndexOf('|'); int i2 = s.IndexOf('|', i1 + 1); int i3 = s.IndexOf('|', i2 + 1); int i4 = s.IndexOf('|', i3 + 1);
                s_PendingRpcId = s.Substring(0, i1);
                s_CommandName = s.Substring(i1 + 1, i2 - i1 - 1);
                s_StartedAt = long.Parse(s.Substring(i2 + 1, i3 - i2 - 1));
                s_TimeoutMs = int.Parse(s.Substring(i3 + 1, i4 - i3 - 1));
                s_PendingJson = s.Substring(i4 + 1);
            }
            catch { File.Delete(StateFile); }
        }

        public static string GetPendingPayload() { Restore(); return s_PendingJson; }
        public static string GetPendingCommand() { Restore(); return s_CommandName; }
        public static string GetPendingRpcId() { Restore(); return s_PendingRpcId; }
        public static long GetStartedAt() { return s_StartedAt; }

        public static bool IsTimeout()
        {
            long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_StartedAt;
            if (elapsed > s_TimeoutMs) { UBridgeFileStore.WriteResponse(s_PendingRpcId, $"{{\"Error\":1,\"Message\":\"Timeout\"}}"); Clear(); return true; }
            return false;
        }
    }
}