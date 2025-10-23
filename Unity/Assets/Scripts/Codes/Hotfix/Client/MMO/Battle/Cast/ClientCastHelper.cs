using ET;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public static class ClientCastHelper
    {
        public static async ETTask<int> CastSkill(Scene zoneScene, int castConfigId)
        {

            M2C_TestCast m2cTestCast = (M2C_TestCast) await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2M_TestCast()
            {
                CastConfigId = castConfigId
            });
            return m2cTestCast.Error;
        }

    }

}
