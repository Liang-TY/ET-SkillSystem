namespace ET.Client
{
    [EntitySystemOf(typeof(LSBattleWatcherComponent))]
    [FriendOf(typeof(LSBattleWatcherComponent))]
    public static partial class LSBattleWatcherComponentSystem
    {
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
                    return;   // 还有活怪
                }
            }

            self.Reported = true;
            C2Room_BattleClear clear = C2Room_BattleClear.Create();
            self.Root().GetComponent<ClientSenderComponent>().Send(clear);
            Log.Info("[Battle] 怪物全灭，上报 BattleClear");
        }

        [EntitySystem]
        private static void Destroy(this LSBattleWatcherComponent self)
        {
        }
    }
}
