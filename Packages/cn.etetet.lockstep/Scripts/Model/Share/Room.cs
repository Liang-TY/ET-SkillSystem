using System.Collections.Generic;

namespace ET
{
    [ComponentOf]
    public class Room: Entity, IScene, IAwake, IUpdate
    {
        public Fiber Fiber { get; set; }
        public int SceneType { get; set; }
        public string Name { get; set; }

        // 本局地图Id（MapIds；0=空地）。匹配链带进（C2G_Match→...→RoomManager2Room_Init），
        // 客户端在 LSSceneChangeStart（room.Init 前）就读它懒加载瓦片
        public int MapId { get; set; }

        public long StartTime { get; set; }

        // 帧缓存
        public FrameBuffer FrameBuffer { get; set; }

        // 计算fixedTime，fixedTime在客户端是动态调整的，会做时间膨胀缩放
        public FixedTimeCounter FixedTimeCounter { get; set; }

        // 玩家id列表
        public List<long> PlayerIds { get; } = new(LSConstValue.MatchCount);
        
        // 预测帧
        public int PredictionFrame { get; set; } = -1;

        // 权威帧
        public int AuthorityFrame { get; set; } = -1;

        // 存档
        public Replay Replay { get; set; } = new();

        private EntityRef<LSWorld> lsWorld;

        // LSWorld做成child，可以有多个lsWorld，比如守望先锋有两个
        public LSWorld LSWorld
        {
            get
            {
                return this.lsWorld;
            }
            set
            {
                this.AddChild(value);
                this.lsWorld = value;
            }
        }

        public bool IsReplay { get; set; }

        public int SpeedMultiply { get; set; }

        // 战斗已结束（怪物全灭）：LSServerUpdater 据此停止帧收集/广播；RoomRoot 纤程保留等玩家走完（03 文档 §1.4）
        public bool BattleEnded { get; set; }
    }
}