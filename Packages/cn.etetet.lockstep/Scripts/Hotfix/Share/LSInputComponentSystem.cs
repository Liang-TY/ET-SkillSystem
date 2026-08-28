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

            // R（button=7）：调试回出生点（方案3——位移出界救援）。走输入帧保证两端确定性；
            // 先于其他输入门禁（硬直/攻击中也生效——救援语义）
            if (self.LSInput.Button == 7)
            {
                Room room = unit.LSWorld().GetParent<Room>();
                MapDefinition mapDef = MapLoader.Get(room?.MapId ?? 0);
                if (mapDef?.PlayerSpawn != null)
                {
                    unit.Position = mapDef.PlayerSpawn;
                    Log.Info($"[Debug] 单位{unit.Id} R 键回出生点 {mapDef.PlayerSpawn}");
                }
                return;
            }

            // 按下沿检测：Button 从 0→非0 才算一次输入（按住不连发；支持多按键值）
            bool pressed = self.LSInput.Button != 0 && self.LastButton == 0;
            self.LastButton = self.LSInput.Button;

            LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            bool inHitstun = combat != null && combat.HitstunTimer > 0;
            // 攻击中不能移动：检查所有攻击动画 ID（怪物 kneekick + 鬼剑士 attack1-3 + 冲刺）
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

            // 跳跃（C 键，button 16；DNF 跳跃非技能同构）：地面 + 非硬直 + 非在技 → 起跳初速度交给
            // LSFlightComponent 重力积分（IsJump 落地回默认动画）。空中不接二段跳。
            if (pressed && self.LSInput.Button == 16)
            {
                LSFlightComponent flight = unit.GetComponent<LSFlightComponent>();
                bool grounded = unit.Position.y <= FP.Zero && (flight == null || !flight.Active);
                if (flight != null && grounded
                    && unit.GetComponent<LSCastComponent>()?.GetActiveCast() == null)
                {
                    flight.Active = true;
                    flight.IsJump = true;
                    flight.Velocity = new TSVector(FP.Zero, 10, FP.Zero);   // 重力40 → 空中0.5s，最高1.25单位
                    anim?.Play(AnimId.JumpUp);
                    return;
                }
            }

            LSInput input = self.LSInput;
            TSVector2 v2 = input.V * LSConstValue.PlayerMoveSpeed * 50 / 1000;
            FP vy = input.VY * LSConstValue.PlayerMoveSpeed * 50 / 1000;
            bool hasMovement = v2.LengthSquared() > FP.Zero || vy != FP.Zero;

            // 走/停动画切换（逻辑层驱动，视图层 LSSpriteAnimViewComponent 读 AnimId 换 sprite）。
            // 怪物 AI 同款防重启（!= 才 Play）；受击/攻击/ForbidMove 已在上方 return，这里再加技中锁不抢施法动画；
            // 空中不切（跳跃动画由 LSFlightComponentSystem 按物理状态驱动，起跳/下落段不被覆盖）
            if (anim != null && unit.Position.y <= FP.Zero
                && unit.GetComponent<LSCastComponent>()?.GetActiveCast() == null)
            {
                int wantAnim = hasMovement ? AnimId.SwordmanWalk : AnimId.SwordmanIdle;
                if (anim.AnimId != wantAnim) anim.Play(wantAnim);
            }

            if (!hasMovement) return;

            // 网格碰撞：被挡轴回退（贴墙滑动）；空地图无 LSCollisionComponent 直落
            LSCollisionComponent collision = unit.LSWorld().GetComponent<LSCollisionComponent>();
            // 屏幕等速（2026-08-27 改）：原实现 z 分量乘 ZCellRatio()（=CellSizeZ/CellSize）做"格子等速"，
            // 使副本（非正方格 16×18.67px）W/S 比 A/D 快 ~17%，且与城镇（正方格 16×16，无补偿）W/S 手感不一致。
            // 现去掉 z 补偿，x/z 两轴同速度——屏幕坐标 1:1 映射下 W/S 与 A/D 等速，两图口径统一。
            TSVector delta = new(v2.x, vy, v2.y);
            if (collision != null)
            {
                collision.TryMove(unit, delta);
            }
            else
            {
                unit.Position += delta;
            }
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