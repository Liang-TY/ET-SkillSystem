using System.Collections.Generic;

namespace ET.UIBuilder
{
    /// <summary>
    /// E 表事件定义（§3.6）。Params 合法值 = EUIEventParamType 枚举名。
    /// </summary>
    public class EventSpec
    {
        public string Name;

        /// <summary>false = TaskEvent（异步），true = UIEvent（同步）</summary>
        public bool Sync;

        public readonly List<string> Params = new List<string>();

        /// <summary>挂载目标节点名（必须存在于节点树）</summary>
        public string Target;

        /// <summary>Click/ClickDown/ClickUp</summary>
        public string Trigger = "Click";
    }
}
