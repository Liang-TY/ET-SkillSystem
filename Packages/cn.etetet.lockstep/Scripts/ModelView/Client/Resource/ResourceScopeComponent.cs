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
        /// <summary>已加载的 IMG：文件名 → 图集 key</summary>
        public Dictionary<string, string> LoadedAtlasKeys = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>作用域 key → 该作用域加载的 IMG 文件名集合</summary>
        public Dictionary<string, HashSet<string>> ScopePaths = new();

        /// <summary>IMG 文件名 → 引用该 IMG 的作用域 key 集合</summary>
        public Dictionary<string, HashSet<string>> ImgScopes = new(System.StringComparer.OrdinalIgnoreCase);
    }
}

