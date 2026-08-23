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
}