using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

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
    [FriendOfAttribute(typeof(ET.Server.Cast))]
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
            if (cast == null || cast.IsDisposed == true)
            {
                return ErrorCode.ERR_Cast_ArgsError;
            }
            if (cast.Caster == null || cast.Caster.IsDisposed == true)
            {
                return ErrorCode.ERR_Cast_CasterIsNull;
            }
            return ErrorCode.ERR_Success;
        }

        public static void SelectTarget(this Cast cast)
        {
            Unit caster = cast.Caster;
            CastConfig castConfig = cast.Config;
            int rang = 0;
            switch (castConfig.SelectType)
            {
                // 选择身边范围内的一个人，不选自己
                case 1:
                    rang = int.Parse(castConfig.SelectParam[0]);
                    foreach (AOIEntity aoiEntity in caster.GetBeSeePlayers().Values)
                    {
                        Unit unit = aoiEntity.GetParent<Unit>();
                        if(unit == caster){
                            //不选择自己
                            continue;
                        }
                        if (math.length(unit.Position - caster.Position)< rang)
                        {
                            cast.Targets.Add(unit.Id);
                            break;
                        }
                    }

                    break;
                // 选择身边范围内的所有人
                case 2:
                    rang = int.Parse(castConfig.SelectParam[0]);
                    foreach (AOIEntity aoiEntity in caster.GetBeSeePlayers().Values)
                    {
                        Unit unit = aoiEntity.GetParent<Unit>();
                        if (math.length(unit.Position - caster.Position) < rang)
                        {
                            cast.Targets.Add(unit.Id);
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }

        }
        public static int CastCheckBeforeBegin(this Cast cast)
        {
            switch (cast.Config.SelectType)
            {
                case 1:
                    break;
                case 2:
                    if (cast.Targets.Count <= 0)
                    {
                        return ErrorCode.ERR_Cast_TargetIsNull;
                    }
                    break;
                default : 
                    break;
            }
            return ErrorCode.ERR_Success;
        }

        public static async ETTask CastBeginAsync(this Cast cast)
        {
            cast.StartTime = TimeHelper.ServerNow();
            M2C_CastStart m2C_CastStart = new M2C_CastStart()
            {
                CastId = cast.Id,
                CasterId = cast.Caster.Id,
                CastConfigId = cast.ConfigId,
                TargetsId = new List<long>(),
            };
            m2C_CastStart.TargetsId.AddRange(cast.Targets);
            MMOMessageHelper.SendClient(cast.Caster,m2C_CastStart,(NoticeClientType)cast.Config.NoticeClientType);

            CastConfig castConfig = cast.Config;
            if (castConfig.Times.Count <= 0)
            {
                return;
            }

            long castInstanceId = 0;
            long casterInstanceId = 0;

            foreach (int time in castConfig.Times)
            {
                await TimerComponent.Instance.WaitTillAsync(cast.StartTime + time);

                //TODO 创建技能行为
            }
            await ETTask.CompletedTask;
        }
    }
}
