using TrueSync;

namespace ET
{
    /// <summary>
    /// 裂波斩·裂波波轮（vaneslash.obj 相位1 直译：pass all 全穿透，20 帧旋转）。
    /// 多段 3 次（static[1]）= Tick 350ms ×3（350ms 起跳与 PO 无盒暖机段对齐）；
    /// 以身前固定点为中心（DNF 以被抓敌人为中心——抓取系统简化）。
    /// 视图：VaneSlash.ani（F13 悬停帧已钳 80ms，总 1410ms）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/058-VaneSlash.md §2.3
    /// </summary>
    [AreaId(AreaIds.VaneSlashWheel)]
    public class VaneSlashWheelArea : AreaDefinition
    {
        public override int TotalTimeMs => 1250;
        public override int TickTimeMs => 350;    // 350/700/1050ms = 3 段

        // PO 主盒 x∈[-76,212]（2.88 单位宽）折算半尺寸
        public override TSVector HalfExtents => new((FP)14 / 10, (FP)5 / 10, (FP)45 / 100);

        // vaneslash.atk 直译：damage/push 100/lift 50
        private static readonly HitReaction Reaction = new()
        {
            Damage = 45,
            HitstunMs = 500,
            KnockbackX = 100,
            LaunchY = 50,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] TickActionsArr = { ActionIds.MeleeHit };
        public override int[] TickActions => TickActionsArr;

        public override int ViewAnimId => AnimId.VaneSlashWheel;
    }
}
