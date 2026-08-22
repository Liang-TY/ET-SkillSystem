using TrueSync;

namespace ET
{
    /// <summary>怪物 AI 配置类标记（值=MonsterAiID）。MonsterLoader/RegisterAssembly 反射扫描注册。</summary>
    public class MonsterAiIdAttribute : BaseAttribute, IContentIdAttribute
    {
        public int Id { get; }

        public MonsterAiIdAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>怪物 AI 配置 ID 常量。</summary>
    public static class MonsterAiIds
    {
        public const int BantuAmazones = 1;   // 班图女战士（.mob 数据驱动，见 02 文档 §10.2）
    }

    /// <summary>
    /// 怪物 AI 配置基类（第六类内容，无状态属性类——同 SkillLogic/AreaDefinition）。
    /// 全部数值来自 DNF .mob/attack kind 直译；AI 行为机（LSMonsterAIComponentSystem）只读配置零常量。
    /// 配置类不准存运行状态（守门员强制）——状态进 LSMonsterAIComponent（快照）。
    /// 数值出处逐行标注，新怪物按 .mob 抄一份 override 即可（02 文档 §10.2 有数据全录）。
    /// </summary>
    public abstract class MonsterAiDefinition
    {
        // ---- 索敌 ----
        /// <summary>视野/仇恨半径（单位）。.mob [sight] 600px</summary>
        public virtual FP SightRange => 6;

        // ---- 移动 ----
        /// <summary>移动速度（单位/s）。.mob [move speed] 1000 → demo 换算 4（玩家 6，怪追不上玩家）</summary>
        public virtual FP MoveSpeed => 4;

        /// <summary>移动动画（循环）</summary>
        public virtual int MoveAnimId => AnimId.Walk;

        /// <summary>待机动画</summary>
        public virtual int IdleAnimId => AnimId.Idle;

        // ---- 决策节奏 ----
        /// <summary>行为重估节流 ms（行为机调度周期）</summary>
        public virtual int ThinkIntervalMs => 1000;

        /// <summary>两次出手最小间隔 ms。.mob [attack delay] 800</summary>
        public virtual int AttackIntervalMs => 800;

        // ---- 近战选招池（attack kind 权重列直译）----
        /// <summary>近战技能池（static readonly 数组防 GC）</summary>
        public virtual int[] MeleeSkillIds => null;

        /// <summary>与 MeleeSkillIds 对应的选取权重（LSRng 加权）</summary>
        public virtual int[] MeleeWeights => null;

        /// <summary>近战触发距离（单位）。attack kind 第 5 列：LowKick 115px</summary>
        public virtual FP MeleeRange => (FP)115 / 100;

        // ---- 远程先手 ----
        /// <summary>远程技能（0=无）。触发窗口 (RangedMinRange, RangedMaxRange]</summary>
        public virtual int RangedSkillId => 0;

        public virtual FP RangedMinRange => (FP)12 / 10;

        public virtual FP RangedMaxRange => 6;

        // ---- 返回 ----
        /// <summary>离出生点超过此距离返回（0=不返回）</summary>
        public virtual FP HomeReturnRange => 0;
    }

    /// <summary>MonsterAi 注册表薄壳。</summary>
    public static class MonsterAiLoader
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
            => ContentLoader<MonsterAiIdAttribute, MonsterAiDefinition>.RegisterAssembly(assembly);

        public static MonsterAiDefinition Get(int monsterAiId)
            => ContentLoader<MonsterAiIdAttribute, MonsterAiDefinition>.Get(monsterAiId);
    }
}
