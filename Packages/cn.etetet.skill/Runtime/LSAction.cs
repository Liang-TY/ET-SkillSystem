using TrueSync;

namespace ET
{
    /// <summary>效果节点标记（值=ActionID）。</summary>
    public class ActionIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public ActionIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>效果节点 ID 常量。参数第一版内嵌在节点类 const（luban 化时参数进表，见 05 §5 记档）。</summary>
    public static class ActionIds
    {
        public const int MeleeHit = 1;        // 近战命中：伤害 + 硬直 + 受击动画（参数读来源 HitReaction）
        public const int FireDamageTick = 2;  // 燃烧 tick 伤害
        public const int ForbidMoveOn = 3;    // 禁移动开（眩晕 AddActions）
        public const int ForbidMoveOff = 4;   // 禁移动关（眩晕 RemoveActions）
        public const int AddBurnBuff = 5;     // 命中挂燃烧
        public const int BleedDamageTick = 6; // 出血 tick 伤害
        public const int AddBleedBuff = 7;    // 命中挂出血
    }

    /// <summary>
    /// 效果节点基类——无状态（守门员强制），全单位共享单例。
    /// 执行时机：技能命中（SkillLogic.HitActions）/ Buff 添加-Tick-移除（BuffDefinition.*Actions）。
    /// 只通过 LSActionContext 门面访问世界（同 SkillContext 门面模式）。
    /// </summary>
    public abstract class LSAction
    {
        public virtual void Run(LSActionContext ctx) { }
    }

    /// <summary>Action 注册表薄壳。</summary>
    public static class ActionLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<ActionIdAttribute, LSAction>.RegisterAssembly(assembly);

        public static LSAction Get(int actionId)
            => ContentLoader<ActionIdAttribute, LSAction>.Get(actionId);
    }

    /// <summary>
    /// 效果节点上下文（readonly struct 门面，同 SkillContext 模式：零 GC、重入安全、不漏实体类型）。
    /// owner = 效果作用单位（受击者/buff 宿主）；source = 施加者（攻击方/buff 来源）。
    /// </summary>
    public readonly struct LSActionContext
    {
        private readonly LSWorld world;
        private readonly LSUnit owner;
        private readonly LSUnit source;
        private readonly int frameNo;
        private readonly HitReaction hitReaction;   // 来源（技能/区域/投射物）的命中反应参数；null = 默认

        public LSActionContext(LSWorld world, LSUnit owner, LSUnit source, int frameNo, HitReaction hitReaction = null)
        {
            this.world = world;
            this.owner = owner;
            this.source = source;
            this.frameNo = frameNo;
            this.hitReaction = hitReaction;
        }

        public int FrameNo => frameNo;

        public long GetOwnerId() => owner.Id;

        public long GetSourceId() => source != null ? source.Id : 0;

        /// <summary>来源命中反应参数（SkillLogic/AreaDefinition/BulletDefinition.HitReaction 由调用方传入；
        /// 未传如 Buff 动作 = Default 50/500，与旧行为一致）</summary>
        public HitReaction GetSourceHitReaction() => hitReaction ?? HitReaction.Default;

        // ---- 数值 ----
        public void DamageOwner(int damage)
            => owner.GetComponent<LSNumericComponent>()?.Add(NumericType.Hp, -damage);

        public FP GetOwnerHp()
            => owner.GetComponent<LSNumericComponent>()?.Get(NumericType.Hp) ?? FP.Zero;

        public void AddOwnerNumeric(int numericKey, int value)
            => owner.GetComponent<LSNumericComponent>()?.Add(numericKey, value);

        // ---- 战斗状态 ----
        public void SetOwnerHitstun(int ms)
        {
            LSCombatComponent combat = owner.GetComponent<LSCombatComponent>();
            if (combat != null) combat.HitstunTimer = ms;   // 重打刷新（DNF 行为）
        }

        /// <summary>禁移动开关（眩晕类；NumericType.ForbidMove 的门面封装——内容层不见 ET.Model 类型）</summary>
        public void OwnerForbidMove(bool on)
            => owner.GetComponent<LSNumericComponent>()?.Add(NumericType.ForbidMove, on ? 1 : -1);

        /// <summary>
        /// 击退/浮空（DNF .atk push aside / lift up 同构）：给 owner 写飞行初速度，
        /// 之后 LSFlightComponentSystem 重力积分（抛物线 + 落地即停，参数见那边注释）。
        /// 参数为 DNF 像素值：knockbackX 400 → 水平初速 10 单位/s；liftY 400 → 垂直初速 8 单位/s。
        /// 方向 = 攻击者指向受击者（被推开）。
        /// </summary>
        public void LaunchOwner(FP knockbackX, FP liftY)
        {
            LSFlightComponent flight = owner.GetComponent<LSFlightComponent>();
            if (flight == null) return;
            FP dir = source != null && owner.Position.x < source.Position.x ? -FP.One : FP.One;
            flight.Velocity = new TSVector(
                dir * (knockbackX / 100) * ((FP)5 / 2),   // px → 单位/s ×2.5
                (liftY / 100) * (FP)2,                      // px → 单位/s ×2.0
                FP.Zero);
            flight.Active = true;   // 重打刷新（空中再被击 → 覆盖速度，DNF 行为）
        }

        /// <summary>受击者自己的受击动画 ID（DNF sq_GetDamageAni 同构——每角色自带，0=未配置）</summary>
        public int GetOwnerHurtAnimId()
        {
            LSCombatComponent combat = owner.GetComponent<LSCombatComponent>();
            return combat != null ? combat.HurtAnimId : 0;
        }

        // ---- 动画 ----
        public void PlayOwnerAnim(int animId) => LSAnimPlayUtil.Play(owner, animId);

        // ---- Buff ----
        public void AddBuffToOwner(int buffId)
            => owner.GetComponent<LSBuffComponent>()?.AddBuff(source, buffId);
    }

    /// <summary>
    /// 播动画的属性赋值实现（LSAnimComponentSystem.Play 是 ET.Hotfix 扩展，ET.Skill 引用不到；
    /// 两处门面共用这一份，保持单一同步点）。
    /// </summary>
    internal static class LSAnimPlayUtil
    {
        internal static void Play(LSUnit unit, int animId)
        {
            LSAnimComponent anim = unit.GetComponent<LSAnimComponent>();
            if (anim == null) return;
            anim.AnimId = animId;
            anim.FrameIndex = 0;
            anim.FrameTick = FP.Zero;
            anim.Speed = FP.One;
            anim.IsFinished = false;
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip != null) anim.IsLoop = clip.loop;
        }
    }
}
