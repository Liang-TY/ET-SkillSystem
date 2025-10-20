using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.ClientBuff))]
    public static class BuffFactory
    {
        public static ClientBuff Create(Unit owner, BuffProto buffData)
        {
            ClientBuff buff = owner.GetComponent<ClientBuffComponent>().AddChildWithId<ClientBuff, int>(buffData.Id, buffData.ConfigId);
            buff.CreateTime = buffData.CreateTime;
            buff.ExpireTime = buffData.ExpireTime;
            buff.Owner = owner;
            return buff;
        }





    }

}
