using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CastStart_PlayView:AEvent<CastStart>
    {
        protected override async ETTask Run(Scene scene, CastStart a)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(a.CasterId);
            if (unit == null)
            {
                return;
            }

            unit.GetComponent<AnimatorComponent>()?.Play(MotionType.Attack);
            CastConfig castConfig = CastConfigCategory.Instance.Get((int)a.CastConfigId);
            foreach (var effectID in castConfig.StartEffect)
            {
                ParticleEffectHelper.CreateParticle(unit, effectID);
            }

            await ETTask.CompletedTask;
        }

    }
}
