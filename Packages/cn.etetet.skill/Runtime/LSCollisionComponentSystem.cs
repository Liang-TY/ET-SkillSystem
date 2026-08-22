using TrueSync;

namespace ET
{
    /// <summary>
    /// 网格碰撞系统：IsBlocked 查矩阵（世界坐标→格子）；TryMove 按轴滑动移动（被挡轴回退）。
    /// 消费方：LSInputComponentSystem（玩家移动）/ LSMonsterAIComponentSystem（追击）/ LSBulletSystem（弹撞墙）。
    /// 放 ET.Skill（同 LSBulletSystem）：弹的撞墙检查在本程序集，而 ET.Skill 不得引用 ET.Hotfix（循环依赖）。
    /// </summary>
    [EntitySystemOf(typeof(LSCollisionComponent))]
    [FriendOf(typeof(LSCollisionComponent))]
    [FriendOf(typeof(LSUnit))]   // TryMove 写 Position（ET0002）
    public static partial class LSCollisionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSCollisionComponent self)
        {
        }

        /// <summary>
        /// 位置是否被阻挡。网格外一律阻挡（地图边界=墙）。
        /// 只看 (x, z)：x→列、z→行（DNF y 纵深 ↔ 我们 z，1 单位=100px；行 0 = 世界 z=0 自上而下）。
        /// </summary>
        public static bool IsBlocked(this LSCollisionComponent self, TSVector position)
        {
            if (self.PassGrid == null || self.GridWidth <= 0 || self.GridHeight <= 0) return false;
            int col = (int)TSMath.Floor(position.x / self.CellSize);
            int row = (int)TSMath.Floor(position.z / self.CellSize);
            if (col < 0 || col >= self.GridWidth || row < 0 || row >= self.GridHeight) return true;
            return self.PassGrid[row * self.GridWidth + col] == 0;
        }

        /// <summary>
        /// 移动滑动辅助：先试整 delta，被挡则 X/Y/Z 各轴分别尝试（被挡的轴回退）——贴墙滑行。
        /// Y（高度）不进网格恒通过（跳跃/浮空不受地面网格管），保留逐轴结构以后立体碰撞直接用。
        /// </summary>
        public static void TryMove(this LSCollisionComponent self, LSUnit unit, TSVector delta)
        {
            TSVector oldPos = unit.Position;
            if (!self.IsBlocked(oldPos + delta))
            {
                unit.Position = oldPos + delta;
                return;
            }

            // 逐轴滑动（X 优先——横向战斗主轴；对角抵墙时保留可走分量）
            TSVector moved = oldPos;
            TSVector stepX = new(delta.x, FP.Zero, FP.Zero);
            if (!self.IsBlocked(moved + stepX)) moved += stepX;
            TSVector stepY = new(FP.Zero, delta.y, FP.Zero);
            if (!self.IsBlocked(moved + stepY)) moved += stepY;
            TSVector stepZ = new(FP.Zero, FP.Zero, delta.z);
            if (!self.IsBlocked(moved + stepZ)) moved += stepZ;
            unit.Position = moved;
        }
    }
}
