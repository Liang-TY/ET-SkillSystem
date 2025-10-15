using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class BuffcomponentAwakeSystem : AwakeSystem<BuffComponent>
    {
        protected override void Awake(BuffComponent self)
        {
            self.AddComponent<BuffTempComponent>();
        }
    }


    public class BuffComponentDestroySystem : DestroySystem<BuffComponent>
    {
        protected override void Destroy(BuffComponent self)
        {
            self.ConfigIdBuffs.Clear();
        }
    }


    [FriendOfAttribute(typeof(ET.Server.Buff))]
    public class BuffComponentDeserializeSystem : DeserializeSystem<BuffComponent>
    {
        protected override void Deserialize(BuffComponent self)
        {
            self.AddComponent<BuffTempComponent>();
            foreach (Buff buff in self.Children.Values)
            {
                self.ConfigIdBuffs.Add(buff.ConfigId,buff);
            }

        }
    }




    [FriendOfAttribute(typeof(ET.Server.BuffCreateInfo))]
    [FriendOfAttribute(typeof(ET.Server.Buff))]
    [FriendOfAttribute(typeof(ET.Server.BuffComponent))]
    public static class BuffcomponentSystem
    {
        public static BuffCreateInfo Create(this BuffComponent self, int configId)
        {
            return self.GetComponent<BuffTempComponent>().AddChild<BuffCreateInfo, int>(configId);
        }

        public static bool CreateAndAdd(this BuffComponent self, int configId)
        {
            using (BuffCreateInfo buffcreateInfo = self.Create(configId))
            {
                return self.Add(buffcreateInfo);
            }

        }



        public static bool Add(this BuffComponent self, BuffCreateInfo buffCreateInfo)
        {

            if (buffCreateInfo == null ||  buffCreateInfo.IsDisposed){ 
                return false;
            }

            if (self == null || self.IsDisposed){
                return false;
            }

            Buff buff = self.AddChild<Buff, int>(buffCreateInfo.ConfigId);
            buff.Owner = self.GetParent<Unit>();
            if(buff.Owner == null){
                buff.Dispose();
                return false;
            }

            int configId = buff.ConfigId;
            if (self.ConfigIdBuffs.ContainsKey(configId))
            {
                //已有相同buff，这里暂定直接顶掉
                self.Remove(self.ConfigIdBuffs[configId].Id);
            }


            self.ConfigIdBuffs.Add(configId, buff);
            if((NoticeClientType)buff.Config.NoticeClientType != NoticeClientType.NoNotice)
            {
                M2C_BuffAdd m2CBuffAdd = new M2C_BuffAdd(){ UnitId = buff.Owner.Id, BuffData = buff.ToBuffAddProto() };
                MMOMessageHelper.SendClient(buff.Owner, m2CBuffAdd, (NoticeClientType)buff.Config.NoticeClientType);
            }

            //todo 触发创建Buff时的行为实体逻辑

            buff.AddActions();


            return true;
        }

        public static void Remove(this BuffComponent self, long buffId)
        {
            if (!self.Children.TryGetValue(buffId, out Entity entity))
            {
                return;
            }
            Buff buff = entity as Buff;
            try
            {
                self.ConfigIdBuffs.Remove(buff.ConfigId);

                if ((NoticeClientType)buff.Config.NoticeClientType != NoticeClientType.NoNotice)
                {
                    M2C_BuffRemove m2CBuffRemove = new M2C_BuffRemove(){ BuffId = buff.Id, UnitId = buff.Owner.Id };
                    MMOMessageHelper.SendClient(buff.Owner, m2CBuffRemove, (NoticeClientType)buff.Config.NoticeClientType);
                }

                //todo 触发移除Buff时的行为实体逻辑
                buff.RemoveActions();

                buff.Dispose();
            }
            catch (Exception e)
            {

                Log.Error($"Buff Remove’ error! buffcompID: {self.Id}, buffID: {buff.Id}, buffconfigID: {buff.Config?.Id ?? 0}, {e}");
            }

        }






    }
}
