using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSHitboxComponent))]
    [LSEntitySystemOf(typeof(LSHitboxComponent))]
    [FriendOf(typeof(LSHitboxComponent))]
    [FriendOf(typeof(LSCast))]
    public static partial class LSHitboxComponentSystem
    {
        /// <summary>注册动画 json 任一帧自带 attackBoxes → 帧驱动自动激活采样（判定帧=有盒帧）。
        /// 翻译产物有盒即设计意图，不再白名单逐技能接线（漏加=静默无判定的坑）；无盒动画不受影响
        /// （手动盒路径 SetAttackHitbox 照常）。冰息本体无盒（判定在弹上）。
        /// 不做静态缓存（ET0004：Hotfix 层禁静态字段）——每帧遍历 ≤16 帧零分配，开销可忽略。</summary>
        private static bool IsAttackDrivenAnim(int animId)
        {
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip?.frames == null) return false;
            foreach (AnimFrameData frame in clip.frames)
            {
                if (frame.attackBoxes is { Length: > 0 }) return true;
            }
            return false;
        }

        [EntitySystem]
        private static void Awake(this LSHitboxComponent self)
        {
            self.HitTargets ??= new();
            self.CurrentHurtBoxes ??= new();
            self.CurrentAttackBoxes ??= new();
        }

        /// <summary>新一轮攻击前清空已命中表（SkillContext.ClearHitTargets / NormalAttack.OnCast 调）</summary>
        public static void ClearHitTargets(this LSHitboxComponent self) => self.HitTargets.Clear();

        [LSEntitySystem]
        private static void LSUpdate(this LSHitboxComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();

            // 1) 受击盒：每帧从当前动画帧 damageBoxes 重采样（多盒；旧 JSON 回退单数字段）
            AnimFrameData frame = anim != null ? anim.GetCurrentFrame() : default;
            self.CurrentHurtBoxes.Clear();
            if (frame.damageBoxes is { Length: > 0 } hurtBoxes)
            {
                foreach (AnimBox box in hurtBoxes) self.CurrentHurtBoxes.Add(SampleBox(unit, box));
            }
            else
            {
                self.CurrentHurtBoxes.Add(SampleBox(unit, frame.damageBox));
            }

            // 2) 攻击盒：攻击动作的判定帧（有 attackBoxes 的帧）驱动，无盒帧 = 前摇/收招无判定。
            //    判定帧驱动的动画 = json 里带 attackBoxes 的（kneekick / 波动爆发冲刺）；
            //    其他动画不动列表——固定盒技能走 SkillContext.SetAttackHitbox 手动管理；
            //    攻击动作状态机（起手/取消/结束）在 Cast 框架（LSSkillComponentSystem / NormalAttack）。
            if (anim != null && IsAttackDrivenAnim(anim.AnimId))
            {
                self.CurrentAttackBoxes.Clear();
                if (frame.attackBoxes is { Length: > 0 } attackBoxes)
                {
                    foreach (AnimBox box in attackBoxes) self.CurrentAttackBoxes.Add(SampleBox(unit, box));
                }
            }
            self.AttackEnabled = self.CurrentAttackBoxes.Count > 0;   // 派生态（观察用）

            // 3) 命中检测 + 结算（本次攻击同一目标只结算一次）
            if (self.AttackEnabled)
            {
                CheckAttack(self, unit, unit.LSWorld().Frame);
            }
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
                ApplyHit(self, unit, other, frameNo);
            }
        }

        // 命中结算（阶段5 Actions 化）：伤害/硬直/受击动画等效果全部在技能配置的 HitActions 节点里
        // （SkillContent 的 Action 类，如 MeleeHitAction）；本方法只做防重记录 + cast 回写 + 分发。
        private static void ApplyHit(LSHitboxComponent self, LSUnit attacker, LSUnit target, int frameNo)
        {
            // 回写施放实例（Route B：JustHit + TargetIds，阶段7 视图层用）
            LSCast activeCast = attacker.GetComponent<LSCastComponent>()?.GetActiveCast();
            activeCast?.NotifyHit(target.Id);

            // 分发技能命中效果节点（owner=受击者，source=攻击方）
            SkillLogic logic = activeCast != null ? SkillLoader.Get(activeCast.SkillId) : null;
            int[] hitActions = logic?.HitActions;
            if (hitActions == null)
            {
                Log.Warning($"[Combat] 帧{frameNo} unit{attacker.Id} 命中 unit{target.Id}，但技能未配 HitActions，无效果");
                return;
            }
            foreach (int actionId in hitActions)
            {
                LSAction action = ActionLoader.Get(actionId);
                if (action == null)
                {
                    Log.Error($"[Combat] 技能{activeCast.SkillId} 引用了未注册的 actionId={actionId}，跳过");
                    continue;
                }
                action.Run(new LSActionContext(attacker.LSWorld(), target, attacker, frameNo, logic?.HitReaction));
            }
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
