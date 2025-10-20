using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CastFinish_PlayView:AEvent<CastFinish>
    {
        protected override async ETTask Run(Scene scene, CastFinish a)
        {

            Unit caster = scene.GetComponent<UnitComponent>().Get(a.CasterId);
            if (caster == null)
            {
                return;

            }

            ClientCast cast = caster.GetComponent<ClientCastComponent>().Get(a.CastId);
            if (cast == null)
            {
                return;
            }
            caster.GetComponent<AnimatorComponent>()?.Play(MotionType.Idle);
            await ETTask.CompletedTask;
        }

    }
}
