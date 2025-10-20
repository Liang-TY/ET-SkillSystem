using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class ClientCastComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long,ClientCast> Casts = new Dictionary<long, ClientCast>();
    }

}
