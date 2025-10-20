using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class BuffAdd_CreateBuffView : AEvent<BuffAdd>
    {
        protected override async ETTask Run(Scene scene, BuffAdd a)
        {
            ClientBuff buff = a.Unit.GetComponent<ClientBuffComponent>().Get(a.BuffId);
            if (buff == null)
            {
                return;
            }

            BuffConfig config = buff.BuffConfig;
            foreach (var effectId in config.OwnerEffect){
                ParticleEffectHelper.CreateParticle(a.Unit, effectId);
            }
            await ETTask.CompletedTask;
        }
    }
}
