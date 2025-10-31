using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class AutoSkillComponent:Entity,IAwake,IDestroy
    {
        public long NextAttackTime;
    }
}
