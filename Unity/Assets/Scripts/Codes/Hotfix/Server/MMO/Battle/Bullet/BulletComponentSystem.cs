
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        }
    }









    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]

    public static class BulletComponentSystem
    {
        public static Unit Getowner(this BulletComponent self)
        {
            return self.DomainScene().GetComponent<UnitComponent>().Get(self.OwnerId);
        }


        public static void PreDestroy(this BulletComponent self)
        {

            TimerComponent.Instance.Remove(ref self.TickTimer);

            Unit owner = self.Getowner();
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
            Unit owner = self.Getowner();
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
