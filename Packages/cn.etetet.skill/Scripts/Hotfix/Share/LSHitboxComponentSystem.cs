using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSHitboxComponent))]
    [LSEntitySystemOf(typeof(LSHitboxComponent))]
    [FriendOf(typeof(LSHitboxComponent))]
    [FriendOf(typeof(LSCombatComponent))]
    [FriendOf(typeof(LSInputBufferComponent))]
    public static partial class LSHitboxComponentSystem
    {
        // kneekick（5 帧）取消窗口从帧 3 起（收招）；阶段4 从 attack.json 的 cancelFrame 读
        private const int Attack1CancelFrame = 3;

        // 输入缓冲窗口 ms（阶段3.5 临时常量，DNF 连段窗口=[cancelFrame,动画末]）
        public const int BufferWindowMs = 300;

        [EntitySystem]
        private static void Awake(this LSHitboxComponent self)
        {
            self.HitTargets ??= new();
            self.CurrentHurtBoxes ??= new();
            self.CurrentAttackBoxes ??= new();
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSHitboxComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();
            LSInputBufferComponent buf = unit.GetComponent<LSInputBufferComponent>();
            bool attacking = anim != null && anim.AnimId == AnimId.Attack1;

            // 1) 攻击动作状态机（阶段3.5：动画帧驱动；阶段4 由 Cast/SkillLogic 接管）
            //    起手：非攻击中、无硬直、缓冲有攻击（写入在 LSInputComponentSystem，按下沿检测不连发）
            //    取消：攻击中且进入取消窗口（收招帧起）→ 重新起手 = 连段
            //    屏蔽：前摇/判定帧中缓冲只持有不消费——狂按不会中断当前攻击
            if (buf != null && buf.BufferedButton == 1 && anim != null)
            {
                bool canStart = !attacking && (combat == null || combat.HitstunTimer <= 0);
                bool canCancel = attacking && anim.FrameIndex >= Attack1CancelFrame;
                if (canStart || canCancel) StartAttack(self, anim, buf);
            }
            if (attacking && anim.IsFinished)
            {
                anim.Play(combat != null ? combat.DefaultAnimId : AnimId.Idle);
            }

            // 2) 盒采样（多盒）：受击盒每帧重采样；攻击盒仅攻击动作的判定帧
            AnimFrameData frame = anim != null ? anim.GetCurrentFrame() : default;
            self.CurrentHurtBoxes.Clear();
            if (frame.damageBoxes is { Length: > 0 } hurtBoxes)
            {
                foreach (AnimBox box in hurtBoxes) self.CurrentHurtBoxes.Add(SampleBox(unit, box));
            }
            else
            {
                // 旧 JSON 无 damageBoxes 数组 → 单数字段（兼容 stay/move.json）
                self.CurrentHurtBoxes.Add(SampleBox(unit, frame.damageBox));
            }

            self.CurrentAttackBoxes.Clear();
            if (attacking && frame.attackBoxes is { Length: > 0 } attackBoxes)
            {
                foreach (AnimBox box in attackBoxes) self.CurrentAttackBoxes.Add(SampleBox(unit, box));
            }
            self.AttackEnabled = self.CurrentAttackBoxes.Count > 0;

            // 3) 命中检测 + 结算（每帧；本轮攻击同一目标只结算一次）
            if (self.AttackEnabled)
            {
                CheckAttack(self, unit, unit.LSWorld().Frame);
            }
        }

        // 起手（也用于取消窗口的重新起手）：重置动画到帧 0 + 清缓冲 + 清已命中列表
        private static void StartAttack(LSHitboxComponent self, LSAnimComponent anim, LSInputBufferComponent buf)
        {
            anim.Play(AnimId.Attack1);
            buf.BufferedButton = 0;
            buf.BufferTimer = 0;
            self.HitTargets.Clear();
        }

        // 攻击盒 × 受击盒 双层循环；命中且本次攻击未命中过 → 结算
        private static void CheckAttack(LSHitboxComponent self, LSUnit unit, int frameNo)
        {
            LSUnitComponent unitComponent = unit.LSWorld().GetComponent<LSUnitComponent>();
            foreach (var kv in unitComponent.Children)
            {
                LSUnit other = kv.Value as LSUnit;
                if (other == null || other.Id == unit.Id) continue;
                LSHitboxComponent otherHitbox = other.GetComponent<LSHitboxComponent>();
                if (otherHitbox == null) continue;
                if (self.HitTargets.Contains(other.Id)) continue;   // 本次攻击已命中过

                bool hit = false;
                foreach (AABB atk in self.CurrentAttackBoxes)
                {
                    foreach (AABB hurt in otherHitbox.CurrentHurtBoxes)
                    {
                        if (!AABBUtil.Intersects(atk, hurt)) continue;
                        hit = true;
                        break;
                    }
                    if (hit) break;
                }
                if (!hit) continue;

                self.HitTargets.Add(other.Id);
                ApplyHit(unit, other, frameNo);
            }
        }

        // 命中结算。阶段3.5 临时常量（阶段4 attack.json 配置化——DNF 实证：命中反应是攻击方配置驱动
        // damageAct/upForce/backForce/hitStunTime；浮空/击退/倒地等 Z 轴反应后续阶段做）
        private static void ApplyHit(LSUnit attacker, LSUnit target, int frameNo)
        {
            const int damage = 50;      // 伤害
            const int hitstunMs = 500;  // 受击硬直

            var targetNum = target.GetComponent<LSNumericComponent>();
            if (targetNum == null) return;
            targetNum.Add(NumericType.Hp, -damage);

            LSCombatComponent targetCombat = target.GetComponent<LSCombatComponent>();
            if (targetCombat != null)
            {
                targetCombat.HitstunTimer = hitstunMs;   // 重打刷新（DNF 行为）
                target.GetComponent<LSAnimComponent>()?.Play(AnimId.Hurt);   // 受击动画，重打重置到帧 0
            }

            Log.Info($"[Combat] 帧{frameNo} unit{attacker.Id} 命中 unit{target.Id}，伤害{damage}，" +
                     $"HP={targetNum.Get(NumericType.Hp)} hitstun={hitstunMs}ms");
        }

        // DNF 坐标(像素)：x=横向(面右正) y=纵深 z=高度(0=地面脚底)。
        // 我们坐标：TSVector(x=横向, y=高度, z=纵深)，1 单位=100px，Position=脚底中心。
        // 采样公式（实证，Notes/技能系统/03-阶段2 §3）：面左时 x 区间镜像（绕脚底中心），y/z 不变。
        // 受击盒/攻击盒同一局部空间，同一公式。
        private static AABB SampleBox(LSUnit unit, AnimBox box)
        {
            // DNF 有 min/max 倒序脏数据，先归一化
            int minX = Math.Min(box.min.x, box.max.x), maxX = Math.Max(box.min.x, box.max.x);
            int minY = Math.Min(box.min.y, box.max.y), maxY = Math.Max(box.min.y, box.max.y);
            int minZ = Math.Min(box.min.z, box.max.z), maxZ = Math.Max(box.min.z, box.max.z);
            bool facingRight = unit.Forward.x >= FP.Zero;
            int wx0 = facingRight ? minX : -maxX;   // 面左：x 区间镜像
            int wx1 = facingRight ? maxX : -minX;
            TSVector pos = unit.Position;
            return new AABB
            {
                // 像素 → 单位：先转 FP 再除（直接 int/int 会截断丢精度）
                Min = new TSVector(pos.x + (FP)wx0 / 100, pos.y + (FP)minZ / 100, pos.z + (FP)minY / 100),
                Max = new TSVector(pos.x + (FP)wx1 / 100, pos.y + (FP)maxZ / 100, pos.z + (FP)maxY / 100),
            };
        }
    }
}
