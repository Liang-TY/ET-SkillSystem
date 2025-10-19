using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class BuffComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, Buff> Buffs = new Dictionary<long, Buff>();
    }

}
