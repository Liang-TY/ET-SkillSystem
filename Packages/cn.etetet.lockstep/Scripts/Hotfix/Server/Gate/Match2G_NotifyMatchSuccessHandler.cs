using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class Match2G_NotifyMatchSuccessHandler : MessageHandler<Player, Match2G_NotifyMatchSuccess>
	{
		protected override async ETTask Run(Player player, Match2G_NotifyMatchSuccess message)
		{
			// GetOrAdd 语义：连续多局匹配时组件已存在，重复 AddComponent 会抛（entity already has component）
			PlayerRoomComponent roomComponent = player.GetComponent<PlayerRoomComponent>();
			if (roomComponent == null)
			{
				roomComponent = player.AddComponent<PlayerRoomComponent>();
			}
			roomComponent.RoomActorId = message.ActorId;

			player.GetComponent<PlayerSessionComponent>().Session.Send(message);
			await ETTask.CompletedTask;
		}
	}
}