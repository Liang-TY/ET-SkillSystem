using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET.Server
{
    public enum NoticeClientType
    {
        /// <summary>
        /// 不通知
        /// </summary>
        NoNotice = 0,
        /// <summary>
        /// 仅通知自己
        /// </summary>
        Self = 1,
        /// <summary>
        /// 广播AOI
        /// </summary>
        Broadcast = 2,
        /// <summary>
        /// 广播AOI，除自己以外
        /// </summary>
        BroadcastNoSelf = 3,
    }
}
