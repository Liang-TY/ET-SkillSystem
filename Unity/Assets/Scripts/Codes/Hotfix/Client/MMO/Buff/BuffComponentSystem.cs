using ET.EventType;
using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class BuffComponentDestroySystem : DestroySystem<BuffComponent>
    {
        protected override void Destroy(BuffComponent self)
        {
            foreach (var buffsValue in self.Buffs.Values)
            {
                buffsValue?.Dispose();
            }
            self.Buffs.Clear();
        }
    }
    [FriendOfAttribute(typeof(ET.Client.BuffComponent))]
    [FriendOfAttribute(typeof(ET.Client.Buff))]
    public static class BuffComponentSystem
    {
        public static void Add(this BuffComponent self, Buff buff)
        {
            if (self.Buffs.ContainsKey(buff.Id))
            {
                return;
            }

            self.Buffs.Add(buff.Id, buff);
            buff.Owner = self.GetParent<Unit>();
        }

        public static Buff Get(this BuffComponent self, long buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out Buff buff))
            {
                return buff;
            }

            return buff;
        }

        public static void Remove(this BuffComponent self, long buffId)
        {
            Buff buff = self.Get(buffId);
            if (buff == null)
            {
                return;
            }

            self.Buffs.Remove(buffId);
            buff?.Dispose();

        }
        public static void Update(this BuffComponent self, BuffProto buffData)
        {
            Buff buff = self.Get(buffData.Id);
            if (buff == null)
            {
                return;

            }

            buff.CreateTime = buffData.CreateTime;
            buff.ExpireTime = buffData.ExpireTime;
        }


    }

}
