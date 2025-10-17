using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    /// <summary>
    /// 行为类型
    /// </summary>
    public enum ActionsRunType
    {
        BuffAdd,
        BuffTick,
        BuffRemove,
        CastHit,
        BulletAwake,
        BulletDestroy,
        BulletTick
    }
}
