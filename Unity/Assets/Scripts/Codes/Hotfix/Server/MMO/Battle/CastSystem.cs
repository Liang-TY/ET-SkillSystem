using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{

    public class CastAwakeSystem : AwakeSystem<Cast,int>
    {
        protected override void Awake(Cast self,int configId)
        {
            self.ConfigId = configId;
        }
    }

    public class CastDestroySystem : DestroySystem<Cast>
    {
        protected override void Destroy(Cast self)
        {
            self.ConfigId = default;
            self.Caster = default;
        }
    }

    public static class CastSystem
    {
        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int Cast(this Cast cast)
        {
            int err = cast.CastCheck();
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }
            cast.SelectTarget();
            err = cast.CastCheckBeforeBegin();
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }
            cast.CastBeginAsync().Coroutine();
            return ErrorCode.ERR_Success;
        }

        public static int CastCheck(this Cast cast)
        {
            return ErrorCode.ERR_Success;
        }

        public static void SelectTarget(this Cast cast)
        {

        }
        public static int CastCheckBeforeBegin(this Cast cast)
        {
            return ErrorCode.ERR_Success;
        }

        public static async ETTask CastBeginAsync(this Cast cast)
        {
            await ETTask.CompletedTask;
        }
    }
}
