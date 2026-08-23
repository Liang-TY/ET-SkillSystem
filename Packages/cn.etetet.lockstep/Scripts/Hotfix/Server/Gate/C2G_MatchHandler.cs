namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_MatchHandler : MessageSessionHandler<C2G_Match, G2C_Match>
	{
		protected override async ETTask Run(Session session, C2G_Match request, G2C_Match response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			// 城镇→战斗的路由切换（03 文档 §1.2）：通知 TownScene 移除+广播离开 → 摘 PlayerTownComponent
			PlayerTownComponent townComponent = player.GetComponent<PlayerTownComponent>();
			if (townComponent != null)
			{
				G2Town_LeavePlayer g2TownLeave = G2Town_LeavePlayer.Create();
				g2TownLeave.PlayerId = player.Id;
				session.Root().GetComponent<MessageSender>().Send(townComponent.TownActorId, g2TownLeave);   // Send 是 void（投递即返回）
				player.RemoveComponent<PlayerTownComponent>();
			}

			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetOneBySceneType(session.Zone(), SceneType.Match);

			G2Match_Match g2MatchMatch = G2Match_Match.Create();
			g2MatchMatch.Id = player.Id;
			g2MatchMatch.MapId = request.MapId;
			await session.Root().GetComponent<MessageSender>().Call(startSceneConfig.ActorId, g2MatchMatch);
		}
	}
}