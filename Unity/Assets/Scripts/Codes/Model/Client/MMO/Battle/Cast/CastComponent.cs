using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class CastComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long,Cast> Casts = new Dictionary<long, Cast>();
    }

}
