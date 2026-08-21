namespace ET
{
    /// <summary>
    /// 命中反应参数（DNF .atk 同构：每个技能/区域/投射物独立的"被打者怎么反应"配置）。
    /// SkillLogic/AreaDefinition/BulletDefinition 的 HitReaction 虚属性提供，MeleeHit 等命中节点读取。
    ///
    /// 帧同步/回滚安全性：纯只读配置数据——static readonly 预分配共享实例，不进快照、不参与回滚。
    /// SkillLogic 等配置类不是 LSEntity/Component/System，不在 ET 分析器管辖名单，字段属性合法
    /// （机器不管，规范见 skill/CLAUDE.md）。
    ///
    /// luban 前置形态：C# 虚属性 → 以后迁配置表只改 getter 为表查询，其余零改动。
    /// 击退/浮空字段待 Z 轴物理系统（延后记档），当前只有 Damage/HitstunMs 生效。
    /// </summary>
    [EnableClass]
    public class HitReaction
    {
        /// <summary>兜底默认（= 原 MeleeHitAction 硬编码值：未 override 的技能/区域/弹行为不变）</summary>
        public static readonly HitReaction Default = new() { Damage = 50, HitstunMs = 500 };

        public int Damage;      // 伤害
        public int HitstunMs;   // 受击硬直 ms（重打刷新）
        public int KnockbackX;  // 击退力（px）
        public int LaunchY;     // 浮空力（px）

        // 命中按概率附加的 Buff（DNF .atk [active status] 同构：出血/冰冻/感电…）
        // v1 简化：时长/每跳伤害用各 Buff 预设，不随 .atk 参数化（记档 02 文档 §9）
        public int ProcBuffId;  // 0 = 无
        public int ProcChance;  // 概率 %（0-100；LSRng 确定性判定）
    }
}
