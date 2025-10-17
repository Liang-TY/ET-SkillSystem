
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
        }
    }


    public class BulletComponentDestroySystem : DestroySystem<BulletComponent>
    {
        protected override void Destroy(BulletComponent self)
        {
            self.PreDestroy();
            self.ConfigId = default;
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









    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]

    public static class BulletComponentSystem
    {

        public static void Tick(this BulletComponent self)
        {
            Unit selfUnit = self.GetParent<Unit>();
            Unit owner = self.GetOwner();
            if (owner == null)
            {
                self.Dispose();
                return;
            }
            Log.Console($"->子弹 {self.ConfigId} Tick");
            BulletConfig bulletConfig = self.Config;
            using (ListComponent<Unit> list = ListComponent<Unit>.Create())
            {
                switch (bulletConfig.Shape)
                {
                    case 1://选择身边一定范围内的一个人
                        int range = int.Parse(bulletConfig.ShapeParam[0]);


                        foreach(AOIEntity aoiEntity in selfUnit.GetBeSeePlayers().Values)
                        {
                            Unit unit = aoiEntity.GetParent<Unit>();
                            if (unit == owner)
                            {
                                //不选自己
                                continue;
                            }
                            if (math.length(unit.Position - selfUnit.Position) < range)
                            {
                                list.Add(unit);
                            }

                        }
                        break;
                    default:
                        throw new Exception($"not such Bulletconfig shape: {bulletConfig.Shape}");
                }


                if(list.Count > 0)
                {
                    foreach (var unit in list)
                    {
                        if (bulletConfig.TickCastId.Length > 0)
                        {
                            foreach (var tickCastId in bulletConfig.TickCastId)
                            {
                                owner.CreateAndCast(tickCastId);
                            }
                        }
                        if(bulletConfig.TickAction.Length > 0){
                            foreach (var actionsId in bulletConfig.TickAction)
                            {
                                self.CreateActions(actionsId, unit, self.GetOwner(), ActionsRunType.BulletTick);
                            }
                        }
                    }
                }


                
                    
               




            }

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
                    
                self.TickTimer = TimerComponent.Instance.NewRepeatedTimer(Interval,TimerInvokeType.BulletTick, self);
            } 
        }

    }
}
