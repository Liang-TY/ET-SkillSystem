using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{

    [ChildOf(typeof(BuffComponent))]
    public class Buff : Entity, IAwake<int>, IDestroy
    {
        public int ConfigId;
        public BuffConfig CastConfig => BuffConfigCategory.Instance.Get(this.ConfigId);
        public Unit Owner;
        public long CreateTime;
        public long ExpireTime;
    }


}
