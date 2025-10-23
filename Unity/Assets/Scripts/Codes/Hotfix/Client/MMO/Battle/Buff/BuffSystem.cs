using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class BuffAwakeSystem : AwakeSystem<ClientBuff, int>
    {
        protected override void Awake(ClientBuff self, int configId)
        {
            self.ConfigId = configId;
        }

    }


    public class BuffDestroySystem : DestroySystem<ClientBuff>
    {
        protected override void Destroy(ClientBuff self)
        {
            self.ConfigId = default;
            self.Owner = default;
            self.CreateTime = default;
            self.ExpireTime = default;
        }

    }

}
