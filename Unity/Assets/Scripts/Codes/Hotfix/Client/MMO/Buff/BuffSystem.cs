using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class BuffAwakeSystem : AwakeSystem<Buff, int>
    {
        protected override void Awake(Buff self, int configId)
        {
            self.ConfigId = configId;
        }

    }


    public class BuffDestroySystem : DestroySystem<Buff>
    {
        protected override void Destroy(Buff self)
        {
            self.ConfigId = default;
            self.Owner = default;
            self.CreateTime = default;
            self.ExpireTime = default;
        }

    }

}
