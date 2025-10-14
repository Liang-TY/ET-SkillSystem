using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BufferConfigCategory : ConfigSingleton<BufferConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BufferConfig> dict = new Dictionary<int, BufferConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BufferConfig> list = new List<BufferConfig>();
		
        public void Merge(object o)
        {
            BufferConfigCategory s = o as BufferConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (BufferConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public BufferConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BufferConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BufferConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BufferConfig> GetAll()
        {
            return this.dict;
        }

        public BufferConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BufferConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
