using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class CastAwakeSystem : AwakeSystem<Cast, int>
    {
        protected override void Awake(Cast self, int configId)
        {
            self.ConfigId = configId;
        }

    }


    public class CastDestroySystem : DestroySystem<Cast>
    {
        protected override void Destroy(Cast self)
        {
            self.ConfigId = default;
            self.CasterId = default;
            self.TargetsId.Clear();
        }

    }

}
