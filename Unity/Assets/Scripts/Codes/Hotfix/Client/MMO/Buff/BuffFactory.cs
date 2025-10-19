using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.Buff))]
    public static class BuffFactory
    {
        public static Buff Create(Unit owner, BuffProto buffData)
        {
            Buff buff = owner.GetComponent<BuffComponent>().AddChildWithId<Buff, int>(buffData.Id, buffData.ConfigId);
            buff.CreateTime = buffData.CreateTime;
            buff.ExpireTime = buffData.ExpireTime;
            buff.Owner = owner;
            return buff;
        }





    }

}
