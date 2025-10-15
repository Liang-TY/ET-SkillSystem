using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class BuffCreateInfoAwakeSystem : AwakeSystem<BuffCreateInfo, int>
    {
        protected override void Awake(BuffCreateInfo self, int ConfigId)
        {
            self.ConfigId = ConfigId;
        }

    }
    public class BuffCreateInfoDestroySystem : DestroySystem<BuffCreateInfo>
    {
        protected override void Destroy(BuffCreateInfo self)
        {

        }

    }
    public class BuffAwakeSystem : AwakeSystem<Buff, int>
    {
        protected override void Awake(Buff self, int ConfigId)
        {
            self.ConfigId = ConfigId;
            self.AddComponent<ActionsTempComponent>();
            self.CreateTime = TimeHelper.ServerNow();
        }

    }

    public class BuffDeserializeSystem : DeserializeSystem<Buff>
    {
        protected override void Deserialize(Buff self)
        {
            self.AddComponent<ActionsTempComponent>();
            self.Owner = self.Parent.GetParent<Unit>();
        }

    }

    public class BuffDestroySystem : DestroySystem<Buff>
    {
        protected override void Destroy(Buff self)
        {
            self.ConfigId = default;
            self.Owner = default;
            self.CreateTime = default;
            self.TickTime = default;
            self.TickBeginTime = default;

            TimerComponent.Instance.Remove(ref self.TickTimer);
            TimerComponent.Instance.Remove(ref self.WaitTickTimer);

            self.ExpireTime = default;
            TimerComponent.Instance.Remove(ref self.ExpireTimer);

        }

    }
    [FriendOfAttribute(typeof(ET.Server.Buff))]
    public static class BuffSystem
    {
        public static void AddActions(this Buff buff)
        {
            long instanceId = buff.InstanceId;
            foreach (int i in buff.Config.AddAction)
            {
                try
                {
                    buff.CreateActions(i, ActionsRunType.BuffAdd);
                    //可能在效果的过程中，本buff被移除回池了，然后又从池里取出来了，所以如果只判断IsDisposed是不够的的!
                    if (buff.InstanceId != instanceId)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {

                    Log.Error($"AddActions error, ownerID: {buff.Owner?.Id ?? 0} buffiD: {buff.Id}, buffConfig: {buff.Config.Id} Actions: {i} {e}");
                }
            }

        }



        public static void RemoveActions(this Buff buff)
        {
            long instanceId = buff.InstanceId;
            foreach (int i in buff.Config.RemoveAction)
            {
                try
                {
                    buff.CreateActions(i, ActionsRunType.BuffRemove);
                    //可能在效果的过程中，本buff被移除回池了，然后又从池里取出来了，所以如果只判断IsDisposed是不够的的!
                    if (buff.InstanceId != instanceId)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"RemoveActions error, ownerID: {buff.Owner?.Id ?? 0} buffID: {buff.Id}, buffConfig: {buff.Config.Id} Actions: {i} {e}");
                }
            }

        }


        public static BuffProto ToBuffAddProto(this Buff buff)
        {
            BuffProto buffProto = new BuffProto()
            {
                Id = buff.Id,
                ConfigId = buff.ConfigId,
                CreateTime = buff.CreateTime,
                ExpireTime = buff.ExpireTime
            };
            //如果有额外的数据，可以走这里，例如buff上的组件
            //buffProto.ExtraData
            return buffProto;
        }

        public static void TickActions(this Buff buff)
        {
            if (buff.IsDisposed)
            {
                return;
            }
            long instanceId = buff.InstanceId;
            foreach (int i in buff.Config.TickAction)
            {
                try
                {
                    buff.CreateActions(i,ActionsRunType.BuffTick);
                    if (buff.InstanceId != instanceId)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {

                    if (instanceId == buff.InstanceId)
                    {
                        Log.Error(
                            $"TickActions error, ownerID: {buff.Owner?.Id ?? 0} buffID: {buff.Id}, buffconfigID: {buff.Config.Id} Actions: {i} {e}");
                    }
                    else
                    {
                        Log.Error($"TickActions error, ownerID: {buff.Owner?.Id ?? 0} buffID: {buff.Id}, Actions: {i} {e} ");
                    }
                }
            }

            if (buff.InstanceId != instanceId)
            {
                return;
            }

            if (buff.Config.TickAction.Length > 0)
            {
                if (buff.Owner == null)
                {
                    return;
                }

                M2C_BuffTick m2C_BuffTick = new M2C_BuffTick() 
                { 
                    BuffId = buff.Id,
                    UnitId = buff.Owner.Id,
                };

                MMOMessageHelper.SendClient(buff.Owner, m2C_BuffTick, (NoticeClientType)buff.Config.NoticeClientType);
            }




        }







        }



}
