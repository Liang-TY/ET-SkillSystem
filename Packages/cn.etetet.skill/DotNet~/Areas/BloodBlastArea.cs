using TrueSync;

namespace ET
{
    /// <summary>
    /// 怒气爆发·外圈血柱（BlastBlood.obj 直译：血柱 F10-F13 盒，4 段多段）。
    /// Tick 450ms ×4（static[0]=4 段）；多段小抬浮空维持 juggle（Ly150）。
    /// 视觉：blastblood1.ani + .als 血柱 8 层（blood1-8 挂接）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/024-BloodBlast.md §2.3
    /// </summary>
    [AreaId(AreaIds.BloodBlastOuter)]
    public class BloodBlastArea : AreaDefinition
    {
        public override int TotalTimeMs => 1800;   // 血柱 2200ms 内 4 Tick
        public override int TickTimeMs => 450;     // static[1] 推断值

        // 主血柱盒（x[-41,80]/z[0,330]）+ static[3]=700px 半径折中
        public override TSVector HalfExtents => new((FP)35 / 10, (FP)10 / 10, (FP)165 / 100);

        // BlastBlood.atk：down / push 0 / lift 0（原地连打）——Tick 小抬维持浮空
        private static readonly HitReaction Reaction = new()
        {
            Damage = 80,
            HitstunMs = 500,
            KnockbackX = 0,
            LaunchY = 150,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] TickActionsArr = { ActionIds.MeleeHit };
        public override int[] TickActions => TickActionsArr;

        public override int ViewAnimId => AnimId.BlastBlood1;
    }
}
