using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class Bullet2ConfigCategory : ConfigSingleton<Bullet2ConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Bullet2Config> dict = new Dictionary<int, Bullet2Config>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Bullet2Config> list = new List<Bullet2Config>();
		
        public void Merge(object o)
        {
            Bullet2ConfigCategory s = o as Bullet2ConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (Bullet2Config config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public Bullet2Config Get(int id)
        {
            this.dict.TryGetValue(id, out Bullet2Config item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Bullet2Config)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Bullet2Config> GetAll()
        {
            return this.dict;
        }

        public Bullet2Config GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Bullet2Config: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
