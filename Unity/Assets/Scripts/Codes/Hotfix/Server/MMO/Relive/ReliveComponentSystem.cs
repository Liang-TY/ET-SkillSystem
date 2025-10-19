using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class ReliveComponentAwakeSystem : AwakeSystem<ReliveComponent>
    {
        protected override void Awake(ReliveComponent self)
        {
            self.Alive = true;
        }

    }


    public class ReliveComponentDestroySystem : DestroySystem<ReliveComponent>
    {
        protected override void Destroy(ReliveComponent self)
        {
            self.Alive = default;
        }

    }


    public static class ReliveComponentSystem
    {

    }




}
