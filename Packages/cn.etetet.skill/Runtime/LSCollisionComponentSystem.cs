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
        /// 坐标换算：格子 = (世界坐标 - Origin) / CellSize——Origin 在 InitCollision 时对齐瓦片贴图和可行走带。
        /// </summary>
        public static bool IsBlocked(this LSCollisionComponent self, TSVector position)
        {
            if (self.PassGrid == null || self.GridWidth <= 0 || self.GridHeight <= 0) return false;
            if (self.CellSize <= FP.Zero || self.CellSizeZ <= FP.Zero) return false;
            // X 轴：世界 x → 格子列（左右边界）
            int col = (int)TSMath.Floor((position.x - self.OriginX) / self.CellSize);
            if (col < 0 || col >= self.GridWidth) return true;

            // Z 轴（= 屏幕 Y，上下移动）：反转行映射
            // 游戏 z 就是屏幕竖直方向（W=z+往上，S=z-往下），贴图 row 0=顶部
            // → z 越大（往上）row 越小（靠近顶部墙），z 越小（往下）row 越大（靠近底部地板）
            int row = (int)TSMath.Floor((self.OriginZ - position.z) / self.CellSizeZ);
            if (row < 0 || row >= self.GridHeight) return true;

            return self.PassGrid[row * self.GridWidth + col] == 0;
        }

        /// <summary>
        /// 移动滑动辅助：先试整 delta，被挡则 X/Y/Z 各轴分别尝试（被挡的轴回退）——贴墙滑行。
        /// Y（高度）不进网格恒通过（跳跃/浮空不受地面网格管），保留逐轴结构以后立体碰撞直接用。
        /// </summary>
        public static void TryMove(this LSCollisionComponent self, LSUnit unit, TSVector delta)
        {
            TSVector oldPos = unit.Position;
            // 诊断（按一次 WASD 后看第一行）——公式与 IsBlocked 完全一致（此前漏了 z 翻转，打印过假 row=-29）
            {
                int dCol = (int)TSMath.Floor((oldPos.x - self.OriginX) / self.CellSize);
                int dRow = (int)TSMath.Floor((self.OriginZ - oldPos.z) / self.CellSizeZ);
                bool ok = dCol >= 0 && dCol < self.GridWidth && dRow >= 0 && dRow < self.GridHeight;
                byte val = ok ? self.PassGrid[dRow * self.GridWidth + dCol] : (byte)255;
                int walkable = 0; foreach (byte b in self.PassGrid) if (b == 1) walkable++;
                Log.Info($"[Collision_diag] pos=({oldPos.x:F2},{oldPos.z:F2}) → cell=({dCol},{dRow}) ok={ok} val={val} " +
                         $"walkable={walkable}/{self.PassGrid.Length} cell=({self.CellSize},{self.CellSizeZ}) origin=({self.OriginX},{self.OriginZ})");
            }
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
