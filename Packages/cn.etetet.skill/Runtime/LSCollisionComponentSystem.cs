using TrueSync;

namespace ET
{
    /// <summary>
    /// 网格碰撞系统：IsBlocked 查矩阵（世界坐标→格子）；TryMove 按轴滑动移动（被挡轴回退）。
    /// 消费方：LSInputComponentSystem（玩家移动）/ LSMonsterAIComponentSystem（追击）/ LSBulletSystem（弹撞墙）。
    /// 放 ET.Skill（同 LSBulletSystem）：弹的撞墙检查在本程序集，而 ET.Skill 不得引用 ET.Hotfix（循环依赖）。
    /// </summary>
    [EntitySystemOf(typeof(LSCollisionComponent))]
    [LSEntitySystemOf(typeof(LSCollisionComponent))]
    [FriendOf(typeof(LSCollisionComponent))]
    [FriendOf(typeof(LSUnit))]   // TryMove/兜底 写 Position（ET0002）
    public static partial class LSCollisionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSCollisionComponent self)
        {
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSCollisionComponent self)
        {
            // 出界兜底（方案2）：位移漏网导致单位落在阻挡格/网格外 → 螺旋搜索最近可走格拉回。
            // 方案1（位移走碰撞）生效后本兜底正常静默；留着防未来新位移类型漏网 + 救旧存档卡死单位。
            if (self.PassGrid == null) return;
            LSUnitComponent unitComponent = self.GetParent<LSWorld>()?.GetComponent<LSUnitComponent>();
            if (unitComponent == null) return;
            foreach (var kv in unitComponent.Children)
            {
                LSUnit unit = (LSUnit)kv.Value;
                if (!self.IsBlocked(unit.Position)) continue;
                self.RelocateToNearestWalkable(unit);
            }
        }

        /// <summary>螺旋扩圈找最近可走格，把单位传送到格心（保留高度 y；扫描顺序固定=两端确定）</summary>
        private static void RelocateToNearestWalkable(this LSCollisionComponent self, LSUnit unit)
        {
            TSVector pos = unit.Position;
            int col = CellClamp((int)TSMath.Floor((pos.x - self.OriginX) / self.CellSize), self.GridWidth);
            int row = CellClamp((int)TSMath.Floor((self.OriginZ - pos.z) / self.CellSizeZ), self.GridHeight);
            if (self.PassGrid[row * self.GridWidth + col] == 1)
            {
                // 夹取后的格子可走=只是出了网格外沿 → 直接进该格心
                unit.Position = CellCenter(self, col, row, pos.y);
                Log.Warning($"[Collision] 单位{unit.Id} 出界拉回：格({col},{row})");
                return;
            }

            int maxR = System.Math.Max(self.GridWidth, self.GridHeight);
            for (int r = 1; r < maxR; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (System.Math.Abs(dx) != r && System.Math.Abs(dy) != r) continue;   // 只扫第 r 圈
                    int c = col + dx, w = row + dy;
                    if (c < 0 || c >= self.GridWidth || w < 0 || w >= self.GridHeight) continue;
                    if (self.PassGrid[w * self.GridWidth + c] != 1) continue;
                    unit.Position = CellCenter(self, c, w, pos.y);
                    Log.Warning($"[Collision] 单位{unit.Id} 阻挡格拉回：({col},{row})→({c},{w})");
                    return;
                }
            }
        }

        private static int CellClamp(int v, int max)
        {
            if (v < 0) return 0;
            if (v >= max) return max - 1;
            return v;
        }

        private static TSVector CellCenter(LSCollisionComponent self, int col, int row, FP y)
        {
            return new TSVector(
                self.OriginX + ((FP)col + (FP)1 / 2) * self.CellSize,
                y,
                self.OriginZ - ((FP)row + (FP)1 / 2) * self.CellSizeZ);
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
        /// Z 轴格尺寸补偿比例（CellSizeZ/CellSize，训练场 ≈1.167）。移动端用：纵向分量乘它实现
        /// 地面平面"格子等速"（DNF 同款——纵深感烘在美术非正方形格里，屏幕上 W/S 比 A/D 快 ~17%）。
        /// 无网格/尺寸非法返回 1（空地图退化为屏幕等速）。
        /// </summary>
        public static FP ZCellRatio(this LSCollisionComponent self)
        {
            if (self.CellSize <= FP.Zero || self.CellSizeZ <= FP.Zero) return FP.One;
            return self.CellSizeZ / self.CellSize;
        }

        /// <summary>
        /// 子步进位移（方案1，位移技能/击退用，DNF 同款"撞墙截断停住"）：
        /// 把水平 delta 切成 ≤半格 的子步逐格走碰撞——步长大于格宽会隧穿薄墙（如冲刺一步 0.3 > 格 0.16）；
        /// 第一步被挡即截断（不贴墙滑行——DNF 冲刺/击退是停在墙边）。y 分量不参与网格直加（网格忽略高度）。
        /// 返回是否走完全程（false=中途撞墙——击退方据此清水平动量）。
        /// </summary>
        public static bool MoveByStep(this LSCollisionComponent self, LSUnit unit, TSVector delta)
        {
            if (self.CellSize <= FP.Zero || self.CellSizeZ <= FP.Zero)
            {
                unit.Position += delta;   // 尺寸非法=无碰撞语义（同 IsBlocked 守卫）
                return true;
            }

            TSVector pos = unit.Position;
            FP lenSqr = delta.x * delta.x + delta.z * delta.z;
            if (lenSqr <= FP.Zero)
            {
                unit.Position = pos + new TSVector(FP.Zero, delta.y, FP.Zero);
                return true;
            }

            FP maxStep = (self.CellSize < self.CellSizeZ ? self.CellSize : self.CellSizeZ) / 2;
            int steps = (int)TSMath.Ceiling(TSMath.Sqrt(lenSqr) / maxStep);
            TSVector sub = new(delta.x / steps, FP.Zero, delta.z / steps);
            TSVector moved = pos;
            for (int i = 0; i < steps; i++)
            {
                if (self.IsBlocked(moved + sub)) return false;   // 撞墙截断
                moved += sub;
            }
            unit.Position = moved + new TSVector(FP.Zero, delta.y, FP.Zero);
            return true;
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
