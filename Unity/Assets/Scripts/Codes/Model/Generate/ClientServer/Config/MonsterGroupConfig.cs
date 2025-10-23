using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class MonsterGroupConfigCategory : ConfigSingleton<MonsterGroupConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, MonsterGroupConfig> dict = new Dictionary<int, MonsterGroupConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<MonsterGroupConfig> list = new List<MonsterGroupConfig>();
		
        public void Merge(object o)
        {
            MonsterGroupConfigCategory s = o as MonsterGroupConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (MonsterGroupConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public MonsterGroupConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MonsterGroupConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MonsterGroupConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MonsterGroupConfig> GetAll()
        {
            return this.dict;
        }

        public MonsterGroupConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class MonsterGroupConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set;}
		/// <summary>位置x</summary>
		[ProtoMember(2)]
		public float PosX { get; set;}
		/// <summary>位置y</summary>
		[ProtoMember(3)]
		public float PosY { get; set;}
		/// <summary>位置z</summary>
		[ProtoMember(4)]
		public float PosZ { get; set;}
		/// <summary>范围</summary>
		[ProtoMember(5)]
		public int Range { get; set;}

	}
}
