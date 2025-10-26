using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET.Server
{

    [Actions(ActionsType.Damage)]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    [FriendOfAttribute(typeof(ET.Server.Actions))]
    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]
    public class Actions_MoveToTarget : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {

            Unit unit = null;
            List<long> Target = null;
            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                    Cast cast = actions.CastSelf;
                    unit = cast.Caster;
                    Target = cast.Targets;
                    break;
                case ActionsRunType.BulletTick:
                    BulletComponent bulletComponent = actions.BulletSelf;
                    unit = actions.Caster;
                    Target = bulletComponent.Target;
                    break;

                default:
                    return;

            }

            float3 newPos = float3.zero;
            ActionsConfig config = actions.Config;
            int dir = int.Parse(config.Param[0]);
            Unit tar = null;
            if (Target.Count > 0)
            {
                UnitComponent unitComponent = actions.DomainScene().GetComponent<UnitComponent>();
                Unit u = unitComponent.Get(Target[0]);
                if (u != null && !u.IsDisposed && u != unit)
                {
                    tar = u;
                }

            }

            if (tar != null)
            {
                newPos = unit.Position + math.normalize(tar.Position - unit.Position) * dir;
            }

            else
            {
                newPos = unit.Position + math.normalize(unit.Forward) * dir;
            }

            newPos.y = unit.Position.y;
            unit.FindPathMoveToAsync(newPos).Coroutine();
            Log.Console($"unit {unit.Id} 向目标移动{dir}米 newPos:{newPos}");
        }

    }
}
