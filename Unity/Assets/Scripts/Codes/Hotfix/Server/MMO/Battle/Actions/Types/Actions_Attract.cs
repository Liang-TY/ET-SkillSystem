

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;


namespace ET.Server
{
    [Actions(ActionsType.Attract)]
    [FriendOfAttribute(typeof(ET.Server.Actions))]
    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]
    public class Actions_Attract : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            if (actionsRunType != ActionsRunType.BulletTick)
            {
                return;
            }

            Unit unit = actions.Caster;
            List<long> Target = actions.BulletSelf.Target;
            if (Target.Count <= 0)
            {
                return;
            }

            ActionsConfig config = actions.Config;
            float f = float.Parse(config.Param[0]);
            UnitComponent unitComponent = actions.DomainScene().GetComponent<UnitComponent>();
            foreach (var uid in Target)
            {
                Unit u = unitComponent.Get(uid);
                if (u == null || u.IsDisposed){
                    continue;
                }
                if (math.distance(u.Position, unit.Position) < 0.3f)
                {
                    continue;
                }

                float3 newPos = u.Position + math.normalize(unit.Position - u.Position) * f;
                u.ForceSetPosition(newPos, true);
                Log.Console($"吸引目标 {u.Id} 新位置:{newPos}");
            }


        }


    }

}

