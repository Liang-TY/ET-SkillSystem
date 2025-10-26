

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;


namespace ET.Server
{
    [Actions(ActionsType.HitFlyTarget)]
    [FriendOfAttribute(typeof(ET.Server.Buff))]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public class Actions_HitFlyTarget : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit caster = null;
            switch (actionsRunType)
            {
                case ActionsRunType.BuffTick:
                    caster = actions.BuffSelf.Owner;
                    break;
                case ActionsRunType.CastHit:
                    caster = actions.CastSelf.Caster;
                    break;
            }

            ActionsConfig config = actions.Config;
            float range = float.Parse(config.Param[0]);
            float dir = float.Parse(config.Param[1]);
            int buffId = int.Parse(config.Param[2]);

            foreach (var aoiEntity in caster.GetBeSeeUnits().Values)
            {
                Unit unit = aoiEntity.GetParent<Unit>();
                if (unit.Type != UnitType.Player && unit.Type != UnitType.Monster)
                {
                    continue;
                }

                if (unit == caster)
                {
                    continue;
                }

                if (math.length(unit.Position - caster.Position) < range)
                {
                    float3 unitPos = new float3(unit.Position.x, 0, unit.Position.z);
                    float3 casterPos = new float3(caster.Position.x, 0, caster.Position.z);
                    float3 targetDir = math.normalize(unitPos - casterPos);
                    float3 forwardDir = caster.Forward;
                    forwardDir.y = 0.0f;
                    float3 newPos = unitPos + (forwardDir * dir);
                    unit.FindPathMoveToAsync(newPos).Coroutine();
                    if (buffId != 0)
                    {
                        unit.GetComponent<BuffComponent>()?.CreateAndAdd(buffId);
                    }

                }


            }
        }


    }

}

