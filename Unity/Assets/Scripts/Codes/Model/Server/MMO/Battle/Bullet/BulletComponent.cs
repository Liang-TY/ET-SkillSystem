
using MongoDB.Bson.Serialization.Attributes;
using NLog.Fluent;
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


        public int TickCount = default;
        public List<long> Target = new List<long>();
        public long OwnerId = default;
        public long TickTimer = default;
        public long TickTimer2 = default;

        public long TickTimer3 = default;
        public long TotalTimer = default;

    }
}
