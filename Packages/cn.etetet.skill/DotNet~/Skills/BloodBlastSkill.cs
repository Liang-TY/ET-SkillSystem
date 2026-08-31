using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·怒气爆发（全引擎内置——角色无动作，姿势由血柱 PO 层承担；三件 PO → 三 Area 同构）。
    /// DNF：自身周围怒气爆发，先手浮空（PreSub lift400）→ 血柱 4 段（Tick450），中心敌人被
    /// 外圈+内圈双 Area 同时覆盖 = 8 段（explain 中心双倍，数学等效）。血气狂暴前置跳过。
    /// 参考：Notes/技能实现/鬼剑士技能解析/024-BloodBlast.md
    /// </summary>
    [SkillId(SkillIds.BloodBlast)]
    public class BloodBlastSkill : SkillLogic
    {
        public override int CooldownMs => 2000;    // .skl 16000（用户定案：本批 CD 全 2s）
        public override int TotalTimeMs => 400;    // 角色侧无动作——施法锁 400ms 后交棒给 Area

        public override void OnCast(SkillContext ctx)
        {
            // 自身中心三 Area（外圈+内圈同心；先手段）
            ctx.CreateAreaInFront(AreaIds.BloodBlastPre, FP.Zero);
            ctx.CreateAreaInFront(AreaIds.BloodBlastOuter, FP.Zero);
            ctx.CreateAreaInFront(AreaIds.BloodBlastCore, FP.Zero);
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
