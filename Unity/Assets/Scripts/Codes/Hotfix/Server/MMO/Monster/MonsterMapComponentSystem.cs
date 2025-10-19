using ET.Client;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET.Server
{
    public class MonsterFlagAwakeSystem:AwakeSystem<MonsterFlag, int, int>
    {
        protected override void Awake(MonsterFlag self, int id, int group)
        {
            self.ConfigId = id;
            self.GroupConfigId = group;
        }
    }

    public class MonsterFlagDestroySystem : DestroySystem<MonsterFlag>
    {
        protected override void Destroy(MonsterFlag self)
        {
            self.DomainScene().GetComponent<MonsterMapComponent>().OnMonsterDead(self.ConfigId,self.GroupConfigId);
        }
    }


    public class CreateMonsterInfoAwakeSystem : AwakeSystem<CreateMonsterInfo, int>
    {
        protected override void Awake(CreateMonsterInfo self, int id)
        {
            self.monsterId = id;
        }
    }


    public class MonsterMapComponentAwakeSystem : AwakeSystem<MonsterMapComponent>
    {
        protected override void Awake(MonsterMapComponent self)
        {
            foreach (var monsterId in MonsterConfigCategory.Instance.GetAll().Keys)
            {
                self.CreateMonster(monsterId);
            }
        }
    }


    [Invoke(TimerInvokeType.CreateMonster)]
    [FriendOfAttribute(typeof(ET.Server.CreateMonsterInfo))]
    public class CreateMonster_TimerHandler : ATimer<CreateMonsterInfo>
    {
        protected override void Run(CreateMonsterInfo t)
        {
            t.GetParent<MonsterMapComponent>().CreateMonster(t.monsterId);
        }

    }

    [Invoke(TimerInvokeType.MonsterDead)]
    public class MonsterDead_TimerHandler : ATimer<Unit>
    {
        protected override void Run(Unit t)
        {
            t?.Dispose();
        }

    }



    public static class MonsterMapComponentSystem
    {

        public static void OnMonsterDead(this MonsterMapComponent self ,int id, int groupId)
        {
            TimerComponent.Instance.NewOnceTimer(
                TimeHelper.ServerNow() + 3000,
                TimerInvokeType.CreateMonster,
                self.AddChild<CreateMonsterInfo,int>(id));
        }


        public static Unit CreateMonster(this MonsterMapComponent self,int id)
        {
            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(id);
            MonsterGroupConfig groupConfig = MonsterGroupConfigCategory.Instance.Get(monsterConfig.GroupId);
            int h_range = groupConfig.Range / 2;
            float3 pos =  new float3(groupConfig.PosX, groupConfig.PosY, groupConfig.PosZ) +
            new float3(RandomGenerator.RandomNumber(-h_range, h_range), 0, RandomGenerator.RandomNumber(-h_range, h_range));

            Unit unit = UnitFactory.CreateMonster(self.DomainScene(), monsterConfig.UnitConfigId, pos);
            unit.AddComponent<MonsterFlag, int, int>(id, monsterConfig.GroupId);
            return unit;
        
        
        
        }



    }
}
