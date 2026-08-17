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
        }

        // 攻击盒开关（工厂/技能系统调用；字段访问收敛在 Friend 内）
        public static void SetAttackEnabled(this LSHitboxComponent self, bool enabled)
        {
            self.AttackEnabled = enabled;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSHitboxComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();

            // 1) 受击盒：当前动画帧 damageBox（DNF 像素）→ 世界 AABB
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            if (anim != null)
            {
                AnimFrameData frame = anim.GetCurrentFrame();
                self.CurrentHurtBox = SampleHurtBox(unit, frame.damageBox);
            }

            // 2) 攻击盒：阶段2临时——面前固定盒，随位置/朝向每帧更新（阶段3+ 换成攻击动作/技能驱动）
            if (self.AttackEnabled)
            {
                FP facing = unit.Forward.x >= FP.Zero ? FP.One : -FP.One;
                TSVector center = unit.Position + new TSVector(facing * 2, 0, 0);
                AABBUtil.UpdateCenter(ref self.CurrentAttackBox, center, new TSVector(1, 1, 1));
            }

            // 3) 阶段2验证 Log：每秒 1 次（20 帧逻辑帧）。阶段3 换成真正的命中处理时删除。
            int frameNo = unit.LSWorld().Frame;
            if (frameNo % LSConstValue.FrameCountPerSecond != 0) return;

            AABB hurt = self.CurrentHurtBox;
            Log.Info($"[Hitbox] 帧{frameNo} unit{unit.Id} anim{anim?.AnimId}-{anim?.FrameIndex} " +
                     $"受击盒 Min=({hurt.Min.x},{hurt.Min.y},{hurt.Min.z}) Max=({hurt.Max.x},{hurt.Max.y},{hurt.Max.z})");

            if (!self.AttackEnabled) return;
            CheckAttackLog(self, unit, frameNo);
        }

        // 攻击盒 vs 世界内所有其他单位的受击盒（阶段3 的命中检测骨架，当前只 Log）
        private static void CheckAttackLog(LSHitboxComponent self, LSUnit unit, int frameNo)
        {
            LSUnitComponent unitComponent = unit.LSWorld().GetComponent<LSUnitComponent>();
            foreach (var kv in unitComponent.Children)
            {
                LSUnit other = kv.Value as LSUnit;
                if (other == null || other.Id == unit.Id) continue;
                LSHitboxComponent otherHitbox = other.GetComponent<LSHitboxComponent>();
                if (otherHitbox == null) continue;

                bool hit = AABBUtil.Intersects(self.CurrentAttackBox, otherHitbox.CurrentHurtBox);
                AABB atk = self.CurrentAttackBox;
                Log.Info($"[Hitbox] 帧{frameNo} unit{unit.Id}攻击盒[{atk.Min.x}~{atk.Max.x}] vs " +
                         $"unit{other.Id}受击盒：{(hit ? "命中" : "未命中")}");
            }
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
                // 像素 → 单位：先转 FP 再除（直接 int/int 会截断丢 6% 精度）
                Min = new TSVector(pos.x + (FP)wx0 / 100, pos.y + (FP)minZ / 100, pos.z + (FP)minY / 100),
                Max = new TSVector(pos.x + (FP)wx1 / 100, pos.y + (FP)maxZ / 100, pos.z + (FP)maxY / 100),
            };
        }
    }
}
