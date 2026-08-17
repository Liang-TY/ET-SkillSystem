using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSHitboxComponent))]
    [LSEntitySystemOf(typeof(LSHitboxComponent))]
    [FriendOf(typeof(LSHitboxComponent))]
    public static partial class LSHitboxComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSHitboxComponent self)
        {
            self.HitTargets ??= new();
        }

        // 攻击输入驱动（阶段3临时：按住=攻击盒持续激活，松开=关闭；
        // 每次按下算一次新攻击，清空已命中列表。阶段4+ 换成攻击动作帧事件/技能 Cast 驱动）
        public static void SetAttackInput(this LSHitboxComponent self, bool pressed)
        {
            if (pressed)
            {
                if (self.AttackEnabled) return;      // 持续按住：不是新攻击，保留已命中列表
                self.AttackEnabled = true;
                self.HitTargets.Clear();
            }
            else
            {
                if (!self.AttackEnabled) return;
                self.AttackEnabled = false;
                self.HitTargets.Clear();
            }
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSHitboxComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
            int frameNo = unit.LSWorld().Frame;

            // 1) 受击盒：当前动画帧 damageBox（DNF 像素）→ 世界 AABB
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            if (anim != null)
            {
                AnimFrameData frame = anim.GetCurrentFrame();
                self.CurrentHurtBox = SampleHurtBox(unit, frame.damageBox);
            }

            // 2) 攻击盒：随位置/朝向更新 + 命中检测扣血（每帧；激活/关闭由 LSInputComponentSystem 按攻击键驱动）
            if (self.AttackEnabled)
            {
                FP facing = unit.Forward.x >= FP.Zero ? FP.One : -FP.One;
                TSVector center = unit.Position + new TSVector(facing * 2, 0, 0);
                AABBUtil.UpdateCenter(ref self.CurrentAttackBox, center, new TSVector(1, 1, 1));
                CheckAttack(self, unit, frameNo);
            }
        }

        // 攻击盒 vs 世界内所有其他单位的受击盒；相交且本次攻击未命中过 → 扣血（防多重命中）
        private static void CheckAttack(LSHitboxComponent self, LSUnit unit, int frameNo)
        {
            LSUnitComponent unitComponent = unit.LSWorld().GetComponent<LSUnitComponent>();
            foreach (var kv in unitComponent.Children)
            {
                LSUnit other = kv.Value as LSUnit;
                if (other == null || other.Id == unit.Id) continue;
                LSHitboxComponent otherHitbox = other.GetComponent<LSHitboxComponent>();
                if (otherHitbox == null) continue;

                if (AABBUtil.Intersects(self.CurrentAttackBox, otherHitbox.CurrentHurtBox)
                    && !self.HitTargets.Contains(other.Id))
                {
                    self.HitTargets.Add(other.Id);
                    ApplyDamage(unit, other, frameNo);
                }
            }
        }

        // 阶段3临时：固定伤害 50（注意 FP.FromRaw(50) 是设内部原始值≈0，文档笔误，别用）
        // 阶段4+ 换成攻击方 Attack 数值 / 技能数据
        private static void ApplyDamage(LSUnit attacker, LSUnit target, int frameNo)
        {
            var targetNum = target.GetComponent<LSNumericComponent>();
            if (targetNum == null) return;
            FP damage = 50;
            targetNum.Add(NumericType.Hp, -damage);
            Log.Info($"[Combat] 帧{frameNo} unit{attacker.Id} 命中 unit{target.Id}，伤害{damage}，HP={targetNum.Get(NumericType.Hp)}");
        }

        // DNF 坐标(像素)：x=横向(面右正) y=纵深 z=高度(0=地面脚底)。
        // 我们坐标：TSVector(x=横向, y=高度, z=纵深)，1 单位=100px，Position=脚底中心。
        // 采样公式（实证，Notes/技能系统/03-阶段2 §3）：面左时 x 区间镜像（绕脚底中心），y/z 不变。
        private static AABB SampleHurtBox(LSUnit unit, AnimBox box)
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
