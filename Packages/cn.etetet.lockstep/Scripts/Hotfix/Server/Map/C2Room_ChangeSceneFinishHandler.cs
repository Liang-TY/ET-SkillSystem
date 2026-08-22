using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    [FriendOf(typeof (RoomServerComponent))]
    public class C2Room_ChangeSceneFinishHandler: MessageHandler<Scene, C2Room_ChangeSceneFinish>
    {
        protected override async ETTask Run(Scene root, C2Room_ChangeSceneFinish message)
        {
            Room room = root.GetComponent<Room>();
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();
            RoomPlayer roomPlayer = room.GetComponent<RoomServerComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.Progress = 100;
            
            if (!roomServerComponent.IsAllPlayerProgress100())
            {
                return;
            }
            
            await room.Fiber.Root.GetComponent<TimerComponent>().WaitAsync(1000);

            // 玩家出生点来自地图配置（MapDefinition.PlayerSpawn；空地图退化为原点）
            MapDefinition mapDef = MapLoader.Get(room.MapId);
            TSVector playerSpawn = mapDef != null ? mapDef.PlayerSpawn : TSVector.zero;

            Room2C_Start room2CStart = Room2C_Start.Create();
            room2CStart.StartTime = TimeInfo.Instance.ServerFrameTime();
            room2CStart.MapId = room.MapId;
            foreach (RoomPlayer rp in roomServerComponent.Children.Values)
            {
                LockStepUnitInfo lockStepUnitInfo = LockStepUnitInfo.Create();
                lockStepUnitInfo.PlayerId = rp.Id;
                lockStepUnitInfo.Position = playerSpawn;
                lockStepUnitInfo.Rotation = TSQuaternion.identity;
                room2CStart.UnitInfo.Add(lockStepUnitInfo);
            }

            room.Init(room2CStart.UnitInfo, room2CStart.StartTime, room.MapId);

            room.AddComponent<LSServerUpdater>();

            RoomMessageHelper.BroadCast(room, room2CStart);
        }
    }
}