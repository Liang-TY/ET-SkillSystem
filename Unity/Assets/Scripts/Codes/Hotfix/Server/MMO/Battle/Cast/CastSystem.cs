using MongoDB.Driver.Core.Clusters;
using NLog;
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
            self.AddComponent<ActionsTempComponent>();
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

            CastConfig config = cast.Config;
            if (config.Times.Count <= 0)
            {
                return;
            }

            long castInstanceId = 0;
            long casterInstanceId = 0;

            foreach (int time in config.Times)
            {
                castInstanceId = cast.InstanceId;
                casterInstanceId = cast.Caster.InstanceId;
                await TimerComponent.Instance.WaitTillAsync(cast.StartTime + time);

                if (!cast.checkAsyncInvalid(castInstanceId,casterInstanceId))
                {
                    Log.Error($"Cast asyncInvalid {castInstanceId} {casterInstanceId}");
                }


                //TODO 创建出一系列技能行为
                foreach (CastActionTimes castActionTimes in config.TimesDict[time])
                {
                    if (castActionTimes.IsSelfHit)
                    {
                        cast.HandleSelfHit(castActionTimes.Index);
                    }
                    else
                    {
                        cast.HandleTargetHit(castActionTimes.Index);
                    }
                }



            }



            if (config.TotalTime > 0)
            {
                castInstanceId = cast.InstanceId;
                casterInstanceId = cast.Caster.InstanceId;
                await TimerComponent.Instance.WaitTillAsync(cast.StartTime + config.TotalTime);
                if (!cast.checkAsyncInvalid(castInstanceId, casterInstanceId))
                {
                    Log.Error($"Cast asyncInvalid {castInstanceId} {casterInstanceId}");
                    return;
                }
            }

            CastFinish(cast);
        }


        public static void HandleSelfHit(this Cast cast, int index)
        {
            CastConfig config = cast.Config;
            cast.SelectTarget();
            if (cast.Targets.Count <= 0)
            {
                return;
            }
            M2C_CastHit m2CCastHit = new M2C_CastHit() { CastId = cast.Id, CasterId = cast.Caster.Id, TargetsId = new List<long>() };
            m2CCastHit.TargetsId.AddRange(cast.Targets);
            MMOMessageHelper.SendClient(cast.Caster, m2CCastHit,(NoticeClientType)cast.Config.NoticeClientType);



            if (config.SelfHitAction.Length > index)
            {
                int actionId = config.SelfHitAction[index];
                cast.CreateActions(actionId, cast.Caster, ActionsRunType.CastHit);
            }

        }

        public static void HandleTargetHit(this Cast cast, int index)
        {

            CastConfig config = cast.Config;
            cast.SelectTarget();
            if (cast.Targets.Count <= 0)
            {
                return;
            }
            M2C_CastHit m2CCastHit = new M2C_CastHit() { CastId = cast.Id, CasterId = cast.Caster.Id, TargetsId = new List<long>() };
            m2CCastHit.TargetsId.AddRange(cast.Targets);
            MMOMessageHelper.SendClient(cast.Caster, m2CCastHit, (NoticeClientType)cast.Config.NoticeClientType);

            UnitComponent unitComponent = cast.DomainScene().GetComponent<UnitComponent>();

            foreach (long unitId in cast.Targets)
            {
                Unit unit = unitComponent.Get(unitId);
                if (unit == null || unit.IsDisposed)
                {
                    continue;
                }
                if (config.HitAction.Length > index)
                {
                    int actionId = config.HitAction[index];
                    cast.CreateActions(actionId,unit,ActionsRunType.CastHit);
                }
            }
        }

        public static void CastFinish(this Cast cast)
        {

            //没有持续时间，就是瞬发的技能流程，可以不用通知结束，客户端自行结束
            if (cast.Config.TotalTime >0)
            {
                M2C_CastFinish m2cCastFinish = new M2C_CastFinish(){ CastId = cast.Id, CasterId = cast.Caster.Id };
                MMOMessageHelper.SendClient(cast.Caster, m2cCastFinish, (NoticeClientType)cast.Config.NoticeClientType);
                cast?.Dispose();
            }



        }

    //检测技能异步结束后是否仍合法
        public static bool checkAsyncInvalid(this Cast cast, long castInstanceId, long casterInstanceId)
        {
            if (cast.Caster == null)
            {
                return false;

            }
            if (cast.InstanceId != castInstanceId || cast.Caster.InstanceId != casterInstanceId)
            {
                return false;
            }
            return true;
        }
    }
}
