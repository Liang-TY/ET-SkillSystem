using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, Sprite> Sprites = new();
        public List<Texture2D> Textures = new();
    }
}
