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
		public int Id { get; set;}
		/// <summary>技能名字</summary>
		[ProtoMember(2)]
		public string Name { get; set;}
		/// <summary>总时长</summary>
		[ProtoMember(3)]
		public int TotalTime { get; set;}
		/// <summary>冷却时间</summary>
		[ProtoMember(4)]
		public int CoolDown { get; set;}
		/// <summary>状态技能标志(只允许有一个状态技能在释放)</summary>
		[ProtoMember(5)]
		public int StatusSkill { get; set;}
		/// <summary>目标选择方式</summary>
		[ProtoMember(6)]
		public int SelectType { get; set;}
		/// <summary>目标选择参数</summary>
		[ProtoMember(7)]
		public string[] _SelectParam;
		
		[BsonIgnore]
		[ProtoIgnore]
		public string[] SelectParam
		{
		get
		{
				if(_SelectParam == null)
					_SelectParam = new string[] {};
				return _SelectParam;
			}
		}
		/// <summary>通知客户端类型</summary>
		[ProtoMember(8)]
		public int NoticeClientType { get; set;}
		/// <summary>命中行为</summary>
		[ProtoMember(9)]
		public int[] _HitAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] HitAction
		{
		get
		{
				if(_HitAction == null)
					_HitAction = new int[] {};
				return _HitAction;
			}
		}
		/// <summary>技能命中目标时间点</summary>
		[ProtoMember(10)]
		public int[] _HitActionTimes;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] HitActionTimes
		{
		get
		{
				if(_HitActionTimes == null)
					_HitActionTimes = new int[] {};
				return _HitActionTimes;
			}
		}
		/// <summary>命中自身行为</summary>
		[ProtoMember(11)]
		public int[] _SelfHitAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] SelfHitAction
		{
		get
		{
				if(_SelfHitAction == null)
					_SelfHitAction = new int[] {};
				return _SelfHitAction;
			}
		}
		/// <summary>技能命中自身时间点</summary>
		[ProtoMember(12)]
		public int[] _SelfHitActionTimes;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] SelfHitActionTimes
		{
		get
		{
				if(_SelfHitActionTimes == null)
					_SelfHitActionTimes = new int[] {};
				return _SelfHitActionTimes;
			}
		}
		/// <summary>命中Buff</summary>
		[ProtoMember(13)]
		public int[] _Buffs;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] Buffs
		{
		get
		{
				if(_Buffs == null)
					_Buffs = new int[] {};
				return _Buffs;
			}
		}
		/// <summary>命中自身Buff</summary>
		[ProtoMember(14)]
		public int[] _SelfBuffs;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] SelfBuffs
		{
		get
		{
				if(_SelfBuffs == null)
					_SelfBuffs = new int[] {};
				return _SelfBuffs;
			}
		}
		/// <summary>结束行为</summary>
		[ProtoMember(15)]
		public int[] _FinishAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] FinishAction
		{
		get
		{
				if(_FinishAction == null)
					_FinishAction = new int[] {};
				return _FinishAction;
			}
		}
		/// <summary>技能开始时的自身特效</summary>
		[ProtoMember(16)]
		public int[] _StartEffect;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] StartEffect
		{
		get
		{
				if(_StartEffect == null)
					_StartEffect = new int[] {};
				return _StartEffect;
			}
		}
		/// <summary>技能命中时的目标特效</summary>
		[ProtoMember(17)]
		public int[] _HitEffect;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] HitEffect
		{
		get
		{
				if(_HitEffect == null)
					_HitEffect = new int[] {};
				return _HitEffect;
			}
		}
		/// <summary>起手动画(motiontype)</summary>
		[ProtoMember(18)]
		public int StartAnimation { get; set;}
		/// <summary>命中动画(motiontype)</summary>
		[ProtoMember(19)]
		public int HitAnimation { get; set;}

	}
}
