using System.Collections.Generic;
using System.Linq;

namespace ET.Client
{
    /// <summary>
    /// 资源源类型定义（方案文档 §14）。
    /// 每种源类型是一个静态方法：输入 ScopeContext → 输出 IMG 名字集合。
    /// 加新源类型 = 加一个 case，不改接口。
    /// </summary>
    public static class ResourceSourceTypes
    {
        /// <summary>作用域上下文：当前场景的参数（mapId/classId/townId/eventId）</summary>
        public struct ScopeContext
        {
            public string ScopeType;   // "dungeon" / "town" / "character" / ...
            public string ScopeId;    // "15089" / "swordman" / "default" / ...
        }

        /// <summary>
        /// 执行一个源类型收集。
        /// sourceType = 源类型名，sourceParam = JSON 里的额外参数（ids/list/scope 等）。
        /// </summary>
        public static HashSet<string> Collect(string sourceType, object sourceParam, ScopeContext ctx, Room room)
        {
            HashSet<string> result = new(System.StringComparer.OrdinalIgnoreCase);

            switch (sourceType)
            {
                case "static_list":
                    if (sourceParam is List<string> list)
                        foreach (string s in list) result.Add(s);
                    break;

                case "anim_ids":
                    if (sourceParam is List<int> ids)
                        foreach (int id in ids)
                            CollectFromAnimId(id, result);
                    break;

                case "anim_all":
                    foreach (var (_, clip) in AnimConfigRegistry.GetAll())
                        if (clip?.frames != null)
                            foreach (var frame in clip.frames)
                            {
                                string path = frame.image.path;
                                if (!string.IsNullOrEmpty(path)) result.Add(path);
                            }
                    break;

                case "character_body":
                    // TODO: 从角色配置读（classId → body IMG）
                    // 当前只有鬼剑士
                    result.Add("sm_body0000.img");
                    break;

                case "character_weapon":
                    // TODO: 从角色配置读（classId + weaponType → weapon IMG）
                    result.Add("katana9200b.img");
                    result.Add("katana9200c.img");
                    break;

                case "character_skills":
                    // 该职业全部技能的动画 IMG（含特效）
                    // 当前：遍历所有已注册动画（后续按职业过滤）
                    foreach (var (_, clip) in AnimConfigRegistry.GetAll())
                        if (clip?.frames != null)
                            foreach (var frame in clip.frames)
                            {
                                string path = frame.image.path;
                                if (!string.IsNullOrEmpty(path)) result.Add(path);
                            }
                    break;

                case "map_monsters":
                    CollectMapMonsters(ctx, room, result);
                    break;

                case "map_tiles":
                    CollectMapTiles(ctx, room, result);
                    break;

                case "tile_layout":
                    CollectTileLayout(ctx, room, result);
                    break;

                case "event_resources":
                    // TODO: 从活动配置读（eventId → 资源列表）
                    break;

                // scope_ref 不在这里处理（需要递归 LoadScope，在 LoadScope 层处理）
            }

            return result;
        }

        private static void CollectFromAnimId(int animId, HashSet<string> result)
        {
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip?.frames == null) return;
            foreach (var frame in clip.frames)
            {
                string path = frame.image.path;
                if (!string.IsNullOrEmpty(path)) result.Add(path);
            }
        }

        private static void CollectMapMonsters(ScopeContext ctx, Room room, HashSet<string> result)
        {
            // MapDefinition.MonsterAiIds → 怪物动画 → IMG
            // TODO: 从 MapDefinition 读 MonsterAiIds，再查怪物对应的动画
            // 当前：MapDefinition 在 room.Init 时已加载，但 MonsterAi → 动画 的映射还没建
            // 暂时走 anim_all 兜底
            foreach (var (_, clip) in AnimConfigRegistry.GetAll())
                if (clip?.frames != null)
                    foreach (var frame in clip.frames)
                    {
                        string path = frame.image.path;
                        if (!string.IsNullOrEmpty(path)) result.Add(path);
                    }
        }

        private static void CollectMapTiles(ScopeContext ctx, Room room, HashSet<string> result)
        {
            // TODO: 从地图 tile layout 读 imgPath
            // 当前由 TownMapViewComponent / LSMapViewComponent 自己处理
        }

        private static void CollectTileLayout(ScopeContext ctx, Room room, HashSet<string> result)
        {
            // TODO: 从城镇 tile layout 读 imgPath
            // 当前由 TownMapViewComponent 自己处理
        }
    }
}
