using ET.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.Cast))]
    public static class CastFactory
    {
        public static Cast Create(Unit caster, long id, int configId)
        {
            Cast cast = caster.GetComponent<CastComponent>().AddChildWithId<Cast, int>(id, configId);
            cast.CasterId = caster.Id;
            return cast;
        }

    }

}
