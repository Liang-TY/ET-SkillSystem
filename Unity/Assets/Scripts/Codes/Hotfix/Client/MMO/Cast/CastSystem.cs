using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class CastAwakeSystem : AwakeSystem<ClientCast, int>
    {
        protected override void Awake(ClientCast self, int configId)
        {
            self.ConfigId = configId;
        }

    }


    public class CastDestroySystem : DestroySystem<ClientCast>
    {
        protected override void Destroy(ClientCast self)
        {
            self.ConfigId = default;
            self.CasterId = default;
            self.TargetsId.Clear();
        }

    }

}
