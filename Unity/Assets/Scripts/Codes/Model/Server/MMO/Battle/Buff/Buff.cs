using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [ChildOf(typeof(BuffComponent))]
    public class Buff : Entity, IAwake<int>, IDestroy, ISerializeToEntity
    {
        public int ConfigId;
        [BsonIgnore]
        public BuffConfig Config
        {
            get
            {
                return BuffConfigCategory.Instance.Get(this.ConfigId);
            }
        }

        [BsonIgnore]
        public Unit Owner;


        public long CreateTime;


        public int TickTime;

        public int TickBeginTime;

        [BsonIgnore]
        public long TickTimer;


        [BsonIgnore]
        public long WaitTickTimer;


        public long ExpireTime;


        [BsonIgnore]
        public long ExpireTimer;









    }
}
