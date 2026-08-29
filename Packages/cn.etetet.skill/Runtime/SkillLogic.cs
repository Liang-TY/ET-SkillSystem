using TrueSync;

namespace ET
{
    /// <summary>
    /// 技能逻辑基类——**无状态**（帧同步回滚硬要求：逻辑实例不进快照，状态必须存 LSCast 实体；
    /// SkillLoader.RegisterAssembly 守门员机器强制——有非 const 实例字段拒绝注册）。
    /// 全单位共享单例；生命周期回调只通过 SkillContext 门面访问世界。
    /// 编写规范见 Packages/cn.etetet.skill/CLAUDE.md。
    /// </summary>
    public abstract class SkillLogic
    {
        /// <summary>
        /// 冷却 ms。CD 双机制（DNF 实证）：默认 TryCast 成功即进 CD（.skl [auto cooltime apply]）；
        /// ManualCooldown=true 时延迟到 OnEnd 后进 CD（= startSkillCoolTime，多段技能用）。
        /// </summary>
        public virtual int CooldownMs => 0;

        /// <summary>手动 CD 开关（多段技能在 OnEnd 才起 CD）</summary>
        public virtual bool ManualCooldown => false;

        /// <summary>总时长 ms；>0 到时自动 OnEnd，0 = 技能自己控制结束</summary>
        public virtual int TotalTimeMs => 0;

        /// <summary>
        /// 施放所需最低自身 HP 百分比（0 = 无门槛；DNF checkExecutableSkill 的 static[0] 同构，
        /// 如浴血之怒"可发动的最低 HP 10%"）。TryCast 检查：不满足 → 拒绝施放且不进 CD。
        /// </summary>
        public virtual FP MinCastHpPct => FP.Zero;

        /// <summary>
        /// 是否要求空中施放（true = 只在离地时可放，银光落刃等跳跃系技能；DNF"跳跃状态中按指令"同构）。
        /// TryCast 检查：地面施放 → 拒绝且不进 CD。
        /// </summary>
        public virtual bool RequireAirborne => false;

        /// <summary>
        /// 命中反应参数（DNF .atk 同构）：伤害/硬直/击退/浮空。override 时用 static readonly
        /// 预分配实例（零 GC）；默认 = 50 伤害 + 500ms 硬直（与旧 MeleeHit 硬编码一致）。
        /// 区域/投射物结算的伤害在各自 AreaDefinition/BulletDefinition.HitReaction。
        /// </summary>
        public virtual HitReaction HitReaction => HitReaction.Default;

        /// <summary>
        /// 分段命中反应（DNF 单技能多段不同 .atk 同构：如崩山击多段 push30/末击 down 击倒）。
        /// phase = 命中时的 LSCast.SubState（连段技能=段号/多段技能=命中窗口号），默认忽略 phase
        /// 返回 HitReaction。多段技能 override 本方法代替"末击走小 Area"的手法（回滚安全，零编排）。
        /// </summary>
        public virtual HitReaction PhaseHitReaction(int phase) => HitReaction;

        /// <summary>
        /// 命中时执行的效果节点列表（ActionIds）——伤害/硬直/挂 Buff 等效果全在节点里配置组合，
        /// 生命周期类只管"过程"（连段/取消/位移）。用 static readonly 数组避免每次命中分配。
        /// attack 表化（luban 专题）后此配置迁表。
        /// </summary>
        public virtual int[] HitActions => null;

        public virtual void OnCast(SkillContext ctx) { }

        public virtual void OnUpdate(SkillContext ctx, int dtMs) { }

        public virtual void OnEnd(SkillContext ctx) { }
    }
}
