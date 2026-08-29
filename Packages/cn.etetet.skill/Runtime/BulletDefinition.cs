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
        public const int IceBreath = 2;    // 冰息弹（班图女战士冰雾，穿透+10% 冰冻）
        public const int ThrustBeam = 3;   // 连突刺激光剑气（贴身不飞行穿透短命弹）
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

        /// <summary>命中反应参数（DNF .atk 同构：命中单位用；MeleeHit 等节点读取）</summary>
        public virtual HitReaction HitReaction => HitReaction.Default;

        /// <summary>出生偏移（DNF .mob [throw attack] 三轴 / .nut CreatePassiveObject 参数 直译；单位=游戏单位）。
        /// x=身前距离（沿朝向）、y=高度、z=纵深。默认：身前 0.8（通用出生位）+ 碰撞盒半高（贴地飞行）。
        /// 每个 DNF 弹按原数据 override 三轴即可，无需手调。</summary>
        public virtual TSVector SpawnOffset => new((FP)8 / 10, HalfExtents.y, FP.Zero);

        /// <summary>视图是否贴地渲染（true=GO 落地面，地波类；false=GO 用逻辑高度，空中弹类）</summary>
        public virtual bool ViewGrounded => true;

        /// <summary>视图摆位补偿（面右为正，视图层按朝向镜像）：逻辑碰撞中心与 DNF PO 原点不重合时，
        /// 把视觉锚回 PO 原点——DNF 的 PO 贴图 imagePos 以自身原点为锚，GO 偏到碰撞中心会双重偏移</summary>
        public virtual TSVector ViewOffset => TSVector.zero;

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
