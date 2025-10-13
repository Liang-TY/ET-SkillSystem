using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class ActionsConfigCategory : ConfigSingleton<ActionsConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, ActionsConfig> dict = new Dictionary<int, ActionsConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<ActionsConfig> list = new List<ActionsConfig>();
		
        public void Merge(object o)
        {
            ActionsConfigCategory s = o as ActionsConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (ActionsConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public ActionsConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ActionsConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ActionsConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ActionsConfig> GetAll()
        {
            return this.dict;
        }

        public ActionsConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class ActionsConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
