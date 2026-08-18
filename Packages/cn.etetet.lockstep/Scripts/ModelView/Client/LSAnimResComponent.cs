using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        /// <summary>多图集：图集名（=动画 json 的 image.path 文件名，忽略大小写）→ 帧索引 → Sprite</summary>
        public Dictionary<string, Dictionary<int, Sprite>> Atlases = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>同 key 结构：每帧"内容中心相对画布中心"偏移（像素，见 02-坐标系 §2.1 锚点修复）</summary>
        public Dictionary<string, Dictionary<int, Vector2>> AtlasOffsets = new(System.StringComparer.OrdinalIgnoreCase);
    }
}
