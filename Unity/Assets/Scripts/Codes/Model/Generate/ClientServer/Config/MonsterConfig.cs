using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class MonsterConfigCategory : ConfigSingleton<MonsterConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, MonsterConfig> dict = new Dictionary<int, MonsterConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<MonsterConfig> list = new List<MonsterConfig>();
		
        public void Merge(object o)
        {
            MonsterConfigCategory s = o as MonsterConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (MonsterConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public MonsterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MonsterConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MonsterConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MonsterConfig> GetAll()
        {
            return this.dict;
        }

        public MonsterConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class MonsterConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set;}
		/// <summary>Unit配置Id</summary>
		[ProtoMember(2)]
		public int UnitConfigId { get; set;}
		/// <summary>组编号</summary>
		[ProtoMember(3)]
		public int GroupId { get; set;}

	}
}
