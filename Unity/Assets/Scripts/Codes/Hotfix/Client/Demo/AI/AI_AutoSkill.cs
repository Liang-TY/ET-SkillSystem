using Unity.Mathematics;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.AutoSkillComponent))]
    [FriendOfAttribute(typeof(ET.Client.XunLuoPathComponent))]
    public class AI_AutoSkill : AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            Scene clientScene = aiComponent.DomainScene();
            Unit myUnit = UnitHelper.GetMyUnitFromClientScene(clientScene);
            if (myUnit == null)
            {
                return 1;
            }

            AutoSkillComponent autoSkillComponent = myUnit.GetComponent<AutoSkillComponent>();
            if (TimeHelper.ServerNow() <= autoSkillComponent.NextAttackTime)
            {
                return 1;
            }
            if (FindOne(myUnit) == null)
            {
                return 1;
            }
            autoSkillComponent.NextAttackTime = TimeHelper.ServerNow() + RandomGenerator.RandomNumber(10 * 1000, 15 * 1000);
            return 0;

        }

        public override async ETTask Execute(AIComponent aiComponent, AIConfig aiConfig, ETCancellationToken cancellationToken)
        {
            Scene clientScene = aiComponent.DomainScene();
            Unit myUnit = UnitHelper.GetMyUnitFromClientScene(clientScene);
            if (myUnit == null)
            {
                return;
            }
            //选一个技能id进行释放
            int castConfigId = RandomGenerator.RandomNumber(2, 3);
            Unit target = FindOne(myUnit);
            myUnit.GetComponent<XunLuoPathComponent>().NextMoveTime = TimeHelper.ServerNow() + RandomGenerator.RandomNumber(5 * 1000, 8 * 1000);
            if (target !=null)
            {
                Session session = clientScene.GetComponent<SessionComponent>().Session;
                if (session != null)
                {
                    session.Send(new C2M_Stop() { });
                    session.Send(new C2M_TestCast() { CastConfigId = castConfigId });
                }
            }
            await ETTask.CompletedTask;
        }

        public Unit FindOne(Unit myUnit, float distance = 5.0f)
        {
            UnitComponent unitComponent = myUnit.DomainScene().GetComponent<UnitComponent>();
            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is not Unit unit)
                {
                    continue;
                }

                if (unit.Type != UnitType.Player)
                {
                    continue;
                }

                if (unit == myUnit)
                {
                    continue;
                }

                if (math.distance(unit.Position, myUnit.Position) <= distance)
                {
                    return unit;
                }

            }

            return null;
        }

    }
}