using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{


    [ChildOf(typeof(MonsterMapComponent))]
    public class MonsterMapInfo : Entity, IAwake<int>
    {
        public int monsterId;
    }



    [ComponentOf(typeof(Scene))]
    public class MonsterMapComponent:Entity,IAwake
    {

    }






}
