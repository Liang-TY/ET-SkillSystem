using TrueSync;

namespace ET
{
    /// <summary>区域效果配置类标记（值=AreaID）。</summary>
    public class AreaIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public AreaIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>区域效果 ID 常量。</summary>
    public static class AreaIds
    {
        public const int BloodBoom = 2;    // 浴血之怒爆炸：以施法者为中心的一次性血爆（伤害+出血）
        public const int TripleSlashFinish = 3;  // 三段斩终段击倒（身前一次性，本体盒让位防双结算）
        public const int AshenFork = 4;          // 银光落刃落地冲击波（落点中心贴地，击倒浮空）
    }

    /// <summary>
    /// 区域效果配置基类（无状态属性类，[AreaId] 反射注册——第五类内容，同 Skill/Buff/Action/Bullet）。
    /// EnterActions = 单位首次进入时执行；TickActions = 每 TickIntervalMs 对区域内单位执行；
    /// ExitActions = 单位离开/区域消失时执行。碰撞用 AABB。
    /// </summary>
    public abstract class AreaDefinition
    {
        /// <summary>总时长 ms（0 = 永久）</summary>
        public virtual int TotalTimeMs => 5000;

        /// <summary>Tick 间隔 ms（0 = 不 Tick）</summary>
        public virtual int TickTimeMs => 1000;

        /// <summary>碰撞盒半尺寸（单位，中心 = 区域中心）</summary>
        public virtual TSVector HalfExtents => new((FP)15 / 10, (FP)5 / 10, (FP)15 / 10);

        /// <summary>进入时执行（ActionIds）</summary>
        public virtual int[] EnterActions => null;

        /// <summary>每次 Tick 对区域内单位执行</summary>
        public virtual int[] TickActions => null;

        /// <summary>离开/消失时执行</summary>
        public virtual int[] ExitActions => null;

        /// <summary>命中反应参数（DNF .atk 同构：Enter/Tick/Exit 打到的单位用；MeleeHit 等节点读取）</summary>
        public virtual HitReaction HitReaction => HitReaction.Default;

        /// <summary>视图动画 id（循环的火焰/地面特效）</summary>
        public virtual int ViewAnimId => AnimId.None;

        /// <summary>视图收尾动画 id（到时熄灭，不循环；AnimId.None = 无收尾）</summary>
        public virtual int ViewEndAnimId => AnimId.None;

        /// <summary>背面视图动画 id（爆炸前后两层，如浴血之怒 boomback；独立帧推进，播完停末帧。
        /// AnimId.None = 单层。渲染在主层之后（sortingOrder 更低））</summary>
        public virtual int ViewBackAnimId => AnimId.None;
    }

    /// <summary>Area 注册表薄壳。</summary>
    public static class AreaLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<AreaIdAttribute, AreaDefinition>.RegisterAssembly(assembly);

        public static AreaDefinition Get(int areaId)
            => ContentLoader<AreaIdAttribute, AreaDefinition>.Get(areaId);
    }
}
