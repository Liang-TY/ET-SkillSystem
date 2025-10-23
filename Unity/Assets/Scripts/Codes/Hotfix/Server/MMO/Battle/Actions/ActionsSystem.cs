using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{


    public class ActionsAwakeSystem : AwakeSystem<Actions,int>
    {
        protected override void Awake(Actions self,int configId)
        {
            self.ConfigId = configId;
        }

    }

    public class ActionsDestroySystem : DestroySystem<Actions>
    {
        protected override void Destroy(Actions self)
        {
            self.ConfigId = default;
            self.Caster = default;
            self.Owner = default;
        }

    }




}
