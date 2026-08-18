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

        public TSVector GetTargetPosition() => cast.TargetPosition;

        // ---- 动画 ----
        /// <summary>播放动画（LSAnimComponentSystem.Play 是 ET.Hotfix 扩展，ET.Skill 引用不到，
        /// 统一走 LSAnimPlayUtil 属性赋值实现——与那边保持同步）</summary>
        public void PlayAnim(int animId) => LSAnimPlayUtil.Play(caster, animId);

        public void PlayDefaultAnim()
        {
            LSCombatComponent combat = caster.GetComponent<LSCombatComponent>();
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
        /// <summary>给目标挂 Buff（source = 施法者）</summary>
        public void AddBuff(LSUnit target, int buffId)
            => target.GetComponent<LSBuffComponent>()?.AddBuff(caster, buffId);

        /// <summary>给施法者自己挂 Buff（自伤/变形/测试用）</summary>
        public void AddBuffToSelf(int buffId)
            => caster.GetComponent<LSBuffComponent>()?.AddBuff(caster, buffId);

        // ---- 连段取消：结束当前施放并立刻重施同技能（取消窗口用）----
        public void RestartCurrentSkill()
        {
            ConsumeBuffer();
            cast.EndNow(this);
            SkillCastHelper.TryCast(caster, cast.SkillId);
        }
    }
}
