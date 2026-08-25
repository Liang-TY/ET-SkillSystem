namespace ET.Client
{
    public struct LSSceneChangeStart
    {
        public Room Room;
    }
    
    public struct LSSceneInitFinish
    {
    }
    
    public struct AfterCreateClientScene
    {
    }
    
    public struct AfterCreateCurrentScene
    {
    }

    public struct AppStartInitFinish
    {
    }

    public struct EnterMapFinish
    {

    }

    // ---------- 城镇（03 文档 §2；照抄 LS 版三件套：struct+发布协程+订阅者）----------

    public struct TownSceneChangeStart
    {
        public Room Room;
    }

    public struct TownSceneInitFinish
    {
    }

    // ---------- DemoUI 流程（战斗 UI）----------

    /// <summary>怪物全灭（3 秒收场倒计时开始；View 层订阅显示 BattleTip）</summary>
    public struct MonsterAllDead
    {
    }

    /// <summary>战斗结束返回城镇（View 层显示回城 Loading）</summary>
    public struct ReturnTown
    {
    }
}