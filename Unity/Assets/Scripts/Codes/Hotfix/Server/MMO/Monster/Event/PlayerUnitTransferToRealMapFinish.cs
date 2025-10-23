
using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [Event(SceneType.Map)]
    public class PlayerUnitTransferToRealMapFinish : AEvent<PlayerUnitTransferToRealMap>
    {
        protected override async ETTask Run(Scene scene, PlayerUnitTransferToRealMap a)
        {

            await ETTask.CompletedTask;
        }

    }


}
