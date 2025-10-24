using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public enum SkillStatusType : byte
    {
        New = 0,
        Init = 1,
        Running = 2,
        Finish = 3
    }




    public class SkillStatusComponent : Entity, IAwake, IDestroy
    {
        public long CurSkillCastInstanceId = default;
        public long CurSkillCastID = default;
        public long CurSkillStartTime = default;
        public SkillStatusType CurSkillStatus = SkillStatusType.New;
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDowns = new Dictionary<int, long>();
    }

}
