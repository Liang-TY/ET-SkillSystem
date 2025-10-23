using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class M2C_BattleResultHandler : AMHandler<M2C_BattleResult>
    {

        protected override async ETTask Run(Session session, M2C_BattleResult message)
        {
            await ETTask.CompletedTask;
        }

    }


}
