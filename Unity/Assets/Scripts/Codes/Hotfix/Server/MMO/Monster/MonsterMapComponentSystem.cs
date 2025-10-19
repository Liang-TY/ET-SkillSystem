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
    public static class MonsterMapComponentSystem
    {
        public static Unit CreateMonster(this MonsterMapComponent self,int id)
        {
            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(id);
            MonsterGroupConfig groupConfig = MonsterGroupConfigCategory.Instance.Get(monsterConfig.GroupId);
            int h_range = groupConfig.Range / 2;
            float3 pos =  new float3(groupConfig.PosX, groupConfig.PosY, groupConfig.Posz) +
            new float3(RandomGenerator.RandomNumber(-h_range, h_range), 0, RandomGenerator.RandomNumber(-h_range, h_range));

            Unit unit = UnitFactory.CreateMonster(self.DomainScene(), monsterConfig.UnitConfigId, pos);
            unit.AddComponent<MonsterFlag, int, int>(id, monsterConfig.GroupId);
            return unit;
        
        
        
        }

    }
}
