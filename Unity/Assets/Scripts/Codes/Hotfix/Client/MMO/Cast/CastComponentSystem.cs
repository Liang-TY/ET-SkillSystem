using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    public class CastComponentDestroySystem : DestroySystem<CastComponent>
    {
        protected override void Destroy(CastComponent self)
        {
            foreach (var castsValue in self.Casts.Values)
            {
                castsValue?.Dispose();
            }
            self.Casts.Clear();
        }
    }
    [FriendOfAttribute(typeof(ET.Client.CastComponent))]
    public static class CastComponentSystem
    {
        public static void Add(this CastComponent self, Cast cast)
        {
            if (self.Casts.ContainsKey(cast.Id))
            {
                return;
            }

            self.Casts.Add(cast.Id, cast);
        }

        public static Cast Get(this CastComponent self, long id)
        {
            if (self.Casts.TryGetValue(id, out Cast cast))
            {
                return cast;
            }
            return null;
        }

        public static void Remove(this CastComponent self, long id)
        {
            Cast cast = self.Get(id);
            if (cast != null)
            {
                self.Casts.Remove(id);
                cast?.Dispose();
            }

        }


    }

}
