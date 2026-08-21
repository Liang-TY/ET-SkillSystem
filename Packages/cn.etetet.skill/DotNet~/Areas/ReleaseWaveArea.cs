using TrueSync;

namespace ET
{
    /// <summary>
    /// 波动爆发爆发（DNF 被动对象 24389 子id9 同构）：以施放点为中心的一次性爆发，
    /// 把周围敌人朝施放者方向击飞推走（releasewave.atk：魔法/击倒/push 400/lift 400/blow）。
    /// 判定 = releasewave1.ani 全帧攻击盒（原点对称 400×100×234px → x±2/纵深±0.5/高 2.34 单位）。
    /// 视觉 = rw_burst1 主层（5 帧 490ms）+ rw_burst_overlay 合并挂层（爆炸 3 子层 + backwind 蓄气 5 层，
    /// DNF 里蓄气特效留在施放点与爆炸同点，故合并到爆炸 Area 的视图）。
    /// </summary>
    [AreaId(AreaIds.ReleaseWave)]
    public class ReleaseWaveArea : AreaDefinition
    {
        // releasewave.atk（demo：伤害 150——小伤害大控制技；硬直 800ms 表现击倒）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 150,
            HitstunMs = 800,
            KnockbackX = 400,
            LaunchY = 400,
        };
        public override HitReaction HitReaction => Reaction;

        public override int TotalTimeMs => 490;    // releasewave1.ani 总时长（70+70+210+70+70）
        public override int TickTimeMs => 0;       // 单次（EnterActions 打完即止，DNF 无 resetHitObjectList 多段）

        public override TSVector HalfExtents => new(2, (FP)117 / 100, (FP)5 / 10);

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.ReleaseWaveBurst1;   // 爆炸主层
        public override int ViewEndAnimId => AnimId.None;             // 播完即销毁（无收尾）
    }
}
