
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [Actions(ActionsType.Damage)]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public class Actions_Damage : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Cast cast = actions.CastSelf;
            if (cast == null || actionsRunType != ActionsRunType.CastHit)
            {
                return;
            }

            if (cast.Targets.Count <= 0)
            {
                return;
            }

            UnitComponent unitComponent = actions.DomainScene().GetComponent<UnitComponent> ();
            foreach (long unitId in cast.Targets)
            {
                Unit unit = unitComponent.Get(unitId);
                if (unit == null || unit.IsDisposed)
                {
                    continue;
                }

                BattleHelper.CalcAttack(cast.Caster,unit,actions);

            }


        }
    }

}

