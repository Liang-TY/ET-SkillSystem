using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSInputComponent))]
    [LSEntitySystemOf(typeof(LSInputComponent))]
    public static partial class LSInputComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSInputComponent self)
        {

        }
        
        [LSEntitySystem]
        private static void LSUpdate(this LSInputComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();

            // 眩晕/冰冻等 ForbidMove > 0 时禁止移动（skill 包的 LSNumericComponent）
            var numeric = unit.GetComponent<LSNumericComponent>();
            if (numeric != null && numeric.Get(NumericType.ForbidMove) > FP.Zero) return;

            LSInput input = self.LSInput;
            TSVector2 v2 = input.V * 6 * 50 / 1000;
            FP vy = input.VY * 6 * 50 / 1000;
            bool hasMovement = v2.LengthSquared() > FP.Zero || vy != FP.Zero;
            if (!hasMovement) return;
            TSVector oldPos = unit.Position;
            unit.Position += new TSVector(v2.x, vy, v2.y);
            // 只有 A/D 才改变朝向，W/S/C/V 不改
            if (v2.x > FP.Zero)
            {
                unit.Forward = new TSVector(1, 0, 0);
            }
            // v2.x == 0 时不改朝向
            else if (v2.x < FP.Zero)
            {
                unit.Forward = new TSVector(-1, 0, 0);
            }
        }
    }
}