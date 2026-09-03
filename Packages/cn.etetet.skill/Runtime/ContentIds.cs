using System;
using System.Collections.Generic;

namespace ET
{
    public enum ContentIdKind
    {
        Skill,
        Bullet,
        Area,
        Buff,
        Action,
        Animation,
    }

    /// <summary>
    /// 只用于 Editor 展示、搜索和诊断的 id -> name 目录。
    /// 运行时引用永远直接保存/查询整数 ID，不提供 name -> id 转换。
    /// </summary>
    public static class ContentIds
    {
        [StaticField]
        private static readonly Dictionary<ContentIdKind, Dictionary<int, string>> names = new();

        public static void Register(ContentIdKind kind, int id, string name)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(name)) return;
            Dictionary<int, string> map = GetOrCreate(kind);
            if (map.TryGetValue(id, out string previous) && !string.Equals(previous, name, StringComparison.Ordinal))
                Log.Warning($"[ContentIds] {kind} id={id} 名称由 {previous} 更新为 {name}");
            map[id] = name;
        }

        public static void Register(Type contentBaseType, int id, string name)
        {
            if (contentBaseType == typeof(SkillLogic)) Register(ContentIdKind.Skill, id, name);
            else if (contentBaseType == typeof(BulletDefinition)) Register(ContentIdKind.Bullet, id, name);
            else if (contentBaseType == typeof(AreaDefinition)) Register(ContentIdKind.Area, id, name);
            else if (contentBaseType == typeof(BuffDefinition)) Register(ContentIdKind.Buff, id, name);
            else if (contentBaseType == typeof(LSAction)) Register(ContentIdKind.Action, id, name);
        }

        public static string GetName(ContentIdKind kind, int id)
        {
            Dictionary<int, string> map = GetOrCreate(kind);
            return map.TryGetValue(id, out string name) ? name : null;
        }

        public static IReadOnlyDictionary<int, string> GetAll(ContentIdKind kind) => GetOrCreate(kind);

        /// <summary>动画注册完成后调用；用首帧资源路径作为显示名。</summary>
        public static void RefreshAnimations()
        {
            foreach ((int animId, AnimClipData data) in AnimConfigRegistry.GetAll())
            {
                string name = $"Anim {animId}";
                if (data?.frames is { Length: > 0 })
                {
                    string path = data.frames[0].image.path;
                    if (!string.IsNullOrWhiteSpace(path)) name = path;
                }
                Register(ContentIdKind.Animation, animId, name);
            }
        }

        private static Dictionary<int, string> GetOrCreate(ContentIdKind kind)
        {
            if (!names.TryGetValue(kind, out Dictionary<int, string> map))
            {
                map = new Dictionary<int, string>();
                names.Add(kind, map);
            }
            return map;
        }
    }
}
