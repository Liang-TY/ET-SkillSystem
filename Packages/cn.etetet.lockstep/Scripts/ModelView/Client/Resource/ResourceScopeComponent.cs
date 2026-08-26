using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 资源作用域组件：管理按需加载/卸载的 IMG 图集作用域（方案文档 §4）。
    /// 数据归实体（ET 无状态红线），行为归 System。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class ResourceScopeComponent: Entity, IAwake, IDestroy
    {
        /// <summary>已加载的 IMG：虚拟路径/文件名 → 加载信息</summary>
        public Dictionary<string, LoadedImgInfo> LoadedImgs = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>作用域 key → 该作用域加载的 IMG 文件名集合</summary>
        public Dictionary<string, HashSet<string>> ScopePaths = new();
    }

    /// <summary>已加载 IMG 的元数据</summary>
    public class LoadedImgInfo
    {
        public string AtlasKey;
        public HashSet<string> Scopes = new();
        public Texture2D Texture;
        public Dictionary<int, Sprite> Sprites;
        public Dictionary<int, Vector2> Centers;
    }
}
