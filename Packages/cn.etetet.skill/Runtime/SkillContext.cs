using System.Collections.Generic;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 技能操作上下文（readonly struct：值传递零 GC，重入天然安全——每次调用独立副本）。
    /// 技能代码只见本门面方法 + 基础类型（TSVector/AnimId/...），不直接摸 LSCast/LSUnit 实体
    /// ——这也是 ET.SkillContent csproj 引用面收敛到最小（ET.Skill/Core/TrueSync/Npkparser）的基础。
    /// 注意：ET.Skill 不在 ET 分析器管辖名单（只查 ET.Core/Model/Hotfix/ModelView/HotfixView），
    /// [FriendOf] 不标 struct 也无机器检查——这层的字段访问纪律靠 skill/CLAUDE.md + 守门员。
    /// Bullet/Area/Buff API 阶段5/6 加。
    /// </summary>
    public readonly struct SkillContext
    {
        private readonly LSWorld world;
        private readonly LSUnit caster;
        private readonly LSCast cast;

        public SkillContext(LSWorld world, LSUnit caster, LSCast cast)
        {
            this.world = world;
            this.caster = caster;
            this.cast = cast;
        }

        // ---- 状态读取（门面：技能不直接摸 LSCast/LSCast 字段）----
        public int GetElapsedMs() => cast.ElapsedMs;

        public long GetCasterId() => caster.Id;

        /// <summary>施法者本体（接触检测用：CheckHit(自己, 敌人)——破军冲撞撞敌停驻等）</summary>
        public LSUnit GetCaster() => caster;

        /// <summary>施法者是否在空中（y > 0；跳跃/被击飞浮空段；贴地滑行不算——y=0）</summary>
        public bool IsCasterAirborne() => caster.Position.y > FP.Zero;

        public TSVector GetTargetPosition() => cast.TargetPosition;

        /// <summary>技能子状态（LSCast.SubState 门面：DNF setSkillSubState 同构——帧内一次性触发的
        /// "已引爆/已进入下一阶段"标记用；进快照回滚安全，施放开始时为 0）</summary>
        public int GetSubState() => cast.SubState;

        public void SetSubState(int value) => cast.SubState = value;

        /// <summary>技能相位（LSCast.Phase 门面）：连段技能存"当前段开始时的累计 ms"，
        /// 段内时间 = GetElapsedMs() - GetPhase()；与 SubState 配对进快照回滚安全</summary>
        public int GetPhase() => cast.Phase;

        public void SetPhase(int value) => cast.Phase = value;

        // ---- 施法者数值（自耗 HP 类技能用；DNF onSetState 扣血同构）----
        public FP GetCasterHp()
            => caster.GetComponent<LSNumericComponent>()?.Get(NumericType.Hp) ?? FP.Zero;

        public FP GetCasterMaxHp()
            => caster.GetComponent<LSNumericComponent>()?.Get(NumericType.MaxHp) ?? FP.Zero;

        /// <summary>扣除施法者自身 HP（直减不经公式重算；量自己算，如 maxHp × 5%）</summary>
        public void ConsumeCasterHp(FP amount)
            => caster.GetComponent<LSNumericComponent>()?.Add(NumericType.Hp, -amount);

        /// <summary>施法者进入霸体（DNF DAMAGE TYPE SUPERARMOR 同构：持续 ms 内被击只扣血，
        /// 不吃硬直/击退/浮空/受击动画；技能在起霸时点一次性调用，重复调用取覆盖值）</summary>
        public void SetCasterSuperArmor(int durationMs)
        {
            LSCombatComponent combat = caster.GetComponent<LSCombatComponent>();
            if (combat != null && durationMs > combat.SuperArmorTimer) combat.SuperArmorTimer = durationMs;
        }

        // ---- 动画 ----
        /// <summary>播放动画（LSAnimComponentSystem.Play 是 ET.Hotfix 扩展，ET.Skill 引用不到，
        /// 统一走 LSAnimPlayUtil 属性赋值实现——与那边保持同步）</summary>
        public void PlayAnim(int animId) => LSAnimPlayUtil.Play(caster, animId);

        public void PlayDefaultAnim()
        {
            LSCombatComponent combat = caster.GetComponent<LSCombatComponent>();
            // 硬直/倒地期间动画由受击系统接管（Hurt/Down + 硬直结束切回默认）——
            // 技能收招不抢（审查修复：在技中被击飞时 OnEnd 会把落地的 Down 覆盖成默认）
            if (combat != null && combat.HitstunTimer > 0) return;
            PlayAnim(combat != null ? combat.DefaultAnimId : AnimId.Idle);
        }

        public int CurrentFrameIndex() => caster.GetComponent<LSAnimComponent>()?.FrameIndex ?? 0;

        // ---- 输入缓冲 ----
        public int PeekBufferedButton() => caster.GetComponent<LSInputBufferComponent>()?.BufferedButton ?? 0;

        public void ConsumeBuffer()
        {
            LSInputBufferComponent buf = caster.GetComponent<LSInputBufferComponent>();
            if (buf == null) return;
            buf.BufferedButton = 0;
            buf.BufferTimer = 0;
        }

        // ---- 碰撞框（固定盒路径：无帧数据的技能用；帧驱动技能如 NormalAttack 不走这）----
        public void SetAttackHitbox(TSVector offset, TSVector halfExtents)
        {
            LSHitboxComponent hitbox = caster.GetComponent<LSHitboxComponent>();
            if (hitbox == null) return;
            FP facing = caster.Forward.x >= FP.Zero ? FP.One : -FP.One;
            TSVector center = caster.Position + new TSVector(facing * offset.x, offset.y, offset.z);
            hitbox.CurrentAttackBoxes.Clear();
            hitbox.CurrentAttackBoxes.Add(AABBUtil.FromCenter(center, halfExtents));
        }

        public void DisableAttackHitbox() => caster.GetComponent<LSHitboxComponent>()?.CurrentAttackBoxes.Clear();

        public void ClearHitTargets() => caster.GetComponent<LSHitboxComponent>()?.HitTargets.Clear();

        // ---- 数值 ----
        public void AddNumeric(LSUnit target, int numericKey, FP value)
            => target.GetComponent<LSNumericComponent>()?.Add(numericKey, value);

        public FP GetNumeric(LSUnit target, int numericKey)
            => target.GetComponent<LSNumericComponent>()?.Get(numericKey) ?? FP.Zero;

        // ---- 查询 ----
        [StaticField]
        private static readonly List<LSUnit> enemyBuf = new();

        /// <summary>场上除自己外所有单位（返回共享缓冲，勿持有；阵营系统之前先这样）</summary>
        public List<LSUnit> GetEnemies()
        {
            enemyBuf.Clear();
            LSUnitComponent unitComponent = world.GetComponent<LSUnitComponent>();
            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is LSUnit unit && unit.Id != caster.Id) enemyBuf.Add(unit);
            }
            return enemyBuf;
        }

        public bool CheckHit(LSUnit attacker, LSUnit target)
        {
            LSHitboxComponent atk = attacker.GetComponent<LSHitboxComponent>();
            LSHitboxComponent hurt = target.GetComponent<LSHitboxComponent>();
            if (atk == null || hurt == null) return false;
            foreach (AABB a in atk.CurrentAttackBoxes)
            {
                foreach (AABB h in hurt.CurrentHurtBoxes)
                {
                    if (AABBUtil.Intersects(a, h)) return true;
                }
            }
            return false;
        }

        // ---- Buff ----
        /// <summary>给目标��� Buff（source = 施法者）</summary>
        public void AddBuff(LSUnit target, int buffId)
            => target.GetComponent<LSBuffComponent>()?.AddBuff(caster, buffId);

        /// <summary>给施法者自己挂 Buff（自伤/变形/测试用）</summary>
        public void AddBuffToSelf(int buffId)
            => caster.GetComponent<LSBuffComponent>()?.AddBuff(caster, buffId);

        // ---- 投射物 ----
        /// <summary>发射投射物（出生=身前 0.8 单位，方向=施法者朝向；配置在 BulletDefinition）</summary>
        public void CreateBullet(int bulletId)
            => world.GetComponent<LSBulletComponent>()?.Create(caster, bulletId);

        // ---- 区域效果 ----
        /// <summary>在指定位置创建区域（火圈等持续型场地效果；配置在 AreaDefinition）</summary>
        public void CreateArea(int areaId, TSVector position)
            => world.GetComponent<LSAreaComponent>()?.Create(caster, position, areaId);

        /// <summary>在施法者身前创建区域</summary>
        public void CreateAreaInFront(int areaId, FP distance)
        {
            TSVector forward = caster.Forward;
            TSVector pos = caster.Position + new TSVector(forward.x >= FP.Zero ? distance : -distance, FP.Zero, FP.Zero);
            world.GetComponent<LSAreaComponent>()?.Create(caster, pos, areaId);
        }

        // ---- 连段取消：结束当前施放并立刻重施同技能（取消窗口用）----
        public void RestartCurrentSkill()
        {
            ConsumeBuffer();
            cast.EndNow(this);
            SkillCastHelper.TryCast(caster, cast.SkillId);
        }

        /// <summary>主动结束当前施放（连段收招/自控时长技能的正常出口；OnEnd 照常执行，ManualCooldown 此刻起 CD）</summary>
        public void EndCast() => cast.EndNow(this);

        // ---- 施法者位移（DNF onProc sq_setCurrentAxisPos 逐帧插值同构）----
        /// <summary>
        /// 沿面朝方向位移（面左自动镜像；每帧增量调用，累计位移由技能自己算——纯函数回滚安全）。
        /// 走碰撞子步进（方案1）：撞墙截断停住——DNF 冲刺贴墙停手感；空地图无碰撞直落。
        /// </summary>
        public void MoveCasterForward(FP distance)
        {
            FP facing = caster.Forward.x >= FP.Zero ? FP.One : -FP.One;
            LSCollisionComponent collision = caster.LSWorld()?.GetComponent<LSCollisionComponent>();
            if (collision != null)
            {
                collision.MoveByStep(caster, new TSVector(facing * distance, FP.Zero, FP.Zero));
                return;
            }
            caster.Position += new TSVector(facing * distance, FP.Zero, FP.Zero);
        }
    }
}
