

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;


namespace ET.Server
{
    [Actions(ActionsType.CreateCast)]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public class Actions_CreateCast : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            RunAsync(actions, actionsRunType).Coroutine();

        }


        public async ETTask RunAsync(Actions actions, ActionsRunType actionsRunType)
        {
            Cast cast = actions.CastSelf;
            if (cast == null || actionsRunType != ActionsRunType.CastFinish)
            {
                return;
            }

            ActionsConfig config = actions.Config;
            int castConfigId = int.Parse(config.Param[0]);
            Unit unit = cast.Caster;
            await TimerComponent.Instance.WaitFrameAsync();
            if (unit.IsDisposed)
            {
                return;
            }

            unit.CreateAndCast(castConfigId);
        }
    }

}

