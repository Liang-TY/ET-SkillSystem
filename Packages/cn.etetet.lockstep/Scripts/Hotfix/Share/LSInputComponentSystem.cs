using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSInputComponent))]
    [LSEntitySystemOf(typeof(LSInputComponent))]
    [FriendOf(typeof(LSCombatComponent))]
    [FriendOf(typeof(LSInputBufferComponent))]
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

            // 按下沿检测：Button 从 0→非0 才算一次输入（按住不连发；支持多按键值）
            bool pressed = self.LSInput.Button != 0 && self.LastButton == 0;
            self.LastButton = self.LSInput.Button;

            LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            bool inHitstun = combat != null && combat.HitstunTimer > 0;
            // 攻击中不能移动：检查所有攻击动画 ID（怪物 kneekick + 鬼剑士 attack1-3）
            bool attacking = anim != null && !anim.IsFinished
                && (anim.AnimId == AnimId.Attack1
                    || anim.AnimId == AnimId.SwordmanAttack1
                    || anim.AnimId == AnimId.SwordmanAttack2
                    || anim.AnimId == AnimId.SwordmanAttack3);

            // 攻击输入：写缓冲（能否起手由 LSSkillComponentSystem.TryCast 决定——三重门禁；取消窗口由技能 OnUpdate 消费）
            if (pressed)
            {
                LSInputBufferComponent buf = unit.GetComponent<LSInputBufferComponent>();
                if (buf != null)
                {
                    buf.BufferedButton = self.LSInput.Button;
                    buf.BufferTimer = LSInputBufferComponentSystem.BufferWindowMs;
                }
            }

            // 受击硬直中不能移动（也不能起手攻击，见 LSHitboxComponentSystem）
            if (inHitstun) return;

            // 攻击动作中不能移动（普攻站桩；移动取消以后做）
            if (attacking) return;

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