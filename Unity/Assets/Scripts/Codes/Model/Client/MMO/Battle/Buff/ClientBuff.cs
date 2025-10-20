using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{

    [ChildOf(typeof(ClientBuffComponent))]
    public class ClientBuff : Entity, IAwake<int>, IDestroy
    {
        public int ConfigId;
        public BuffConfig BuffConfig => BuffConfigCategory.Instance.Get(this.ConfigId);
        public Unit Owner;
        public long CreateTime;
        public long ExpireTime;
    }


}
