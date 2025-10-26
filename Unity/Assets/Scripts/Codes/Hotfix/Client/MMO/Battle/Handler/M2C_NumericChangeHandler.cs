
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class M2C_NumericChangeHandler : AMHandler<M2C_NumericChange>
    {

        protected override async ETTask Run(Session session, M2C_NumericChange message)
        {
            UnitComponent unitComponent = session.ClientScene().CurrentScene().GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit unit = unitComponent.Get(message.UnitId);
            if (unit == null)
            {
                return;
            }




            await ETTask.CompletedTask;
        }

    }


}
