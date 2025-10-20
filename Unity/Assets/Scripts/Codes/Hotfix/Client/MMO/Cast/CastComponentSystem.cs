using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class CastComponentDestroySystem : DestroySystem<ClientCastComponent>
    {
        protected override void Destroy(ClientCastComponent self)
        {
            foreach (var castsValue in self.Casts.Values)
            {
                castsValue?.Dispose();
            }
            self.Casts.Clear();
        }
    }
    [FriendOfAttribute(typeof(ET.Client.ClientCastComponent))]
    public static class CastComponentSystem
    {
        public static void Add(this ClientCastComponent self, ClientCast cast)
        {
            if (self.Casts.ContainsKey(cast.Id))
            {
                return;
            }

            self.Casts.Add(cast.Id, cast);
        }

        public static ClientCast Get(this ClientCastComponent self, long id)
        {
            if (self.Casts.TryGetValue(id, out ClientCast cast))
            {
                return cast;
            }
            return null;
        }

        public static void Remove(this ClientCastComponent self, long id)
        {
            ClientCast cast = self.Get(id);
            if (cast != null)
            {
                self.Casts.Remove(id);
                cast?.Dispose();
            }

        }


    }

}
