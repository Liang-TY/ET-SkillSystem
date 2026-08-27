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

            LSInput input = self.LSInput;
            TSVector2 v2 = input.V * LSConstValue.PlayerMoveSpeed * 50 / 1000;
            FP vy = input.VY * LSConstValue.PlayerMoveSpeed * 50 / 1000;
            bool hasMovement = v2.LengthSquared() > FP.Zero || vy != FP.Zero;

            // 走/停动画切换（逻辑层驱动，视图层 LSSpriteAnimViewComponent 读 AnimId 换 sprite）。
            // 怪物 AI 同款防重启（!= 才 Play）；受击/攻击/ForbidMove 已在上方 return，这里再加技中锁不抢施法动画
            if (anim != null && unit.GetComponent<LSCastComponent>()?.GetActiveCast() == null)
            {
                int wantAnim = hasMovement ? AnimId.SwordmanWalk : AnimId.SwordmanIdle;
                if (anim.AnimId != wantAnim) anim.Play(wantAnim);
            }

            if (!hasMovement) return;

            // 网格碰撞：被挡轴回退（贴墙滑动）；空地图无 LSCollisionComponent 直落
            LSCollisionComponent collision = unit.LSWorld().GetComponent<LSCollisionComponent>();
            // DNF 手感：地面平面格子等速——纵向格子更高（18.67px vs 16px），z 分量乘格尺寸比例，
            // 两轴与斜向全部同格/秒（屏幕上 W/S 比 A/D 快 ~17%，纵深感烘在美术的非正方形格里；03 文档 §9）
            TSVector delta = new(v2.x, vy, v2.y * (collision?.ZCellRatio() ?? FP.One));
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