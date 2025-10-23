using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{


    [ChildOf(typeof(MonsterMapComponent))]
    public class CreateMonsterInfo : Entity, IAwake<int>
    {
        public int monsterId;
    }



    [ComponentOf(typeof(Scene))]
    public class MonsterMapComponent:Entity,IAwake
    {
        /// <summary>
        /// 是否允许创建怪物
        /// </summary>
        [BsonIgnore]
        public bool canCreateMonster;
    }






}
