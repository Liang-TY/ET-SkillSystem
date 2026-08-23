namespace ET.Client
{
    [EntitySystemOf(typeof(LSBattleWatcherComponent))]
    [FriendOf(typeof(LSBattleWatcherComponent))]
    public static partial class LSBattleWatcherComponentSystem
    {
        /// <summary>全灭后战斗继续跑的时长（收场动画/走动），到点才上报停帧（03 文档 §1.4 用户定案）</summary>
        private const int ClearDelayMs = 3000;

        [EntitySystem]
        private static void Awake(this LSBattleWatcherComponent self)
        {
            MapDefinition mapDef = MapLoader.Get(self.GetParent<Room>()?.MapId ?? 0);
            self.HasMonsters = mapDef?.MonsterAiIds is { Length: > 0 };
        }

        [EntitySystem]
        private static void Update(this LSBattleWatcherComponent self)
        {
            if (!self.HasMonsters || self.Reported) return;

            Room room = self.GetParent<Room>();
            LSUnitComponent unitComponent = room.LSWorld?.GetComponent<LSUnitComponent>();
            if (unitComponent == null) return;

            foreach (var kv in unitComponent.Children)
            {
                if (kv.Value is LSUnit unit && unit.GetComponent<LSMonsterAIComponent>() != null)
                {
                    return;   // 还有活怪（死了不会复活，倒计时不用重置）
                }
            }

            // 全灭：起 3 秒倒计时（战斗照常跑，帧不停），到点上报
            long now = TimeInfo.Instance.ClientNow();
            if (self.ReportAtTime == 0)
            {
                self.ReportAtTime = now + ClearDelayMs;
                Log.Info($"[Battle] 怪物全灭，{ClearDelayMs}ms 后上报 BattleClear（战斗继续）");
                return;
            }
            if (now < self.ReportAtTime) return;

            self.Reported = true;
            C2Room_BattleClear clear = C2Room_BattleClear.Create();
            self.Root().GetComponent<ClientSenderComponent>().Send(clear);
            Log.Info("[Battle] 上报 BattleClear");
        }

        [EntitySystem]
        private static void Destroy(this LSBattleWatcherComponent self)
        {
        }
    }
}
