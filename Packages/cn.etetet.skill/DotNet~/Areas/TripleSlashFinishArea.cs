using TrueSync;

namespace ET
{
    /// <summary>
    /// 三段斩终段击倒区域（tripleslash3down.atk 直译：down/push 300/lift 200）。
    /// 技能级 HitReaction 是单值放不下终段第二套参数 → 一次性 Area（BloodBoom 范式）；
    /// 终段期间本体盒让位（TripleSlashSkill.OnUpdate），不会双结算。
    /// 视觉由角色动画承担（ViewAnimId=None）。
    /// </summary>
    [AreaId(AreaIds.TripleSlashFinish)]
    public class TripleSlashFinishArea : AreaDefinition
    {
        public override int TotalTimeMs => 280;   // 终段活跃帧窗口
        public override int TickTimeMs => 0;      // 一次性（Enter 即结）

        // 终段击倒盒：半尺寸 (0.8 横向, 0.4 高度, 0.7 纵深)
        public override TSVector HalfExtents => new((FP)8 / 10, (FP)4 / 10, (FP)7 / 10);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 90,
            HitstunMs = 800,      // down 击倒 → 硬直托底 + 击飞落地 Down 链
            KnockbackX = 300,     // push 300
            LaunchY = 200,        // lift 200
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;
    }
}
