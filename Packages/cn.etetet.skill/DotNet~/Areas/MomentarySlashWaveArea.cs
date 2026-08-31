using TrueSync;

namespace ET
{
    /// <summary>
    /// 拔刀斩·拔刀大波（momentaryslashwave.obj 视觉——New_BigWave 斩光，纯视觉无判定，
    /// 判定走技能手动盒）。start.ani F0 悬停帧已钳 200ms → 480ms 总长。
    /// 参考：Notes/技能实现/鬼剑士技能解析/009-MomentarySlash.md §2.3
    /// </summary>
    [AreaId(AreaIds.MomentarySlashWave)]
    public class MomentarySlashWaveArea : AreaDefinition
    {
        public override int TotalTimeMs => 480;
        public override int TickTimeMs => 0;

        public override TSVector HalfExtents => TSVector.zero;   // 纯视觉无判定

        public override int ViewAnimId => AnimId.MomentarySlashWave;
        public override int ViewBackAnimId => AnimId.MomentarySlashWaveB;
    }
}
