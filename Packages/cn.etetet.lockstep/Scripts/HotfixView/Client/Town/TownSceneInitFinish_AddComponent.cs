namespace ET.Client
{
    /// <summary>城镇初始化收尾（表现层）：本地玩家视图（Unit2D+分层渲染）+ 调试输入（WASD/N/F9）+ 多人组件（阶段D）</summary>
    [Event(SceneType.LockStep)]
    public class TownSceneInitFinish_AddComponent: AEvent<Scene, TownSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, TownSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            await room.AddComponent<TownPlayerViewComponent>().InitAsync();
            room.AddComponent<TownOperaComponent>();
            TownRemotePlayerManagerComponent remote = room.AddComponent<TownRemotePlayerManagerComponent>();

            // 进城时服务器返回的已有成员 → 远端视图（同步开关关时也建——能看到别人，自己不发包）
            if (TownMemory.PendingMembers != null)
            {
                foreach (TownPlayerInfo member in TownMemory.PendingMembers)
                {
                    remote.CreateRemote(member.PlayerId, member.Position);
                }
                TownMemory.PendingMembers = null;
            }
        }
    }
}
