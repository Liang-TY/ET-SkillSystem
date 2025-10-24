using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    public static class CastHelper
    {
        //FriendOf是为了能访问cast类

        /// <summary>
        /// 创建cast技能
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="castConfigId"></param>
        /// <returns></returns>
        public static Cast Create(this Unit caster, int castConfigId)
        {

            Log.Console($"创建技能，caster信息：{caster.Config.Id} {caster.Config.Name}");
            CastComponent castComponent = caster.GetComponent<CastComponent>();
            if (castComponent == null)
            {
                return null;
            }
            Cast cast = castComponent.Create(castConfigId);
            if (cast == null)
            {
                return null;
            }
            cast.Caster = caster;
            return cast;
        }


        public static int CreateAndCast(this Unit caster, int castConfigId)
        {
            return Create(caster,castConfigId).Cast();
        }
    }
}
