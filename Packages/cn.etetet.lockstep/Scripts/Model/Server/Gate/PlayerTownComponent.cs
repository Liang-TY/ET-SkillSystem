namespace ET.Server
{
    /// <summary>
    /// 在城镇标记（抄 PlayerRoomComponent）：Gate 按它把 ITownMessage 路由到 TownScene 纤程。
    /// 加/删本组件 = 城镇路由开关（进城镇挂、匹配进战斗摘，03 文档 §1.2）。
    /// </summary>
    [ComponentOf(typeof (Player))]
    public class PlayerTownComponent: Entity, IAwake
    {
        public ActorId TownActorId { get; set; }
    }
}
