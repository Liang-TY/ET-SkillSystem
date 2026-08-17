using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, Sprite> Sprites = new();
        public Texture2D Atlas;     // 单张运行时图集（替换原来的 List<Texture2D>）

        // 每帧摆位修正（像素，相对校准帧 = 图集首个实体帧；建图集时算好，8 字节/精灵）。
        // DNF 的 imagePos 锚定的是帧画布，内容中心真实位置 = imagePos + X + 宽/2（y 取反）；
        // Sprite 是按紧致内容框裁的（中心 pivot），摆位必须补内容偏移，否则换动作就漂移
        // （膝踢帧 X≈289 vs Idle 216，左漂 81px 就是这个坑）。见 02-坐标系与包围盒-总结.md §2.1
        public Dictionary<int, Vector2> FrameOffsets = new();
    }
}
