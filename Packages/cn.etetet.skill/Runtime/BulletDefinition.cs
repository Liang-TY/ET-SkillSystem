using TrueSync;

namespace ET
{
    /// <summary>投射物配置类标记（值=BulletID）。</summary>
    public class BulletIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public BulletIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>投射物 ID 常量。</summary>
    public static class BulletIds
    {
        public const int NormalWave = 1;   // 地裂·波动剑（穿透地波，命中伤害+硬直）
    }

    /// <summary>
    /// 投射物配置基类（无状态属性类，[BulletId] 反射注册——与技能/Buff/Action 同构）。
    /// 命中效果走 HitActions；碰撞用 AABB（横向波贴合；扇形/环形以后需要再加 LSShapeData）。
    /// 多段间隔命中（DNF resetHitObjectList：HitTargets 定时清空反复结算）记档后续。
    /// </summary>
    public abstract class BulletDefinition
    {
        /// <summary>飞行速度（单位/秒）</summary>
        public virtual FP Speed => 10;

        /// <summary>寿命 ms（穿透弹靠到时销毁）</summary>
        public virtual int TotalTimeMs => 1500;

        /// <summary>碰撞盒半尺寸（单位）</summary>
        public virtual TSVector HalfExtents => new((FP)6 / 10, (FP)4 / 10, (FP)3 / 10);

        /// <summary>命中即毁（false=穿透，靠 HitTargets 去重 + 寿命结束）</summary>
        public virtual bool DestroyOnHit => false;

        /// <summary>命中效果（ActionIds）</summary>
        public virtual int[] HitActions => null;

        /// <summary>视图动画 id（视图层按此查 clip 自推帧；逻辑不消费。属性名避开 AnimId 类型名）</summary>
        public virtual int ViewAnimId => AnimId.None;
    }

    /// <summary>Bullet 注册表薄壳。</summary>
    public static class BulletLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<BulletIdAttribute, BulletDefinition>.RegisterAssembly(assembly);

        public static BulletDefinition Get(int bulletId)
            => ContentLoader<BulletIdAttribute, BulletDefinition>.Get(bulletId);
    }
}
