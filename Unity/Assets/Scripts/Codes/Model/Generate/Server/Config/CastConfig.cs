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
		/// <summary>总时长</summary>
		[ProtoMember(2)]
		public int TotalTime { get; set; }
		/// <summary>目标选择方式</summary>
		[ProtoMember(3)]
		public int SelectType { get; set; }
		/// <summary>目标选择参数</summary>
		[ProtoMember(4)]
		public string[] SelectParam { get; set; }
		/// <summary>通知客户端类型</summary>
		[ProtoMember(5)]
		public int NoticeClientType { get; set; }
		/// <summary>命中行为</summary>
		[ProtoMember(6)]
		public int[] HitAction { get; set; }
		/// <summary>技能命中目标时间点</summary>
		[ProtoMember(7)]
		public int[] HitActionTimes { get; set; }
		/// <summary>命中自身行为</summary>
		[ProtoMember(8)]
		public int[] SelfHitAction { get; set; }
		/// <summary>技能命中自身时间点</summary>
		[ProtoMember(9)]
		public int[] SelfHitActionTimes { get; set; }
		/// <summary>命中Buff</summary>
		[ProtoMember(10)]
		public int[] Buffs { get; set; }
		/// <summary>命中自身Buff</summary>
		[ProtoMember(11)]
		public int[] SelfBuffs { get; set; }
		/// <summary>技能开始时的自身特效</summary>
		[ProtoMember(12)]
		public int[] StartEffect { get; set; }
		/// <summary>技能命中时的目标特效</summary>
		[ProtoMember(13)]
		public int[] HitEffect { get; set; }

	}
}
