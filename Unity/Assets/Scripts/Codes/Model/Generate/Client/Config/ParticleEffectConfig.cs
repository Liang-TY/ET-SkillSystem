using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class ParticleEffectConfigCategory : ConfigSingleton<ParticleEffectConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, ParticleEffectConfig> dict = new Dictionary<int, ParticleEffectConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<ParticleEffectConfig> list = new List<ParticleEffectConfig>();
		
        public void Merge(object o)
        {
            ParticleEffectConfigCategory s = o as ParticleEffectConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (ParticleEffectConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public ParticleEffectConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ParticleEffectConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ParticleEffectConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ParticleEffectConfig> GetAll()
        {
            return this.dict;
        }

        public ParticleEffectConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class ParticleEffectConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>特效资源名称</summary>
		[ProtoMember(2)]
		public string PrefabName { get; set; }
		/// <summary>存在总时长</summary>
		[ProtoMember(3)]
		public int TotalTime { get; set; }
		/// <summary>是否跟随Unit</summary>
		[ProtoMember(4)]
		public int IsFollowUnit { get; set; }
		/// <summary>初始位置x</summary>
		[ProtoMember(5)]
		public float PosX { get; set; }
		/// <summary>初始位置y</summary>
		[ProtoMember(6)]
		public float PosY { get; set; }
		/// <summary>初始位置z</summary>
		[ProtoMember(7)]
		public float PosZ { get; set; }
		/// <summary>缩放x</summary>
		[ProtoMember(8)]
		public float ScaleX { get; set; }
		/// <summary>缩放y</summary>
		[ProtoMember(9)]
		public float ScaleY { get; set; }
		/// <summary>缩放z</summary>
		[ProtoMember(10)]
		public float ScaleZ { get; set; }

	}
}
