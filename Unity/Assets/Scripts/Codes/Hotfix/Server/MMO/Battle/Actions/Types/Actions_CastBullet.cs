
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [Actions(ActionsType.CastBullet)]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public class Actions_CastBullet : IActions
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

            ActionsConfig config = actions.Config;
            UnitComponent unitComponent = actions.DomainScene().GetComponent<UnitComponent>();
            foreach (long uid in cast.Targets)
            {
                Unit unit = unitComponent.Get(uid);
                if (unit == null)
                {
                    continue;
                }
                int unitId = int.Parse(config.Param[0]);
                int bulletId = int.Parse(config.Param[1]);
                Unit bullet = UnitFactory.CreateBullet(cast.DomainScene(), cast.Caster.Id, unitId, bulletId, unit.Position);
                bullet.GetComponent<BulletComponent>().Start();
            }


        }
    }

}

