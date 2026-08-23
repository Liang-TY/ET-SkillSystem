using TrueSync;

namespace ET
{
    /// <summary>
    /// 怪物 AI 系统（行为机的锁步翻译，02 文档 §10.4）：每帧 Tick 当前行为节点；
    /// ThinkTimer 到点重估条件切节点（=行为机调度器）。数值全部读 MonsterAiDefinition（零常量）。
    /// 行为节点：0=Idle / 1=ChaseAttack（追击+攻击一个节点——行为机原则不拆细，移动是行为的一部分）。
    /// </summary>
    [EntitySystemOf(typeof(LSMonsterAIComponent))]
    [LSEntitySystemOf(typeof(LSMonsterAIComponent))]
    [FriendOf(typeof(LSMonsterAIComponent))]
    [FriendOf(typeof(LSCombatComponent))]    // 读 HitstunTimer（硬直静默，ET0002）
    [FriendOf(typeof(LSAnimComponent))]      // 读 AnimId/Play（防 Walk 重启，ET0002）
    [FriendOf(typeof(LSCastComponent))]      // 读活动 cast（技中移动锁，ET0002）
    public static partial class LSMonsterAIComponentSystem
    {
        // 行为节点枚举（行为机互斥节点）
        private const int NodeIdle = 0;
        private const int NodeChaseAttack = 1;

        [EntitySystem]
        private static void Awake(this LSMonsterAIComponent self, int monsterAiId)
        {
            self.MonsterAiId = monsterAiId;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSMonsterAIComponent self)
        {
            MonsterAiDefinition def = MonsterAiLoader.Get(self.MonsterAiId);
            if (def == null) return;

            LSUnit unit = self.GetParent<LSUnit>();

            // 死亡流程：HP≤0 → 播倒地动画（MonsterDown）→ 动画时长倒计时 → 到点移除
            // （BattleWatcher 按 AI 组件消失判活——胜利 3 秒自动等死亡动画播完才起算）
            LSNumericComponent monsterNum = unit.GetComponent<LSNumericComponent>();
            if (self.DyingTimerMs > 0)
            {
                self.DyingTimerMs -= LSConstValue.UpdateInterval;
                if (self.DyingTimerMs <= 0)
                {
                    Log.Info($"[Monster] unit{unit.Id} 死亡移除");
                    unit.Dispose();
                }
                return;
            }
            if (monsterNum != null && monsterNum.Get(NumericType.Hp) <= FP.Zero)
            {
                LSCombatComponent dyingCombat = unit.GetComponent<LSCombatComponent>();
                int downMs = 600;
                if (dyingCombat != null && dyingCombat.DownAnimId != 0)
                {
                    unit.GetComponent<LSAnimComponent>()?.Play(dyingCombat.DownAnimId);
                    AnimClipData downClip = AnimConfigRegistry.Get(dyingCombat.DownAnimId);
                    downMs = downClip?.totalDuration ?? 600;
                }
                self.DyingTimerMs = downMs;
                Log.Info($"[Monster] unit{unit.Id} HP 耗尽，播倒地动画 {downMs}ms");
                return;
            }

            LSCombatComponent combat = unit.GetComponent<LSCombatComponent>();

            // 0) 硬直/倒地中 → 受击系统接管（=行为机"打断"），AI 完全静默
            if (combat != null && combat.HitstunTimer > 0) return;

            // 1) 行为重估（Think 节流）
            self.ThinkTimerMs -= LSConstValue.UpdateInterval;
            if (self.ThinkTimerMs <= 0)
            {
                self.ThinkTimerMs = def.ThinkIntervalMs;
                Think(self, unit, def);
            }

            self.AttackTimerMs -= LSConstValue.UpdateInterval;
            if (self.AttackTimerMs < 0) self.AttackTimerMs = 0;

            // 2) 当前节点 Tick
            if (self.CurrentNode == NodeChaseAttack)
                TickChaseAttack(self, unit, def);
            else
                TickIdle(self, unit, def);
        }

        /// <summary>行为重估：只看当前条件（无网状转移）。有目标→ChaseAttack；无→Idle。</summary>
        private static void Think(this LSMonsterAIComponent self, LSUnit unit, MonsterAiDefinition def)
        {
            LSUnit target = FindTarget(unit, def.SightRange);
            long newTarget = target?.Id ?? 0;
            int newNode = newTarget != 0 ? NodeChaseAttack : NodeIdle;

            if (newNode != self.CurrentNode || newTarget != self.TargetId)
            {
                Log.Info($"[MonsterAI] unit{unit.Id} 切行为 {self.CurrentNode}→{newNode} target={newTarget}");
            }
            if (newNode != self.CurrentNode)
            {
                self.CurrentNode = newNode;
                self.TargetId = newTarget;
                self.AttackTimerMs = 0;
            }
            else
            {
                self.TargetId = newTarget;   // 同节点也刷新目标（最近原则）
            }
        }

        /// <summary>索敌：视野内最近的玩家单位（带 LSInputComponent 的=玩家，02 文档 §10.1）</summary>
        private static LSUnit FindTarget(LSUnit unit, FP sightRange)
        {
            LSUnitComponent unitComponent = unit.LSWorld().GetComponent<LSUnitComponent>();
            LSUnit nearest = null;
            FP nearestDist = sightRange;
            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is not LSUnit other || other.Id == unit.Id) continue;
                if (other.GetComponent<LSInputComponent>() == null) continue;   // 只打玩家
                FP dist = TSMath.Abs(other.Position.x - unit.Position.x);
                if (dist >= nearestDist) continue;
                nearestDist = dist;
                nearest = other;
            }
            return nearest;
        }

        /// <summary>Idle 行为：播待机动画（防重启：!= 才 Play；技中/硬直不抢——由技能/受击系统管）</summary>
        private static void TickIdle(this LSMonsterAIComponent self, LSUnit unit, MonsterAiDefinition def)
        {
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            if (anim == null) return;
            if (unit.GetComponent<LSCastComponent>()?.GetActiveCast() != null) return;
            if (anim.AnimId != def.IdleAnimId) anim.Play(def.IdleAnimId);
        }

        /// <summary>ChaseAttack 行为（追击+攻击一体）：移动是行为的一部分，不拆节点</summary>
        private static void TickChaseAttack(this LSMonsterAIComponent self, LSUnit unit, MonsterAiDefinition def)
        {
            // 目标失效（死了/被回收）→ 本帧静默，等 Think 重估
            LSUnit target = unit.LSWorld().GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.TargetId);
            if (target == null) return;

            // 技中移动锁（玩家侧同款 attacking 检查）：攻击动画播放中不移动
            if (unit.GetComponent<LSCastComponent>()?.GetActiveCast() != null) return;

            FP dist = TSMath.Abs(target.Position.x - unit.Position.x);
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            FP dt = (FP)LSConstValue.UpdateInterval / 1000;

            // 远程先手（窗口内且配了远程技）：CD/技中由 TryCast 门禁管，失败静默
            if (def.RangedSkillId != 0 && dist > def.RangedMinRange && dist <= def.RangedMaxRange)
            {
                if (SkillCastHelper.TryCast(unit, def.RangedSkillId)) return;   // 施放了就等下帧
            }

            if (dist > def.MeleeRange)
            {
                // 追击：朝目标移动（抄玩家移动公式）+ 播 Walk（防重启）。
                // 碰撞同玩家：被挡轴回退（贴墙滑动）——卡墙就卡着（demo 直线追击，以后 A*，03 文档 §8）
                FP dir = target.Position.x >= unit.Position.x ? FP.One : -FP.One;
                unit.Forward = new TSVector(dir, FP.Zero, FP.Zero);
                TSVector delta = new(dir * def.MoveSpeed * dt, FP.Zero, FP.Zero);
                LSCollisionComponent collision = unit.LSWorld().GetComponent<LSCollisionComponent>();
                if (collision != null)
                {
                    collision.TryMove(unit, delta);
                }
                else
                {
                    unit.Position += delta;
                }
                if (anim != null && anim.AnimId != def.MoveAnimId) anim.Play(def.MoveAnimId);
                return;
            }

            // 近战：攻击间隔到点 → 加权选招（LSRng 确定性）
            if (self.AttackTimerMs > 0) return;
            self.AttackTimerMs = def.AttackIntervalMs;
            int skillId = PickWeightedMelee(self, unit, def);
            if (skillId != 0)
            {
                FP dir = target.Position.x >= unit.Position.x ? FP.One : -FP.One;
                unit.Forward = new TSVector(dir, FP.Zero, FP.Zero);   // 出手前朝向目标
                SkillCastHelper.TryCast(unit, skillId);
            }
        }

        /// <summary>LSRng 加权选近战招（roll [0,100) 按权重累计区间命中）</summary>
        private static int PickWeightedMelee(this LSMonsterAIComponent self, LSUnit unit, MonsterAiDefinition def)
        {
            int[] skills = def.MeleeSkillIds;
            int[] weights = def.MeleeWeights;
            if (skills == null || skills.Length == 0) return 0;

            int roll = LSRng.Roll(unit.LSWorld().Frame, unit.Id, LSRng.PurposeAiSelect);
            if (weights == null || weights.Length != skills.Length)
            {
                return skills[roll % skills.Length];   // 无权重=均分
            }

            int total = 0;
            foreach (int w in weights) total += w;
            int cursor = roll * total / 100;
            for (int i = 0; i < skills.Length; i++)
            {
                cursor -= weights[i];
                if (cursor < 0) return skills[i];
            }
            return skills[skills.Length - 1];
        }
    }
}
