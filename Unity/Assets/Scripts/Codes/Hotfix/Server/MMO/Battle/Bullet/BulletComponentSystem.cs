
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET.Server
{


    public class BulletComponentAwakeSystem : AwakeSystem<BulletComponent, int>
    {
        protected override void Awake(BulletComponent self, int ConfigId)
        {
            self.ConfigId = ConfigId;
            self.AddComponent<ActionsTempComponent>();
        }
    }


    public class BulletComponentDestroySystem : DestroySystem<BulletComponent>
    {
        protected override void Destroy(BulletComponent self)
        {
            //self.PreDestroy();
            //self.ConfigId = default;

            self.ConfigId = default;
            self.TickCount = default;
            self.OwnerId = default;
            TimerComponent.Instance.Remove(ref self.TickTimer);
            TimerComponent.Instance.Remove(ref self.TickTimer2);
            TimerComponent.Instance.Remove(ref self.TickTimer3);
            TimerComponent.Instance.Remove(ref self.TotalTimer);
        }
    }


    [Invoke(TimerInvokeType.BulletTick)]
    public class BulletTick_TimerHandler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent t)
        {
            t.Tick();
        }
    }



    [Invoke(TimerInvokeType.BulletTick2)]
    public class BulletTick1_TimerHandler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent t)
        {
            t.Tick1();
        }
    }

    [Invoke(TimerInvokeType.BulletTick3)]
    public class BulletTick2_TimerHandler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent t)
        {
            t.Tick2();
        }
    }

    [Invoke(TimerInvokeType.BulletTotalTime)]
    public class BulletTickOver_TimerHandler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent t)
        {
            t.TimeOver();
        }
    }




    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public static class BulletComponentSystem
    {

        public static void Tick(this BulletComponent self)
        {
            Unit selfUnit = self.GetParent<Unit>();
            Unit owner = self.GetOwner();
            if (owner == null || owner.IsDisposed)
            {
                self.Dispose();
                return;
            }
            BulletConfig bulletConfig = self.Config;
            self.SelectTarget();
            if (self.Target.Count <= 0)
            {
                return;
            }
            self.TickCount++;
            if (bulletConfig.TickCastId.Length > 0)
            {
                foreach (var tickCastId in bulletConfig.TickCastId)
                {
                    Cast cast = owner.CreateCast(tickCastId);
                    Log.Console($"子弹：{self.ConfigId}结算间隔触发，CreateCast，castid: {tickCastId}");
                    cast.Targets.AddRange(self.Target);
                    int err = cast.Cast();
                    if (err != ErrorCode.ERR_Success)
                    {
                        Log.Console($"子弹 {self.ConfigId} 释放Cast {tickCastId} 失败:{err}");
                    }

                }

            }

            if (bulletConfig.TickAction.Length > 0)
            {
                foreach (var actionsId in bulletConfig.TickAction)
                {
                    self.CreateActions(actionsId, selfUnit, selfUnit, ActionsRunType.BulletTick);
                }
            }

            if (bulletConfig.TickLimit > 0 && self.TickCount >= bulletConfig.TickLimit)
            {
                // 结算次数到了,提前结束
                Log.Console($"子弹：{self.ConfigId}结算间隔结束");
                self.TimeOver();
            }



        }

        public static void Tick1(this BulletComponent self)
        {
            BulletConfig bulletConfig = self.Config;
            self.SelectTarget();
            foreach (var actionsId in bulletConfig.Tick1)
            {
                self.CreateActions(actionsId, self.GetParent<Unit>(), self.GetParent<Unit>(), ActionsRunType.BulletTick);
            }
        }


        public static void Tick2(this BulletComponent self)
        {
            BulletConfig bulletConfig = self.Config;
            self.SelectTarget();
            foreach (var actionsId in bulletConfig.Tick2)
            {
                Log.Console($"子弹：{self.ConfigId}每秒结算触发，创建action，actionid:{actionsId}");
                self.CreateActions(actionsId, self.GetParent<Unit>(), self.GetParent<Unit>(), ActionsRunType.BulletTick);
            }
        }

        public static void TimeOver(this BulletComponent self)
        {
            Log.Console($"777 bullet time over,,,bulletid:{self.Config.Id}");
            TimerComponent.Instance.Remove(ref self.TickTimer);
            TimerComponent.Instance.Remove(ref self.TickTimer2);
            TimerComponent.Instance.Remove(ref self.TickTimer3);
            TimerComponent.Instance.Remove(ref self.TotalTimer);
            Unit owner = self.GetOwner();
            if (owner == null || owner.IsDisposed)
            {
                self.DoDispose();
                return;
            }

            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.DestroyAction.Length > 0)
            {
                foreach (var actionsId in bulletConfig.DestroyAction)
                {
                    self.CreateActions(actionsId, self.GetParent<Unit>(), self.GetParent<Unit>(), ActionsRunType.BulletDestroy);
                }

            }

            self.DoDispose();
        }


        public static void SelectTarget(this BulletComponent self)
        {
            self.Target.Clear();
            Unit selfUnit = self.GetParent<Unit>();
            Unit owner = self.GetOwner();
            BulletConfig bulletConfig = self.Config;
            switch (bulletConfig.Shape)
            {
                case 1:
                    {
                        int range = int.Parse(bulletConfig.ShapeParam[0]);
                        foreach (AOIEntity aoiEntity in selfUnit.GetBeSeeUnits().Values)
                        {
                            Unit unit = aoiEntity.GetParent<Unit>();
                            if (unit.Type != UnitType.Player && unit.Type != UnitType.Monster)
                            {
                                continue;
                            }

                            if (unit == owner)
                            {
                                continue;
                            }

                            if (math.length(unit.Position - selfUnit.Position) < range)
                            {
                                self.Target.Add(unit.Id);
                            }

                        }

                    }
                    break;
                default:
                    throw new Exception($"not such BulletConfig Shape: {bulletConfig.Shape}");
            }
        }
        public static void DoDispose(this BulletComponent self)
        {
            self.GetParent<Unit>().Stop(0);
            self.DomainScene().GetComponent<UnitComponent>().Remove(self.Parent.Id);
        }


        public static Unit GetOwner(this BulletComponent self)
        {
            return self.DomainScene().GetComponent<UnitComponent>().Get(self.OwnerId);
        }


        public static void PreDestroy(this BulletComponent self)
        {

            TimerComponent.Instance.Remove(ref self.TickTimer);

            Unit owner = self.GetOwner();
            if (owner == null)
            {
                return;
            }

            BulletConfig bulletConfig = self.Config;

            if (bulletConfig.DestroyAction.Length == 0)
            {
                return;
            }

            foreach (int actionsId in bulletConfig.DestroyAction)
            {
                self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletDestroy);
            }



        }

        public static void Start(this BulletComponent self)
        {
            Unit owner = self.GetOwner();
            if (owner == null)
            {
                self.Dispose();
                return;
            }

            Log.Console($"->子弹 {self.ConfigId} Tick");
            BulletConfig bulletConfig = self.Config;

            if (bulletConfig.AwakeAction.Length != 0)
            {
                foreach (var actionsId in bulletConfig.AwakeAction)
                {
                    self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletAwake);
                }

            }

            if (bulletConfig.Interval > 0)
            {
                int Interval = bulletConfig.Interval;
                if (Interval <= 100)
                {
                    Interval = 100;
                }

                self.TickTimer = TimerComponent.Instance.NewRepeatedTimer(Interval, TimerInvokeType.BulletTick, self);
            }

            if (bulletConfig.Tick1.Length > 0)
            {
                self.TickTimer2 = TimerComponent.Instance.NewRepeatedTimer(100, TimerInvokeType.BulletTick2, self);
            }

            if (bulletConfig.Tick2.Length > 0)
            {
                self.TickTimer3 = TimerComponent.Instance.NewRepeatedTimer(1000, TimerInvokeType.BulletTick3, self);
            }

            if (bulletConfig.TotalTime > 0)
            {
                self.TotalTimer = TimerComponent.Instance.NewOnceTimer(
                    TimeHelper.ServerNow() + bulletConfig.TotalTime,
                    TimerInvokeType.BulletTotalTime,
                    self
                    );
            }
        }

    }
}
