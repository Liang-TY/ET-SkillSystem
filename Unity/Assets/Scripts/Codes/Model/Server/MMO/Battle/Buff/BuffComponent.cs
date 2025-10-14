using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ET.Server
{

    [ComponentOf(typeof(Unit))]
    public class BuffComponent : Entity, IAwake, IDestroy,IDeserialize,ITransfer
    {
        public Dictionary<int ,Buff> ConfigIdBuffs = new Dictionary<int ,Buff>();
    }
}
