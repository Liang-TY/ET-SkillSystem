namespace ET.Server
{
    // 进入视野通知
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_NotifyClient: AEvent<EventType.UnitEnterSightRange>
    {
        protected override async ETTask Run(Scene scene, EventType.UnitEnterSightRange args)
        {
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Id == b.Id)
            {
                return;
            }

            Unit ua = a.GetParent<Unit>();
            MonsterFlag monsterFlag = ua.GetComponent<MonsterFlag>();
            if (monsterFlag != null)
            {
                Log.Console($"玩家进入怪物视野，不向客户端发起通知");
                return;
            }
            if (ua.Type != UnitType.Player)
            {
                return;
            }

            Unit ub = b.GetParent<Unit>();
            if (ua == null || ub == null)
            {
                return;
            }


            MessageHelper.NoticeUnitAdd(ua, ub);
            
            await ETTask.CompletedTask;
        }
    }
}