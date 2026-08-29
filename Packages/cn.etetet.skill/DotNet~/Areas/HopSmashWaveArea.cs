using TrueSync;

namespace ET
{
    /// <summary>
    /// 崩山击·落地冲击波（hopsmashsub.obj 直译：pass all 全穿透，Front1 主层 + Front2 辉光背层）。
    /// 贴地扩张盒（F3 盒 x∈[-145,292] ≈ 437px 宽），down/push0/lift300 击倒浮空 + 出血
    /// （血气旺盛常开 → BleedBuff 100%）。以落点为中心（前冲终点身前 0.5）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/065-HopSmash.md §2.3
    /// </summary>
    [AreaId(AreaIds.HopSmashWave)]
    public class HopSmashWaveArea : AreaDefinition
    {
        public override int TotalTimeMs => 480;   // PO 动画 6 帧 480ms
        public override int TickTimeMs => 0;      // 一次性（Enter 即结）

        public override TSVector HalfExtents => new((FP)22 / 10, (FP)5 / 10, (FP)4 / 10);

        // hopsmashsub.atk 直译：down/push 0/lift 300
        private static readonly HitReaction Reaction = new()
        {
            Damage = 130,
            HitstunMs = 800,
            KnockbackX = 0,
            LaunchY = 300,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit, ActionIds.AddBleedBuff };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.HopSmashWaveFront;
        public override int ViewBackAnimId => AnimId.HopSmashWaveGlow;
    }
}
