using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CastHit_PlayView:AEvent<CastHit>
    {
        protected override async ETTask Run(Scene scene, CastHit a)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(a.TargetId);
            if (unit == null)
            {
                return;
            }

            Unit caster = scene.GetComponent<UnitComponent>().Get(a.CasterId);
            if (caster == null)
            {
                return;
            }

            ClientCast cast = caster.GetComponent<ClientCastComponent>().Get(a.CastId);

            if (cast ==null)
            {
                return;
            }
            CastConfig castConfig = cast.CastConfig;
            unit.GetComponent<AnimatorComponent>()?.Play((MotionType)castConfig.HitAnimation);
            foreach (var effectId in castConfig.HitEffect)
            {
                ParticleEffectHelper.CreateParticle(unit, effectId);
            }




            await ETTask.CompletedTask;
        }

    }
}
