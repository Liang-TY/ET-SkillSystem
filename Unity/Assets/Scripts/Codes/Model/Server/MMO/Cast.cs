using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    /// <summary>
    /// 投射类技能
    /// </summary>
    [ChildOf(typeof(CastComponent))]
    public class Cast: Entity,IAwake<int>,IDestroy
    {
        public int ConfigId;

        [BsonIgnore]
        public CastConfig Config
        {
            get
            {
                return CastConfigCategory.Instance.Get(this.ConfigId);
            }
        }

        /// <summary>
        /// 技能释放者
        /// </summary>
        [BsonIgnore]
        public Unit Caster;

        /// <summary>
        /// 技能受击对象
        /// </summary>
        [BsonIgnore]
        public List<long> Targets = new List<long>();

        /// <summary>
        /// 技能开始时间
        /// </summary>
        public long StartTime;
    }
}
