using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.ClientCast))]
    public static class CastFactory
    {
        public static ClientCast Create(Unit caster, long id, int configId)
        {
            ClientCast cast = caster.GetComponent<ClientCastComponent>().AddChildWithId<ClientCast, int>(id, configId);
            cast.CasterId = caster.Id;
            return cast;
        }

    }

}
