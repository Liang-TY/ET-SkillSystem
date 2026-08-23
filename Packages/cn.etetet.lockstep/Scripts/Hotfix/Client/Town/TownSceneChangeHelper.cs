using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 城镇场景切换协程（MMO 模式非锁步，03 文档 §2）：与战斗 SceneChangeTo 的区别——
    /// 无 Room2C_Start 等待、无 room.Init/LSWorld、无 LSClientUpdater；本地玩家即权威。
    /// </summary>
    [FriendOf(typeof(TownPlayerComponent))]
    public static partial class TownSceneChangeHelper
    {
        public static async ETTask SceneChangeToTown(Scene root, TSVector spawnPosition)
        {
            root.RemoveComponent<Room>();

            // 战斗链 Room id 也是 1（ActorId.InstanceId 恒 1），互斥占用同一槽位
            Room room = root.AddComponentWithId<Room>(1);
            room.Name = "Town";

            // 等待表现层订阅事件完成（加载 Game.unity；阶段 B 追加城镇瓦片视图）
            await EventSystem.Instance.PublishAsync(root, new TownSceneChangeStart() {Room = room});

            // 本地玩家（客户端权威移动的载体）
            TownPlayerComponent townPlayer = room.AddComponent<TownPlayerComponent>();
            townPlayer.Position = spawnPosition;
            townPlayer.Forward = new TSVector(1, 0, 0);

            // 表现层收尾（当前挂调试输入；阶段 B 追加角色视图/相机）
            EventSystem.Instance.Publish(root, new TownSceneInitFinish());
        }
    }
}
