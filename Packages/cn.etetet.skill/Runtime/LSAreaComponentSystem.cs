using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 区域容器系统（Route B 标记清理 + 回收，同 LSSkillComponentSystem/LSBuffComponentSystem 模式）。
    /// </summary>
    [EntitySystemOf(typeof(LSAreaComponent))]
    [LSEntitySystemOf(typeof(LSAreaComponent))]
    [FriendOf(typeof(LSAreaComponent))]
    [FriendOf(typeof(LSArea))]
    public static partial class LSAreaComponentSystem
    {
        [StaticField]
        private static readonly List<LSArea> areaScratch = new();

        [EntitySystem]
        private static void Awake(this LSAreaComponent self)
        {
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSAreaComponent self)
        {
            // 1) 回收 Removing 的区域（标记已被视图读一帧）+ 清存活区域的上一帧标记
            areaScratch.Clear();
            foreach (var kv in self.Children)
            {
                if (kv.Value is LSArea area) areaScratch.Add(area);
            }
            foreach (LSArea area in areaScratch)
            {
                if (area.Removing)
                {
                    area.Dispose();
                    continue;
                }
                area.JustAdded = area.JustRemoved = false;
            }
            // 2) 子区域的 LSUpdate 由 LSEntitySystemOf(LSArea) 自动驱动（不需要这里手动调）
        }
    }
}
