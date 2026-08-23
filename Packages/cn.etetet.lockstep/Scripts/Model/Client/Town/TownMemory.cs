using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 跨场景的城镇位置记忆（阶段B 记、阶段C 回城用，03 文档 §1.2）：Room 销毁不丢——
    /// N 匹配进战斗前记当前位置，战斗结束回城恢复。默认 (0,0,0)=街道中段。
    /// </summary>
    public static class TownMemory
    {
        [StaticField]
        public static TSVector LastTownPosition;
    }
}
