using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{



    [ActorMessageHandler(SceneType.Map)]
    public class C2M_TestCastHandler :AMActorLocationRpcHandler<Unit, C2M_TestCast, M2C_TestCast>
    {
        protected override async ETTask Run(Unit unit, C2M_TestCast request, M2C_TestCast response)
        {
            int err = BattleHelper.CanCastSkill(unit, request.CastConfigId);
            if (err != ErrorCode.ERR_Success)
            {
                response.Error = err;
                return;
            }
            response.Error = unit.CreateAndCast(request.CastConfigId);
            await ETTask.CompletedTask;
        }

    }
}
