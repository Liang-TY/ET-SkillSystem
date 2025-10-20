using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class ClientBuffComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, ClientBuff> Buffs = new Dictionary<long, ClientBuff>();
    }

}
