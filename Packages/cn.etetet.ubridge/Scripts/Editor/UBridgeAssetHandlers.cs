using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class UBridgeAssetSearchHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AssetSearchRequest>(p);
            var resp = AssetSearchResponse.Create();
            string filter = r?.Filter ?? "";
            string type = r?.Type ?? "";
            int max = r?.MaxResults > 0 ? r.MaxResults : 50;
            string[] guids = AssetDatabase.FindAssets(filter);
            foreach (var g in guids)
            {
                if (resp.Assets.Count >= max) break;
                string path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj == null) continue;
                if (!string.IsNullOrEmpty(type) && !obj.GetType().Name.Contains(type)) continue;
                var a = BridgeAssetInfo.Create();
                a.Path = path; a.Guid = g; a.Name = obj.name; a.Type = obj.GetType().Name;
                resp.Assets.Add(a);
            }
            resp.Count = resp.Assets.Count;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeAssetFindHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AssetFindRequest>(p);
            var resp = AssetFindResponse.Create();
            string path = r?.AssetPath;
            if (!string.IsNullOrEmpty(r?.Guid)) path = AssetDatabase.GUIDToAssetPath(r.Guid);
            if (string.IsNullOrEmpty(path)) { resp.Error = 3; resp.Message = "Asset not found"; return UBridgeJsonHelper.ToJson(resp); }
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj == null) { resp.Error = 3; resp.Message = "Asset not found"; return UBridgeJsonHelper.ToJson(resp); }
            var info = BridgeAssetInfo.Create();
            info.Path = path; info.Guid = AssetDatabase.AssetPathToGUID(path); info.Name = obj.name; info.Type = obj.GetType().Name;
            resp.Asset = info;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeAssetGetPathHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AssetGetPathRequest>(p);
            var resp = AssetGetPathResponse.Create();
            resp.AssetPath = AssetDatabase.GUIDToAssetPath(r?.Guid ?? "");
            if (!string.IsNullOrEmpty(resp.AssetPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resp.AssetPath);
                var info = BridgeAssetInfo.Create();
                info.Path = resp.AssetPath; info.Guid = r?.Guid ?? ""; info.Name = obj?.name ?? ""; info.Type = obj?.GetType().Name ?? "";
                resp.Asset = info;
            }
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeAssetLoadHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AssetLoadRequest>(p);
            var resp = AssetLoadResponse.Create();
            resp.AssetPath = r?.AssetPath ?? "";
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resp.AssetPath);
            var info = BridgeAssetInfo.Create();
            info.Path = resp.AssetPath; info.Guid = AssetDatabase.AssetPathToGUID(resp.AssetPath); info.Name = obj?.name ?? ""; info.Type = obj?.GetType().Name ?? "";
            resp.Asset = info;
            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeAssetReadTextHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<AssetReadTextRequest>(p);
            var resp = AssetReadTextResponse.Create();
            resp.AssetPath = r?.AssetPath ?? "";
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), resp.AssetPath);
            if (!File.Exists(fullPath)) { resp.Error = 3; resp.Message = "File not found"; return UBridgeJsonHelper.ToJson(resp); }
            string text = File.ReadAllText(fullPath, Encoding.UTF8);
            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            resp.TotalLines = lines.Length;
            int start = r?.StartLine > 0 ? r.StartLine : 1;
            int maxLines = r?.MaxLines > 0 ? r.MaxLines : 200;
            int maxChars = r?.MaxChars > 0 ? r.MaxChars : 12000;
            resp.ReturnedLineStart = start;
            var sb = new StringBuilder(); int count = 0;
            for (int i = start - 1; i < lines.Length && count < maxLines && sb.Length < maxChars; i++, count++)
            {
                if (count > 0) sb.Append('\n');
                sb.Append($"{i + 1}: {lines[i]}");
            }
            resp.ReturnedLineEnd = start + count - 1; resp.ReturnedLineCount = count;
            resp.Truncated = count < (lines.Length - start + 1); resp.Content = sb.ToString();
            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}