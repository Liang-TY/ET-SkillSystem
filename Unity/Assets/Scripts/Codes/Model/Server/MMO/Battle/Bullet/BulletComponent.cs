
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class BulletComponent : Entity, IAwake<int>, IDestroy
    {
        public int ConfigId;

        [BsonIgnore]
        public BulletConfig Config
        {
            get
            {
                return BulletConfigCategory.Instance.Get(this.ConfigId);
            }
        }




        public long OwnerId;

        public long TickTimer;

    }
}
