using ET.EventType;
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class BuffComponentDestroySystem : DestroySystem<ClientBuffComponent>
    {
        protected override void Destroy(ClientBuffComponent self)
        {
            foreach (var buffsValue in self.Buffs.Values)
            {
                buffsValue?.Dispose();
            }
            self.Buffs.Clear();
        }
    }
    [FriendOfAttribute(typeof(ET.Client.ClientBuffComponent))]
    [FriendOfAttribute(typeof(ET.Client.ClientBuff))]
    public static class BuffComponentSystem
    {
        public static void Add(this ClientBuffComponent self, ClientBuff buff)
        {
            if (self.Buffs.ContainsKey(buff.Id))
            {
                return;
            }

            self.Buffs.Add(buff.Id, buff);
            buff.Owner = self.GetParent<Unit>();
        }

        public static ClientBuff Get(this ClientBuffComponent self, long buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out ClientBuff buff))
            {
                return buff;
            }

            return buff;
        }

        public static void Remove(this ClientBuffComponent self, long buffId)
        {
            ClientBuff buff = self.Get(buffId);
            if (buff == null)
            {
                return;
            }

            self.Buffs.Remove(buffId);
            buff?.Dispose();

        }
        public static void Update(this ClientBuffComponent self, BuffProto buffData)
        {
            ClientBuff buff = self.Get(buffData.Id);
            if (buff == null)
            {
                return;

            }

            buff.CreateTime = buffData.CreateTime;
            buff.ExpireTime = buffData.ExpireTime;
        }


    }

}
