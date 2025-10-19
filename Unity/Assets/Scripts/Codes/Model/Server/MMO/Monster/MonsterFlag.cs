using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{

    [ComponentOf(typeof(Unit))]
    public class MonsterFlag : Entity, IAwake<int,int>,IDestroy
    {
        public int ConfigId;
        public int GroupConfigId;
    }

}
