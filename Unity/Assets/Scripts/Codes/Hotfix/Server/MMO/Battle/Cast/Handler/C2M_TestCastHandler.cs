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
            if (!CastConfigCategory.Instance.Contain(request.CastConfigId))
            {
                response.Error =ErrorCode.ERR_ArgsError;
                return;
            }


            if (!unit.IsAlive())
            {
                response.Error= ErrorCode.ERR_Relive_Dead_Op;
                return;
            }

            response.Error = unit.CreateAndCast(request.CastConfigId);
            await ETTask.CompletedTask;
        }

    }
}
