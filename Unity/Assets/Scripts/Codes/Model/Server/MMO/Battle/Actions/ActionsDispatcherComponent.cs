using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ActionsDispatcheerComponent : Entity, IAwake, IDestroy, ILoad
    {
        [StaticField]
        public static ActionsDispatcheerComponent Instance;
        public Dictionary<int, IActions> Dict = new Dictionary<int, IActions>();
    }

}
