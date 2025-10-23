using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    /// <summary>
    /// 释放技能的组件，角色调用此组件释放技能
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class CastComponent : Entity, IAwake, IDestroy,ITransfer
    {
        //public int ConfigId;

        //[BsonIgnore]
        //public CastConfig Config
        //{
        //    get
        //    {
        //        return CastConfigCategory.Instance.Get(this.ConfigId);
        //    }
        //}


        //[BsonIgnore]
        //public Unit Caster;//技能释放者

        //[BsonIgnore]
        //public List<long> Targets = new List<long>();//技能受击对象

        //public long StartTime;//技能开始时间
    }
}
