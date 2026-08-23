namespace ET.Server
{
    /// <summary>
    /// TownScene 常驻纤程的 ActorId（EntryEvent2 启动时代码驱动创建后登记，Gate 路由查它）。
    /// 不走 StartSceneConfig（Excel 导表流程重，单进程 demo 无必要——03 文档 §2.1 遗留：未来多进程再进表）。
    /// </summary>
    public static class TownSceneHolder
    {
        [StaticField]
        public static ActorId TownActorId;
    }
}
