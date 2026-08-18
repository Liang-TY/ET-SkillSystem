namespace ET
{
    /// <summary>Buff 配置类标记（值=BuffID）。</summary>
    public class BuffIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public BuffIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>Buff ID 常量。</summary>
    public static class BuffIds
    {
        public const int Burn = 1;   // 燃烧：每 1 秒扣 10，持续 3 秒（FireDamageTick）
        public const int Stun = 2;   // 眩晕：1 秒禁移动（ForbidMove 开/关）
    }

    /// <summary>
    /// Buff 配置基类（无状态属性类，[BuffId] 反射注册——与技能同构）。
    /// 效果组合走 Actions；复杂流程不塞这里（那是 SkillLogic 的职责）。
    /// 后续扩展位（ET10 spell 对照记档）：Stack 上限/刷新策略、互斥组、免疫。
    /// </summary>
    public abstract class BuffDefinition
    {
        /// <summary>总时长 ms；0 = 永久（靠外部移除）</summary>
        public virtual int TotalTimeMs => 0;

        /// <summary>Tick 间隔 ms；0 = 不 Tick</summary>
        public virtual int TickTimeMs => 0;

        /// <summary>添加时执行（ActionIds；static readonly 数组避免分配）</summary>
        public virtual int[] AddActions => null;

        /// <summary>每次 Tick 执行</summary>
        public virtual int[] TickActions => null;

        /// <summary>移除时执行（到时/驱散）</summary>
        public virtual int[] RemoveActions => null;
    }

    /// <summary>Buff 注册表薄壳。</summary>
    public static class BuffLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<BuffIdAttribute, BuffDefinition>.RegisterAssembly(assembly);

        public static BuffDefinition Get(int buffId)
            => ContentLoader<BuffIdAttribute, BuffDefinition>.Get(buffId);
    }
}
