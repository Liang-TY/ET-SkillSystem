using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{

    public class CastComponentAwakeSystem:AwakeSystem<CastComponent>
    {
        protected override void Awake(CastComponent self)
        {

        }
    }

    public class CastComponentDestroySystem : DestroySystem<CastComponent>
    {
        protected override void Destroy(CastComponent self)
        {

        }
    }

    public static class CastComponentSystem
    {
        public static Cast Create(this CastComponent self,int configId) 
        {
            return self.AddChild<Cast, int>(configId);
        }



    }
}
