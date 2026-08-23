namespace ET.Server
{
    /// <summary>
    /// 玩家进城镇（Gate 的 C2G_EnterTown 转来）：广播 EnterTown 给已有成员 → 加入成员表 → 回已有成员列表。
    /// demo 无选角：CharacterId 固定 1（鬼剑士）；出生点客户端定，服务器只记（客户端权威）。
    /// </summary>
    [MessageHandler(SceneType.Town)]
    public class G2Town_EnterPlayerHandler: MessageHandler<Scene, G2Town_EnterPlayer, Town2G_EnterPlayer>
    {
        protected override async ETTask Run(Scene root, G2Town_EnterPlayer request, Town2G_EnterPlayer response)
        {
            TownComponent town = root.GetComponent<TownComponent>();
            MessageLocationSenderComponent sender = root.GetComponent<MessageLocationSenderComponent>();

            // 广播给已有成员（广播消息不能被池回收——Create() 默认非池化）
            T2C_PlayerEnterTown enter = T2C_PlayerEnterTown.Create();
            enter.PlayerId = request.PlayerId;
            enter.CharacterId = 1;
            foreach (var kv in town.Members)
            {
                sender.Get(LocationType.GateSession).Send(kv.Key, enter);
            }

            // 加入成员表（重复进城=回城，覆盖旧记录即可）
            town.Members[request.PlayerId] = new TownPlayerInfo() { PlayerId = request.PlayerId, CharacterId = 1 };

            // 回包：已有成员列表（不含自己）
            foreach (var kv in town.Members)
            {
                if (kv.Key == request.PlayerId) continue;
                response.Members.Add(kv.Value);
            }

            Log.Info($"[Town] 玩家{request.PlayerId}进城镇，当前成员 {town.Members.Count}");
            await ETTask.CompletedTask;
        }
    }
}
