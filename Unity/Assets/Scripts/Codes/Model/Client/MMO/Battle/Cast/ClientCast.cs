using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{

    [ChildOf(typeof(ClientCastComponent))]
    public class ClientCast : Entity, IAwake<int>, IDestroy
    {
        public int ConfigId;
        public CastConfig CastConfig => CastConfigCategory.Instance.Get(this.ConfigId);
        public long CasterId;
        public List<long> TargetsId = new List<long>();
    }


}
