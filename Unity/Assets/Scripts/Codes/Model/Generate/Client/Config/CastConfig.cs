using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class CastConfigCategory : ConfigSingleton<CastConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, CastConfig> dict = new Dictionary<int, CastConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<CastConfig> list = new List<CastConfig>();
		
        public void Merge(object o)
        {
            CastConfigCategory s = o as CastConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (CastConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public CastConfig Get(int id)
        {
            this.dict.TryGetValue(id, out CastConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (CastConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, CastConfig> GetAll()
        {
            return this.dict;
        }

        public CastConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class CastConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Type</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>名字</summary>
		[ProtoMember(3)]
		public string Name { get; set; }
		/// <summary>位置</summary>
		[ProtoMember(4)]
		public int Position { get; set; }
		/// <summary>身高</summary>
		[ProtoMember(5)]
		public int Height { get; set; }

	}
}
