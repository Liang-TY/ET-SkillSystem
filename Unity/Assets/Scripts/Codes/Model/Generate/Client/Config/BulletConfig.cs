using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BulletConfigCategory : ConfigSingleton<BulletConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BulletConfig> dict = new Dictionary<int, BulletConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BulletConfig> list = new List<BulletConfig>();
		
        public void Merge(object o)
        {
            BulletConfigCategory s = o as BulletConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (BulletConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public BulletConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BulletConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BulletConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BulletConfig> GetAll()
        {
            return this.dict;
        }

        public BulletConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BulletConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set;}
		/// <summary>形状</summary>
		[ProtoMember(2)]
		public int Shape { get; set;}
		/// <summary>形状参数</summary>
		[ProtoMember(3)]
		public string[] _ShapeParam;
		
		[BsonIgnore]
		[ProtoIgnore]
		public string[] ShapeParam
		{
		get
		{
				if(_ShapeParam == null)
					_ShapeParam = new string[] {};
				return _ShapeParam;
			}
		}
		/// <summary>持续时间</summary>
		[ProtoMember(4)]
		public int TotalTime { get; set;}
		/// <summary>创建时触发</summary>
		[ProtoMember(5)]
		public int[] _AwakeAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] AwakeAction
		{
		get
		{
				if(_AwakeAction == null)
					_AwakeAction = new int[] {};
				return _AwakeAction;
			}
		}
		/// <summary>结算间隔</summary>
		[ProtoMember(6)]
		public int Interval { get; set;}
		/// <summary>结算技能编号</summary>
		[ProtoMember(7)]
		public int[] _TickCastId;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] TickCastId
		{
		get
		{
				if(_TickCastId == null)
					_TickCastId = new int[] {};
				return _TickCastId;
			}
		}
		/// <summary>结算行为</summary>
		[ProtoMember(8)]
		public int[] _TickAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] TickAction
		{
		get
		{
				if(_TickAction == null)
					_TickAction = new int[] {};
				return _TickAction;
			}
		}
		/// <summary>销毁前触发</summary>
		[ProtoMember(9)]
		public int[] _DestroyAction;
		
		[BsonIgnore]
		[ProtoIgnore]
		public int[] DestroyAction
		{
		get
		{
				if(_DestroyAction == null)
					_DestroyAction = new int[] {};
				return _DestroyAction;
			}
		}
		/// <summary>模型</summary>
		[ProtoMember(10)]
		public string Model { get; set;}

	}
}
