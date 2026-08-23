using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 城镇本地玩家（03 文档 §2.1）：客户端权威——输入层 WASD 直改 Position（不走帧同步），
    /// LastTownPosition 语义由调用方在匹配前自行记住（回城恢复，03 文档 §1.2）。
    /// </summary>
    [ComponentOf(typeof (Room))]
    public class TownPlayerComponent: Entity, IAwake, IDestroy
    {
        public TSVector Position;

        public TSVector Forward;
    }
}
