using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 城镇成员表（挂 TownScene 根，03 文档 §2.1）：进=加入+广播 Enter、离=移除+广播 Leave；
    /// 位置包只更新+转发不校验（MMO 模式客户端权威，服务器纯中继站）。
    /// </summary>
    [ComponentOf(typeof (Scene))]
    public class TownComponent: Entity, IAwake
    {
        /// <summary>成员：PlayerId → 最新位置/朝向（远端插值用；demo 不持久化）</summary>
        public Dictionary<long, TownPlayerInfo> Members = new();
    }
}
