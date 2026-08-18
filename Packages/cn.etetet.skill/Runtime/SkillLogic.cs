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

        public virtual void OnCast(SkillContext ctx) { }

        public virtual void OnUpdate(SkillContext ctx, int dtMs) { }

        public virtual void OnEnd(SkillContext ctx) { }
    }
}
