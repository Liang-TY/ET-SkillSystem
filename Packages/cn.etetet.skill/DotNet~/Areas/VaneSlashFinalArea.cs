using TrueSync;

namespace ET
{
    /// <summary>
    /// 裂波斩·终结爆发（vaneslash.obj 相位2 直译：VaneSlashNormal.ani 4 帧 280ms）。
    /// 下斩把敌人砸飞出去（VaneSlashFinal.atk：down/push 500/lift 200 大击飞）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/058-VaneSlash.md §2.2 表④
    /// </summary>
    [AreaId(AreaIds.VaneSlashFinal)]
    public class VaneSlashFinalArea : AreaDefinition
    {
        public override int TotalTimeMs => 280;
        public override int TickTimeMs => 0;

        public override TSVector HalfExtents => new((FP)12 / 10, (FP)5 / 10, (FP)5 / 10);

        private static readonly HitReaction Reaction = new()
        {
            Damage = 120,
            HitstunMs = 800,
            KnockbackX = 500,
            LaunchY = 200,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.VaneSlashNormal;
    }
}
