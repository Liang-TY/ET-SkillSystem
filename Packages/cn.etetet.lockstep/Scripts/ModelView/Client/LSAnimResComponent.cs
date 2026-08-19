using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        /// <summary>多图集：图集名（=动画 json 的 image.path 文件名，忽略大小写）→ 帧索引 → Sprite</summary>
        public Dictionary<string, Dictionary<int, Sprite>> Atlases = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>同 key 结构：每帧"内容中心在画布内"绝对坐标（像素 = X+宽/2, Y+高/2）。
        /// 摆位用 §2.1 绝对公式：renderer local = (imagePos+中心，y 翻转)/100 − prefab中间层偏移（运行时自标定）</summary>
        public Dictionary<string, Dictionary<int, Vector2>> AtlasCenters = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>LINEARDODGE 加法混合材质（ET/SpriteAdditive shader），InitAsync 时创建共享</summary>
        public Material AdditiveMaterial;
    }
}
