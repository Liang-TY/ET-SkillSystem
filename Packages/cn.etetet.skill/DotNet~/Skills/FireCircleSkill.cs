namespace ET
{
    /// <summary>火圈技能（O 键）：在身前 3 单位处生成火圈，持续 5 秒，范围内每秒扣血。</summary>
    [SkillId(SkillIds.FireCircle)]
    public class FireCircleSkill : SkillLogic
    {
        public override int CooldownMs => 3000;
        public override int TotalTimeMs => 350;

        public override void OnCast(SkillContext ctx)
        {
            ctx.CreateAreaInFront(AreaIds.FireCircle, (TrueSync.FP)3 / 1);
        }
    }
}
