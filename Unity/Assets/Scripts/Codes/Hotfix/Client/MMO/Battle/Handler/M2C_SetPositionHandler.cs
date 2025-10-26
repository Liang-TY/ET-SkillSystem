using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class M2C_SetPositionHandler : AMHandler<M2C_SetPosition>
    {

        protected override async ETTask Run(Session session, M2C_SetPosition message)
        {

            UnitComponent unitComponent = session.ClientScene()?.CurrentScene().GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit unit = unitComponent.Get(message.UnitId);
            if (unit == null)
            {
                return;
            }

            unit.Position = message.Position;
            unit.Rotation = message.Rotation;

            await ETTask.CompletedTask;
        }

    }


}
