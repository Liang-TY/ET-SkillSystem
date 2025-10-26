using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [ChildOf(typeof(ActionsTempComponent))]
    public class Actions : Entity, IAwake<int>, IDestroy, ISerializeToEntity
    {
        public int ConfigId;



        [BsonIgnore]
        public ActionsConfig Config
        {
            get
            {
                return ActionsConfigCategory.Instance.Get(this.ConfigId);
            }
        }


        [BsonIgnore]
        public Unit Caster;


        [BsonIgnore]
        public Unit Owner;

        [BsonIgnore]
        public Cast CastSelf
        {
            get
            {
                return this.Parent.GetParent<Cast>();
            }
        }


        public Buff BuffSelf
        {
            get
            {
                return this.Parent.GetParent<Buff>();
            }
        }

        public BulletComponent BulletSelf
        {
            get
            {
                return this.Parent.GetParent<BulletComponent>();
            }
        }



    }

}


