using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, Sprite> Sprites = new();
        public Texture2D Atlas;     // 单张运行时图集（替换原来的 List<Texture2D>）
    }
}
